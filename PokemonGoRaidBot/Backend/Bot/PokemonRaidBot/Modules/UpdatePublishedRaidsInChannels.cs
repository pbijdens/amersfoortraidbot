using Botje.Core;
using Botje.Core.Utils;
using Botje.DB;
using Botje.Messaging;
using Ninject;
using RaidBot.Backend.Bot.PokemonRaidBot.Entities;
using RaidBot.Backend.Bot.PokemonRaidBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RaidBot.Backend.Bot.PokemonRaidBot.Modules
{
    public class UpdatePublishedRaidsInChannels : IBotModule
    {
        private static Random rnd = new Random();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private ILogger _log;

        /// <summary></summary>
        [Inject]
        public IMessagingClient Client { get; set; }

        /// <summary></summary>
        [Inject]
        public IDatabase DB { get; set; }

        /// <summary></summary>
        [Inject]
        public ILoggerFactory LoggerFactory { set { _log = value.Create(GetType()); } }

        [Inject]
        public ISettingsManager Settings { get; set; }

        [Inject]
        public RaidEventHandler RaidEventHandler { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        [Inject]
        public ITimeService TimeService { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            _cts.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Startup()
        {
            _cts = new CancellationTokenSource();

            var targets = (SettingsManager.PogoAfoMappings ?? new List<PogoAfoMapping>()).SelectMany(x => (x.Targets ?? new List<RaidTarget>())).ToArray();

            foreach (var target in targets)
            {
                Thread thr = new Thread(() => ThreadFunc(target))
                {
                    IsBackground = true
                };
                thr.Start();
            }
        }

        private void ThreadFunc(RaidTarget target)
        {
            _log.Info($"*** Starting channel update thread for {target.Description}");

            int[] delays = new int[] { 500, 1000, 5000, 3000 };
            int actionDelayIndex = 0;

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var raidParticipationCollection = DB.GetCollection<RaidParticipation>();
                    // find all raids that end in the future, and that will unlock within the next hour (or are already unlocked)
                    var allActiveRaids = raidParticipationCollection.Find(x =>
                        x.Raid.RaidEndTime >= DateTime.UtcNow
                        && DateTime.UtcNow + TimeSpan.FromHours(1) >= x.Raid.RaidUnlockTime
                        && x.Raid.Publications != null
                        && x.Raid.Publications.Any(p => p.ChannelID == target.ChannelID && p.TelegramMessageID != -1)).ToList();

                    // find new entries that still need to be posted
                    var firstRaidThatNeedsPosting = allActiveRaids.Where(x => x.Raid.Publications
                        .Any(p => p.ChannelID == target.ChannelID && p.TelegramMessageID == default(long)))
                        .OrderBy(x => x.Raid.RaidUnlockTime).RandomOrDefault();
                    if (null != firstRaidThatNeedsPosting)
                    {
                        PostRaid(target, raidParticipationCollection, firstRaidThatNeedsPosting);
                        actionDelayIndex++;
                    }
                    else
                    {
                        var firstRaidThatNeedsUpdating = allActiveRaids.Where(x => x.Raid.Publications.Any(p => p.ChannelID == target.ChannelID && p.LastModificationTimeUTC < x.LastModificationTime)).OrderBy(x => x.LastModificationTime).RandomOrDefault();
                        if (null != firstRaidThatNeedsUpdating)
                        {
                            UpdateRaid(target, raidParticipationCollection, firstRaidThatNeedsUpdating);
                            actionDelayIndex++;
                        }
                        else
                        {
                            // Raid has ended, but there still is a telegram message for it
                            var firstRaidThatNeedsToBeDeleted = raidParticipationCollection.Find(x => x.Raid.RaidEndTime < DateTime.UtcNow && x.Raid.Publications != null && x.Raid.Publications.Any(p => p.ChannelID == target.ChannelID && p.TelegramMessageID != default(long) && p.TelegramMessageID != -1)).RandomOrDefault();
                            if (null != firstRaidThatNeedsToBeDeleted)
                            {
                                DeleteRaid(target, raidParticipationCollection, firstRaidThatNeedsToBeDeleted);
                                actionDelayIndex++;
                            }
                            else
                            {
                                actionDelayIndex = 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"Something went wrong and we first caught it at top-level. Ignoring the problem.");
                }
                finally
                {
                    int delay = delays[Math.Min(delays.Length - 1, actionDelayIndex)];
                    Thread.Sleep(TimeSpan.FromMilliseconds(delay));
                }
            }
        }

        public static object SharedUpdateLock = new object();

        private void PostRaid(RaidTarget target, DbSet<RaidParticipation> coll, RaidParticipation raid)
        {
            _log.Info($"Posting raid {raid.PublicID} / {raid.Raid.Raid} / {raid.Raid.Gym} in channel {target.Description}");

            lock (SharedUpdateLock)
            {
                // Mark the raid as updated with posting in progress, by setting the telegram message ID to -1
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                var pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
                if (null == pe) return;
                raid.Raid.Publications.RemoveAll(x => x != pe && x.ChannelID == pe.ChannelID);
                pe.TelegramMessageID = -1; // hardcoded to indicate the message was not sent
                pe.LastModificationTimeUTC = DateTime.UtcNow;
                coll.Update(raid);
            }

            // Next post the message, this may take some time if we managed to hit the rate limit
            string url = $"{SettingsManager.BotAddress}raids/{raid.PublicID}?r={rnd.Next()}";
            string messageText = $"[{TimeService.AsShortTime(raid.Raid.RaidUnlockTime)}-{TimeService.AsShortTime(raid.Raid.RaidEndTime)}] {MessageUtils.HtmlEscape(raid.Raid.Raid)} @ {MessageUtils.HtmlEscape(raid.Raid.Gym)} ({raid.NumberOfParticipants()})\n<a href=\"{url}\">Klik hier om in te schrijven.</a>";

            try
            {
                var message = Client.SendMessageToChat(target.ChannelID, messageText, "HTML", true, true);
                long messageID = message?.MessageID ?? default(long);

                lock (SharedUpdateLock)
                {
                    // The record may have changed by now, so load it again, then update tge nessage ID
                    raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                    var pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
                    if (null == pe) return;
                    raid.Raid.Publications.RemoveAll(x => x != pe && x.ChannelID == pe.ChannelID);
                    pe.TelegramMessageID = messageID;
                    coll.Update(raid);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to post new raid message, marking as 'new'.");
                lock (SharedUpdateLock)
                {
                    // The record may have changed by now, so load it again, then update tge nessage ID
                    raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                    var pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
                    if (null == pe) return;
                    raid.Raid.Publications.RemoveAll(x => x != pe && x.ChannelID == pe.ChannelID);
                    pe.TelegramMessageID = 0;
                    pe.LastModificationTimeUTC = DateTime.MinValue;
                    coll.Update(raid);
                }
            }
        }

        private void UpdateRaid(RaidTarget target, DbSet<RaidParticipation> coll, RaidParticipation raid)
        {
            _log.Info($"Updating raid {raid.PublicID} / {raid.Raid.Raid} / {raid.Raid.Gym} in channel {target.Description}");

            lock (SharedUpdateLock)
            {
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                var pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
                if (null == pe) return;
                raid.Raid.Publications.RemoveAll(x => x != pe && x.ChannelID == pe.ChannelID);
                if (pe.TelegramMessageID != -1)
                {
                    pe.LastModificationTimeUTC = DateTime.UtcNow;
                    coll.Update(raid);
                }
                else
                {
                    return;
                }
            }

            string url = $"{SettingsManager.BotAddress}raids/{raid.PublicID}?r={rnd.Next()}";
            string messageText = $"[{TimeService.AsShortTime(raid.Raid.RaidUnlockTime)}-{TimeService.AsShortTime(raid.Raid.RaidEndTime)}] {MessageUtils.HtmlEscape(raid.Raid.Raid)} @ {MessageUtils.HtmlEscape(raid.Raid.Gym)} ({raid.NumberOfParticipants()})\n<a href=\"{url}\">Klik hier om in te schrijven.</a>";

            lock (SharedUpdateLock)
            {
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                var pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
                if (null == pe) return;
                // We use -1 as a constant to indicate the message was not sent yet, or was delayed.
                if (pe.TelegramMessageID != -1)
                {
                    try
                    {
                        Client.EditMessageText($"{pe.ChannelID}", pe.TelegramMessageID, null, messageText, "HTML", true, null, "channel");
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Failed to post new raid message.");

                        raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                        pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
                        if (null == pe) return;
                        raid.Raid.Publications.RemoveAll(x => x != pe && x.ChannelID == pe.ChannelID);

                        pe.LastModificationTimeUTC = raid.LastModificationTime - TimeSpan.FromMilliseconds(1);
                        coll.Update(raid);
                    }
                }
            }
        }

        private void DeleteRaid(RaidTarget target, DbSet<RaidParticipation> coll, RaidParticipation raid)
        {
            _log.Info($"Deleting raid {raid.PublicID} / {raid.Raid.Raid} / {raid.Raid.Gym} from channel {target.Description}");

            var pe = raid.Raid.Publications.Where(x => x.ChannelID == target.ChannelID).FirstOrDefault();
            if (null != pe && pe.TelegramMessageID != -1)
            {
                Client.DeleteMessage(target.ChannelID, pe.TelegramMessageID);
            }
            else
            {
                return; // nothing happened.
            }

            // Only update the database when DELETE succeeded
            lock (SharedUpdateLock)
            {
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                raid.Raid.Publications.RemoveAll(x => x.ChannelID == pe.ChannelID);
                coll.Update(raid);
            }
        }
    }

    public static class LocalExtensions
    {
        private static Random _rnd = new Random();

        public static T RandomOrDefault<T>(this IEnumerable<T> coll)
        {
            if (null == coll) return default(T);
            if (!coll.Any()) return default(T);

            int index = _rnd.Next(coll.Count());
            return coll.ToArray()[index];
        }
    }
}
