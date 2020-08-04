using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RaidBot.Backend;
using RaidBot.Backend.DB;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RaidBot.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected ILogger Logger;

        protected ApplicationDbContext DbContext { get; private set; }
        protected RoleManager<IdentityRole> RoleManager { get; private set; }
        protected UserManager<ApplicationUser> UserManager { get; private set; }
        protected IConfiguration Configuration { get; private set; }
        protected JsonSerializerSettings JsonSettings { get; private set; }

        public ControllerBase(ApplicationDbContext db, ILoggerFactory loggerFactory, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            DbContext = db;

            RoleManager = roleManager;
            UserManager = userManager;
            Configuration = configuration;

            // Instantiate a single JsonSerializerSettings object
            // that can be reused multiple times.
            JsonSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
        }

        public OperationContext CreateOperationContext()
        {
            var result = new OperationContext
            {
                AuthenticatedUser = User,
                DB = DbContext
            };
            return result;
        }

        public IActionResult JsonError(System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.BadRequest, string message = "An error occurred", string error = "An error occurred", IEnumerable<string> validationErrors = null, IEnumerable<IdentityError> identityErrors = null)
        {
            HttpContext.Response.StatusCode = (int)statusCode;
            HttpContext.Response.ContentType = "application/json";
            return new JsonResult(new
            {
                success = false,
                error,
                message,
                validationErrors = validationErrors?.ToList(),
                identityErrors = identityErrors?.ToList(),
            }, new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            });
        }

        public ApplicationUser LoggedInUser
        {
            get
            {
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToUpperInvariant();
                    var user = DbContext.Users.Where(x => x.Id.ToUpperInvariant() == userId).SingleOrDefault();
                    return user;
                }
            }
        }
    }
}
