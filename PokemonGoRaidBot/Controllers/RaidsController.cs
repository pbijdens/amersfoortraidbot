using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ninject;
using RaidBot.Backend.API;
using RaidBot.Backend.Bot;
using RaidBot.Backend.Bot.PokemonRaidBot.Entities;
using RaidBot.Backend.DB;
using RaidBot.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace RaidBot.Controllers
{
    [Route("api/raids")]
    public class RaidsController : ControllerBase
    {
        private IPokemonRaidBotHost _botHost;

        public RaidsController(ApplicationDbContext dbContext, ILoggerFactory loggerFactory, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration, IPokemonRaidBotHost botHost)
            : base(dbContext, loggerFactory, roleManager, userManager, configuration)
        {
            _botHost = botHost;
        }

        [Authorize]
        [Audit]
        [JsonWrapper]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveRaids()
        {
            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var coll = db.GetCollection<RaidParticipation>();
            var raids = coll.Find(x => x.Raid != null && x.Raid.RaidEndTime >= DateTime.UtcNow && DateTime.UtcNow + TimeSpan.FromHours(1) >= x.Raid.RaidUnlockTime).OrderBy(x => x.Raid.RaidEndTime).Select(x => x.Adapt<McRaidDescription>());

            return new JsonResult(await Task.FromResult(raids), JsonSettings);
        }

        [Authorize]
        [Audit]
        [JsonWrapper]
        [HttpGet("raid")]
        public async Task<IActionResult> GetRaid([FromQuery] string id)
        {
            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var coll = db.GetCollection<RaidParticipation>();
            var raid = coll.Find(x => x.PublicID == id).SingleOrDefault();

            if (null == raid) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");

            var result = raid.Adapt<McRaidDetails>();
            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        [Authorize]
        [Audit()]
        [JsonWrapper]
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromQuery] string id, [FromQuery] string when, [FromQuery] int extra)
        {
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            if (LoggedInUser == null || LoggedInUser.TelegramUserID == 0) return JsonError(System.Net.HttpStatusCode.BadRequest, "Registreer eerst bij de bot", "Registreer eerst bij de bot");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var logger = _botHost.Kernel.Get<Botje.Core.ILoggerFactory>().Create(typeof(RaidsController));
            var raidParticipationCollection = db.GetCollection<RaidParticipation>();

            var raid = raidParticipationCollection.Find(x => x.PublicID == id).SingleOrDefault();
            if (null == raid) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");

            Team team = Team.Unknown;
            int level = -1;
            Botje.Messaging.Models.User user = GetTgUserForLoggedInUser(db, ref team, ref level);

            RemoveAnyAndAllCurrentParticipations(raid, user);

            var participation = new UserParticipation { User = user };
            participation.Extra = extra;

            if (DateTime.TryParse(when ?? "", out DateTime whenAsDateTime))
            {
                participation.UtcWhen = whenAsDateTime.ToUniversalTime();
            }

            if (!raid.Participants[team].Contains(participation))
            {
                raid.Participants[team].Add(participation);
                raid.Participants[team].Sort((x, y) => string.Compare(x.User.DisplayName(), y.User.DisplayName()));
            }

            raid.LastModificationTime = DateTime.UtcNow;

            raidParticipationCollection.Update(raid);

            logger.Info($"Got subscription for raid {raid.Raid.Raid} ({id}) at {raid.Raid.Gym} for {user.UsernameOrName()} (level: {level}, team: {team}) @ {when}, with {extra} extra players");

            var result = raid.Adapt<McRaidDetails>();

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        [Authorize]
        [Audit()]
        [JsonWrapper]
        [HttpPost("maybe")]
        public async Task<IActionResult> Maybe([FromQuery] string id)
        {
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            if (LoggedInUser == null || LoggedInUser.TelegramUserID == 0) return JsonError(System.Net.HttpStatusCode.BadRequest, "Registreer eerst bij de bot", "Registreer eerst bij de bot");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var logger = _botHost.Kernel.Get<Botje.Core.ILoggerFactory>().Create(typeof(RaidsController));
            var raidParticipationCollection = db.GetCollection<RaidParticipation>();

            var raid = raidParticipationCollection.Find(x => x.PublicID == id).SingleOrDefault();
            if (null == raid) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");

            Team team = Team.Unknown;
            int level = -1;
            Botje.Messaging.Models.User user = GetTgUserForLoggedInUser(db, ref team, ref level);

            RemoveAnyAndAllCurrentParticipations(raid, user);

            raid.Maybe.Add(user);

            raid.LastModificationTime = DateTime.UtcNow;

            raidParticipationCollection.Update(raid);

            logger.Info($"Got maybe for raid {raid.Raid.Raid} ({id}) at {raid.Raid.Gym} for {user.UsernameOrName()} (level: {level}, team: {team})");

            var result = raid.Adapt<McRaidDetails>();

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        [Authorize]
        [Audit()]
        [JsonWrapper]
        [HttpPost("done")]
        public async Task<IActionResult> Done([FromQuery] string id)
        {
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            if (LoggedInUser == null || LoggedInUser.TelegramUserID == 0) return JsonError(System.Net.HttpStatusCode.BadRequest, "Registreer eerst bij de bot", "Registreer eerst bij de bot");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var logger = _botHost.Kernel.Get<Botje.Core.ILoggerFactory>().Create(typeof(RaidsController));
            var raidParticipationCollection = db.GetCollection<RaidParticipation>();

            var raid = raidParticipationCollection.Find(x => x.PublicID == id).SingleOrDefault();
            if (null == raid) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");

            Team team = Team.Unknown;
            int level = -1;
            Botje.Messaging.Models.User user = GetTgUserForLoggedInUser(db, ref team, ref level);

            RemoveAnyAndAllCurrentParticipations(raid, user);

            raid.Done.Add(user);

            raid.LastModificationTime = DateTime.UtcNow;

            raidParticipationCollection.Update(raid);

            logger.Info($"Got done for raid {raid.Raid.Raid} ({id}) at {raid.Raid.Gym} for {user.UsernameOrName()} (level: {level}, team: {team})");

            var result = raid.Adapt<McRaidDetails>();

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        [Authorize]
        [Audit()]
        [JsonWrapper]
        [HttpPost("no")]
        public async Task<IActionResult> No([FromQuery] string id)
        {
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            if (LoggedInUser == null || LoggedInUser.TelegramUserID == 0) return JsonError(System.Net.HttpStatusCode.BadRequest, "Registreer eerst bij de bot", "Registreer eerst bij de bot");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var logger = _botHost.Kernel.Get<Botje.Core.ILoggerFactory>().Create(typeof(RaidsController));
            var raidParticipationCollection = db.GetCollection<RaidParticipation>();

            var raid = raidParticipationCollection.Find(x => x.PublicID == id).SingleOrDefault();
            if (null == raid) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");

            Team team = Team.Unknown;
            int level = -1;
            Botje.Messaging.Models.User user = GetTgUserForLoggedInUser(db, ref team, ref level);

            RemoveAnyAndAllCurrentParticipations(raid, user);

            raid.Rejected.Add(user);

            raid.LastModificationTime = DateTime.UtcNow;

            raidParticipationCollection.Update(raid);

            logger.Info($"Got no for raid {raid.Raid.Raid} ({id}) at {raid.Raid.Gym} for {user.UsernameOrName()} (level: {level}, team: {team})");

            var result = raid.Adapt<McRaidDetails>();

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        private Botje.Messaging.Models.User GetTgUserForLoggedInUser(Botje.DB.IDatabase db, ref Team team, ref int level)
        {
            Botje.DB.DbSet<UserSettings> dbSetUserSettings = db.GetCollection<UserSettings>();
            var userSetting = dbSetUserSettings.Find(x => x.User.ID == LoggedInUser.TelegramUserID).FirstOrDefault();
            if (null != userSetting)
            {
                team = userSetting.Team;
                level = userSetting.Level;
            }

            SplitDisplayName(LoggedInUser.DisplayName, out string firstName, out string lastName);

            var tgUser = new Botje.Messaging.Models.User
            {
                ID = LoggedInUser.TelegramUserID,
                FirstName = firstName,
                LastName = lastName,
                Username = LoggedInUser.UserName,
                IsBot = false,
                LanguageCode = "nl-NL",
            };
            return tgUser;
        }

        private void SplitDisplayName(string displayName, out string firstName, out string lastName)
        {
            int index = (displayName ?? "").IndexOf(' ');
            if (-1 == index)
            {
                firstName = displayName ?? String.Empty;
                lastName = String.Empty;
            }
            else
            {
                firstName = displayName.Substring(0, index).Trim();
                lastName = (displayName + " ").Substring(index + 1).Trim();
            }
        }

        private static void RemoveAnyAndAllCurrentParticipations(RaidParticipation raid, Botje.Messaging.Models.User user)
        {
            foreach (Team t in Enum.GetValues(typeof(Team)).OfType<Team>())
            {
                if (raid.Participants.ContainsKey(t) && null != raid.Participants[t])
                {
                    raid.Participants[t].RemoveAll(x => x.User?.ID == user.ID);
                }
            }
            raid.Rejected.RemoveAll(x => x.ID == user.ID);
            raid.Done.RemoveAll(x => x.ID == user.ID);
            raid.Maybe.RemoveAll(x => x.ID == user.ID);
        }
    }
}
