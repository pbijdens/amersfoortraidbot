using Botje.Core;
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
    public class UpdatePublishedRaidsInPrimaryChannel : IBotModule
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

            Thread thr = new Thread(() => ThreadFunc(new RaidTarget
            {
                ChannelID = Settings.PublicationChannel ?? 0,
                Description = "Primary Publication Channel",
                Levels = new List<int> { 5 },
            }))
            {
                IsBackground = true
            };
            thr.Start();
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
                    var raid = raidParticipationCollection.Find(x =>
                        x.Raid.RaidEndTime >= DateTime.UtcNow
                        && DateTime.UtcNow + TimeSpan.FromHours(1) >= x.Raid.RaidUnlockTime
                        && x.IsPublished
                        && (x.Raid.TelegramMessageID == null || x.Raid.TelegramMessageID == 0 || x.Raid.TelegramMessageID == -1)).ToList().RandomOrDefault();
                    if (null != raid)
                    {
                        PostRaid(target, raidParticipationCollection, raid);
                        actionDelayIndex++;
                    }
                    else
                    {
                        raid = raidParticipationCollection.Find(x => x.Raid.RaidEndTime >= DateTime.UtcNow
                            && DateTime.UtcNow + TimeSpan.FromHours(1) >= x.Raid.RaidUnlockTime
                            && x.IsPublished
                            && x.LastRefresh < x.LastModificationTime
                            && (x.Raid.TelegramMessageID != null && x.Raid.TelegramMessageID != 0 && x.Raid.TelegramMessageID != -1)).ToList().RandomOrDefault();
                        if (null != raid)
                        {
                            UpdateRaid(target, raidParticipationCollection, raid);
                            actionDelayIndex++;
                        }
                        else
                        {
                            raid = raidParticipationCollection.Find(x => x.Raid.RaidEndTime < DateTime.UtcNow
                                    && x.IsPublished
                                    && (x.Raid.TelegramMessageID != null && x.Raid.TelegramMessageID != 0 && x.Raid.TelegramMessageID != -1)).ToList().RandomOrDefault();
                            if (null != raid)
                            {
                                DeleteRaid(target, raidParticipationCollection, raid);
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

        private void PostRaid(RaidTarget target, DbSet<RaidParticipation> coll, RaidParticipation raid)
        {
            _log.Info($"Posting raid {raid.PublicID} / {raid.Raid.Raid} / {raid.Raid.Gym} in channel {target.Description}");

            lock (UpdatePublishedRaidsInChannels.SharedUpdateLock)
            {
                // Mark the raid as updated with posting in progress, by setting the telegram message ID to -1
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                raid.Raid.TelegramMessageID = -1; // hardcoded to indicate the message was not sent
                coll.Update(raid);
            }

            try
            {
                var message = RaidEventHandler.ShareRaidToChat(raid, target.ChannelID);
                long messageID = message?.MessageID ?? default(long);

                lock (UpdatePublishedRaidsInChannels.SharedUpdateLock)
                {
                    // The record may have changed by now, so load it again, then update tge nessage ID
                    raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                    raid.Raid.TelegramMessageID = messageID;
                    coll.Update(raid);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to post new raid message, marking as 'new'.");
                lock (UpdatePublishedRaidsInChannels.SharedUpdateLock)
                {
                    // error publishing, assume it wasn't published and try again
                    raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                    raid.Raid.TelegramMessageID = 0;
                    coll.Update(raid);
                }
            }
        }

        private void UpdateRaid(RaidTarget target, DbSet<RaidParticipation> coll, RaidParticipation raid)
        {
            _log.Info($"Updating raid {raid.PublicID} / {raid.Raid.Raid} / {raid.Raid.Gym} in channel {target.Description}");

            lock (UpdatePublishedRaidsInChannels.SharedUpdateLock)
            {
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                if (raid.Raid.TelegramMessageID != null && raid.Raid.TelegramMessageID != 0 && raid.Raid.TelegramMessageID != -1)
                {
                    raid.LastRefresh = DateTime.UtcNow;
                    coll.Update(raid);
                }
                else
                {
                    return;
                }
            }

            try
            {
                RaidEventHandler.UpdateRaidMessage(target.ChannelID, raid.Raid.TelegramMessageID, null, raid.PublicID, "channel");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to update raid message, marking as modified.");
                lock (UpdatePublishedRaidsInChannels.SharedUpdateLock)
                {
                    // error publishing, assume it wasn't updated and try again
                    raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                    raid.LastModificationTime = DateTime.UtcNow;
                    coll.Update(raid);
                }
            }
        }

        private void DeleteRaid(RaidTarget target, DbSet<RaidParticipation> coll, RaidParticipation raid)
        {
            _log.Info($"Deleting raid {raid.PublicID} / {raid.Raid.Raid} / {raid.Raid.Gym} from channel {target.Description}");

            if (raid.Raid.TelegramMessageID != null && raid.Raid.TelegramMessageID != 0 && raid.Raid.TelegramMessageID != -1)
            {
                Client.DeleteMessage(target.ChannelID, raid.Raid.TelegramMessageID ?? 0);
            }
            else
            {
                return; // nothing happened.
            }

            // Only update the database when DELETE succeeded
            lock (UpdatePublishedRaidsInChannels.SharedUpdateLock)
            {
                raid = coll.Find(x => x.UniqueID == raid.UniqueID).FirstOrDefault();
                raid.Raid.TelegramMessageID = 0;
                coll.Update(raid);
            }
        }
    }
}
