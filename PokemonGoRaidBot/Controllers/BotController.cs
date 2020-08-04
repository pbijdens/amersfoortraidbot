using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ninject;
using RaidBot.Backend;
using RaidBot.Backend.API;
using RaidBot.Backend.Bot;
using RaidBot.Backend.Bot.PokemonRaidBot.Entities;
using RaidBot.Backend.DB;
using RaidBot.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace RaidBot.Controllers
{
    [Route("api/bots")]
    public class BotController : ControllerBase
    {
        private IPokemonRaidBotHost _botHost;
        public BotController(ApplicationDbContext dbContext, ILoggerFactory loggerFactory, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration, IPokemonRaidBotHost botHost)
            : base(dbContext, loggerFactory, roleManager, userManager, configuration)
        {
            _botHost = botHost;
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()]
        [JsonWrapper]
        [HttpGet("list")]
        public IActionResult GetList(int start = 0, int num = Int32.MaxValue, string query = null, bool includeDisabled = false)
        {
            // TODO: In due time we'll keep a list of bots in the database, for now we manage one single bot
            List<McBot> result = new List<McBot>();
            result.Add(new McBot
            {
                Id = _botHost.ID,
                Name = _botHost.Name,
                IsStarted = _botHost.IsRunning
            });
            return new JsonResult(result, JsonSettings);
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()]
        [JsonWrapper]
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromQuery] string id)
        {
            if (id != _botHost.ID) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");
            if (_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Already running", "Already running");

            _botHost.Start();

            var result = await Task.FromResult(new McBot
            {
                Id = _botHost.ID,
                Name = _botHost.ID,
                IsStarted = _botHost.IsRunning
            });

            return new JsonResult(result, JsonSettings);
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()]
        [JsonWrapper]
        [HttpPost("stop")]
        public async Task<IActionResult> Stop([FromQuery] string id)
        {
            if (id != _botHost.ID) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Already stopped", "Already stopped");

            _botHost.Stop();

            var result = await Task.FromResult(new McBot
            {
                Id = _botHost.ID,
                Name = _botHost.ID,
                IsStarted = _botHost.IsRunning
            });

            return new JsonResult(result, JsonSettings);
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()]
        [JsonWrapper]
        [HttpGet("raids")]
        public async Task<IActionResult> Raids([FromQuery] string botID, int start = 0, int num = Int32.MaxValue, string query = null)
        {
            if (botID != _botHost.ID) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var coll = db.GetCollection<RaidParticipation>();

            IQueryable<RaidParticipation> allRaids = coll.FindAll().AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                allRaids = allRaids.Where(x => (x.Raid.Raid != null && x.Raid.Raid.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                               || (x.Raid.Gym != null && x.Raid.Gym.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                               || (x.Raid.Remarks != null && x.Raid.Remarks.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                               || (x.Raid.UniqueID != null && x.Raid.UniqueID.ToString().Equals(query, StringComparison.InvariantCultureIgnoreCase))
                                               || (x.Raid.User != null && "{x.Raid.User.Username}".IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                               || (x.PublicID != null && x.PublicID.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                               );
            }
            allRaids = allRaids.OrderByDescending(x => x.Raid.RaidUnlockTime);
            var result = allRaids.Skip(start).Take(num).ToArray().Select(x => x.Adapt<McRaidDetails>());

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()]
        [JsonWrapper]
        [HttpGet("raid")]
        public async Task<IActionResult> Raid([FromQuery] string botID, [FromQuery] string publicID)
        {
            if (botID != _botHost.ID) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var coll = db.GetCollection<RaidParticipation>();
            var raid = coll.Find(x => x.PublicID == publicID).SingleOrDefault();

            var result = raid.Adapt<McRaidDetails>();

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()]
        [JsonWrapper]
        [HttpPost("raid")]
        public async Task<IActionResult> Raid([FromQuery] string botID, [FromQuery] string publicID, [FromBody] McRaidDetails data)
        {
            if (botID != _botHost.ID) return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid ID", "Invalid ID");
            if (!_botHost.IsRunning) return JsonError(System.Net.HttpStatusCode.BadRequest, "Not started", "Not started");

            var db = _botHost.Kernel.Get<Botje.DB.IDatabase>();
            var coll = db.GetCollection<RaidParticipation>();
            var raid = coll.Find(x => x.PublicID == publicID).SingleOrDefault();

            // TODO: UPDATE THE RAID

            var result = raid.Adapt<McRaidDetails>();

            return new JsonResult(await Task.FromResult(result), JsonSettings);
        }
    }
}
