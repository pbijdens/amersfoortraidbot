using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RaidBot.Backend;
using RaidBot.Backend.API;
using RaidBot.Backend.DB;
using RaidBot.Backend.Services;
using RaidBot.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace RaidBot.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        public UserController(ApplicationDbContext dbContext, ILoggerFactory loggerFactory, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
            : base(dbContext, loggerFactory, roleManager, userManager, configuration)
        {
        }

        [Authorize]
        [Audit()]
        [JsonWrapper]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            using (var context = CreateOperationContext())
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToUpperInvariant();
                var user = context.DB.Users.Where(x => x.Id.ToUpperInvariant() == userId).FirstOrDefault()?.Adapt<McUser>();
                var userClaims = User.Claims.Select(x => new { name = x.Type, value = x.Value }).ToList();
                bool isAdministrator = User.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == SecurityPolicy.RoleAdministrator);

                // infer all member names to be the same as local names
                var result = new { userId, userClaims, user, isAdministrator };

                return new JsonResult(result, JsonSettings);
            }
        }

        [Authorize]// (Policy = SecurityPolicy.CanAccessMessages)]
        [Audit()] // required on every controller function that's part of the API, audits the request
        [JsonWrapper] // needed to make sure errors are sent as JSON not as error page
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            using (var context = CreateOperationContext())
            {
                var result = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).OrderBy(x => x).ToList();
                return new JsonResult(result, JsonSettings);
            }
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()] // required on every controller function that's part of the API, audits the request
        [JsonWrapper] // needed to make sure errors are sent as JSON not as error page
        [HttpGet("list")]
        public IActionResult GetList(int start = 0, int num = Int32.MaxValue, string query = null, bool includeDeleted = false)
        {
            using (var context = CreateOperationContext())
            {
                var queryLC = query?.ToLowerInvariant();
                IQueryable<ApplicationUser> dbResults = context.DB.Users;
                if (!includeDeleted) dbResults = dbResults.Where(x => !x.LockoutEnabled);
                if (!string.IsNullOrWhiteSpace(query)) dbResults = dbResults.Where(x => (x.DisplayName != null && x.DisplayName.ToLowerInvariant().Contains(queryLC)) || (null != x.UserName && x.UserName.ToLowerInvariant().Contains(queryLC)) || (null != x.Email && x.Email.ToLowerInvariant().Contains(queryLC)));
                dbResults = dbResults.OrderBy(x => x.DisplayName);
                dbResults = dbResults.Skip(start).Take(num);

                var result = dbResults.Select(x => x.Adapt<McUser>());
                return new JsonResult(result, JsonSettings);
            }
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()] // required on every controller function that's part of the API, audits the request
        [JsonWrapper] // needed to make sure errors are sent as JSON not as error page
        [HttpGet("user")]
        public async Task<IActionResult> GetUserInfo(string id)
        {
            using (var context = CreateOperationContext())
            {
                var user = await UserManager.FindByIdAsync(id);

                if (user == null) throw new ArgumentException("Invalid user identifier", "id");
                McUserEditorData result = await ToMcEditorUserData(user);

                return new JsonResult(result, JsonSettings);
            }
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()] // required on every controller function that's part of the API, audits the request
        [JsonWrapper] // needed to make sure errors are sent as JSON not as error page
        [HttpPost("user")]
        public async Task<IActionResult> UpdateUser([FromBody] McUserEditorData data)
        {
            using (var context = CreateOperationContext())
            {
                List<string> validationErrors = new List<string>();

                if (null == data)
                {
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input");
                }

                var user = await UserManager.FindByIdAsync(data?.Id ?? "");
                if (null == user)
                {
                    validationErrors.Add("INTERNAL_ERROR");
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input", validationErrors);
                }

                bool isMe = string.Equals(User.FindFirst(ClaimTypes.NameIdentifier).Value, data?.Id, StringComparison.InvariantCultureIgnoreCase);

                if (data.LockoutEnabled && isMe)
                {
                    validationErrors.Add("CANNOT_LOCKOUT_SELF");
                }

                var roles = await UserManager.GetRolesAsync(user);
                if (!data.IsAdministrator && roles.Contains(SecurityPolicy.RoleAdministrator) && isMe)
                {
                    // Can't remove admin role for $self
                    if (string.IsNullOrWhiteSpace(user?.DisplayName)) validationErrors.Add("ADMIN_ROLE_CHANGE_FOR_SELF");
                }

                if (!string.Equals(user.UserName, data?.UserName))
                {
                    var existingUser = await UserManager.FindByNameAsync(data?.UserName ?? "");
                    if (null != existingUser)
                    {
                        validationErrors.Add("USERNAME_UNAVAILABLE");
                    }
                }

                if (!string.Equals(user.Email, data?.Email))
                {
                    var existingUser = await UserManager.FindByEmailAsync(data?.Email ?? "");
                    if (null != existingUser)
                    {
                        validationErrors.Add("EMAIL_UNAVAILABLE");
                    }
                }

                if (!DataHelper.IsValidEMail(data.Email)) validationErrors.Add("EMAIL_INVALID");
                if (string.IsNullOrWhiteSpace(data.Id)) validationErrors.Add("ID_INVALID");
                if (string.IsNullOrWhiteSpace(data.UserName)) validationErrors.Add("USERNAME_INVALID");
                if (string.IsNullOrWhiteSpace(data.DisplayName)) validationErrors.Add("NAME_INVALID");

                // If there are validation errors on the input, stop here and now
                if (validationErrors.Count > 0)
                {
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input", validationErrors);
                }

                // Change password on demand
                if (!string.IsNullOrEmpty(data.Password))
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(user, data.Password);
                }

                user.UserName = data.UserName;
                user.DisplayName = data.DisplayName;
                user.ProfilePictureBase64 = data.ProfilePictureBase64;
                user.Email = data.Email;
                user.EmailConfirmed = true;
                user.LastModificationDateUTC = DateTime.UtcNow;

                if (await UserManager.GetLockoutEnabledAsync(user) != data.LockoutEnabled)
                {
                    await UserManager.SetLockoutEnabledAsync(user, data.LockoutEnabled);
                }

                // Request an update by the identity manager.
                var identityResult = await UserManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                {
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input", validationErrors, identityResult.Errors);
                }
                else
                {
                    await UpdateRoleForUser(user, SecurityPolicy.RoleAdministrator, data.IsAdministrator);

                    // Return the updated user
                    McUserEditorData result = await ToMcEditorUserData(await UserManager.FindByIdAsync(data?.Id ?? ""));
                    return new JsonResult(result, JsonSettings);
                }
            }
        }

        [Authorize(Policy = SecurityPolicy.IsAdministrator)]
        [Audit()] // required on every controller function that's part of the API, audits the request
        [JsonWrapper] // needed to make sure errors are sent as JSON not as error page
        [HttpPut("user")]
        public async Task<IActionResult> CreateUser([FromBody] McUserEditorData data)
        {
            using (var context = CreateOperationContext())
            {
                List<string> validationErrors = new List<string>();

                if (null == data)
                {
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input");
                }

                if (!string.IsNullOrWhiteSpace(data.Id))
                {
                    validationErrors.Add("ID_INVALID");
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input", validationErrors);
                }

                var existingUser = await UserManager.FindByNameAsync(data?.UserName ?? "");
                if (null != existingUser)
                {
                    validationErrors.Add("USERNAME_UNAVAILABLE");
                }

                existingUser = await UserManager.FindByEmailAsync(data?.Email ?? "");
                if (null != existingUser)
                {
                    validationErrors.Add("EMAIL_UNAVAILABLE");
                }

                if (!DataHelper.IsValidEMail(data.Email)) validationErrors.Add("EMAIL_INVALID");
                if (string.IsNullOrWhiteSpace(data.UserName)) validationErrors.Add("USERNAME_INVALID");
                if (string.IsNullOrWhiteSpace(data.DisplayName)) validationErrors.Add("NAME_INVALID");
                if (string.IsNullOrWhiteSpace(data.Password)) validationErrors.Add("PASSWORD_INVALID");

                // If there are validation errors on the input, stop here and now
                if (validationErrors.Count > 0)
                {
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input", validationErrors);
                }

                var user = new ApplicationUser
                {
                    UserName = data.UserName,
                    DisplayName = data.DisplayName,
                    ProfilePictureBase64 = data.ProfilePictureBase64,
                    Email = data.Email,
                    EmailConfirmed = true,
                    CreationDateUTC = DateTime.UtcNow,
                    LastModificationDateUTC = DateTime.UtcNow,
                    LockoutEnabled = false,
                };

                // Ask the usermanager for the user to be created
                var identityResult = await UserManager.CreateAsync(user, data.Password);
                if (!identityResult.Succeeded)
                {
                    return JsonError(System.Net.HttpStatusCode.BadRequest, "Invalid input", "Invalid input", validationErrors, identityResult.Errors);
                }
                else
                {
                    user = await UserManager.FindByNameAsync(data.UserName);
                    await UpdateRoleForUser(user, SecurityPolicy.RoleAdministrator, data.IsAdministrator);
                    await UserManager.SetLockoutEnabledAsync(user, false);

                    // Return the updated user
                    McUserEditorData result = await ToMcEditorUserData(await UserManager.FindByNameAsync(data.UserName));
                    return new JsonResult(result, JsonSettings);
                }
            }
        }

        private async Task<McUserEditorData> ToMcEditorUserData(ApplicationUser user)
        {
            var result = user.Adapt<McUserEditorData>();

            var roles = await UserManager.GetRolesAsync(user);
            result.IsAdministrator = roles.Contains(SecurityPolicy.RoleAdministrator);
            result.LockoutEnabled = await UserManager.GetLockoutEnabledAsync(user);
            return result;
        }

        private async Task UpdateRoleForUser(ApplicationUser user, string role, bool newValue)
        {
            var roles = await UserManager.GetRolesAsync(user);
            if (newValue && !roles.Contains(role))
            {
                await UserManager.AddToRoleAsync(user, role);
            }
            else if (!newValue && roles.Contains(role))
            {
                await UserManager.RemoveFromRoleAsync(user, role);
            }
        }
    }
}
