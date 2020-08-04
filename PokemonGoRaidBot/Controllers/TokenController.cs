using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RaidBot.Backend.API;
using RaidBot.Backend.DB;
using RaidBot.Filters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RaidBot.Controllers
{
    [Route("api/[controller]")]
    public partial class TokenController : ControllerBase
    {
        public TokenController(ApplicationDbContext db, ILoggerFactory loggerFactory, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
            : base(db, loggerFactory, roleManager, userManager, configuration)
        {
            SignInManager = signInManager;
        }

        #region Properties
        protected SignInManager<ApplicationUser> SignInManager { get; private set; }
        #endregion

        [Audit]
        [HttpPost("auth")]
        public async Task<IActionResult> Auth([FromBody]McTokenRequest model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null) return new StatusCodeResult(500);

            switch (model.grant_type)
            {
                case "password":
                    return await GetToken(model);
                case "refresh_token":
                    return await RefreshToken(model);
                default:
                    // not supported - return a HTTP 401 (Unauthorized)
                    return new UnauthorizedResult();
            }
        }

#if false
        [Audit]
        [HttpPost("facebook")]
        public async Task<IActionResult> Facebook([FromBody]McExternalLoginRequest model)
        {
            try
            {
                var fbAPI_url = "https://graph.facebook.com/v2.10/";
                var fbAPI_queryString = String.Format("me?scope=email&access_token={0}&fields=id,name,email", model.access_token);
                string result = null;

                // fetch the user info from Facebook Graph v2.10
                using (var c = new HttpClient())
                {
                    c.BaseAddress = new Uri(fbAPI_url);
                    var response = await c.GetAsync(fbAPI_queryString);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else throw new Exception("Authentication error");
                };

                // load the resulting Json into a dictionary
                var epInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                var info = new UserLoginInfo("facebook", epInfo["id"], "Facebook");

                // Check if this user already registered himself with this external provider before
                var user = await UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user == null)
                {
                    // If we reach this point, it means that this user never tried to logged in
                    // using this external provider. However, it could have used other providers 
                    // and /or have a local account. 
                    // We can find out if that's the case by looking for his e-mail address.

                    // Lookup if there's an username with this e-mail address in the Db
                    user = await UserManager.FindByEmailAsync(epInfo["email"]);
                    if (user == null)
                    {
                        // No user has been found: register a new user using the info 
                        //  retrieved from the provider
                        DateTime now = DateTime.UtcNow;
                        var username = String.Format("FB{0}{1}", epInfo["id"], Guid.NewGuid().ToString("N"));
                        user = new ApplicationUser()
                        {
                            SecurityStamp = Guid.NewGuid().ToString(),
                            // ensure the user will have an unique username
                            UserName = username,
                            Email = epInfo["email"],
                            DisplayName = epInfo["name"],
                            CreationDateUTC = now,
                            LastModificationDateUTC = now
                        };

                        // Add the user to the Db with a random password
                        await UserManager.CreateAsync(user, DataHelper.GenerateRandomPassword());

                        // Assign the user to the 'RegisteredUser' role.
                        await UserManager.AddToRoleAsync(user, "RegisteredUser");

                        // Remove Lockout and E-Mail confirmation
                        user.EmailConfirmed = true;
                        user.LockoutEnabled = false;

                        // Persist everything into the Db
                        DbContext.SaveChanges();
                    }
                    // Register this external provider to the user
                    var ir = await UserManager.AddLoginAsync(user, info);
                    if (ir.Succeeded)
                    {
                        // Persist everything into the Db
                        DbContext.SaveChanges();
                    }
                    else throw new Exception("Authentication error");
                }

                // create the refresh token
                var rt = CreateRefreshToken(model.client_id, user.Id);

                // add the new refresh token to the DB
                DbContext.Tokens.Add(rt);
                DbContext.SaveChanges();

                // create & return the access token
                var t = await CreateAccessToken(user.Id, rt.Value);
                return Json(t);
            }
            catch (Exception ex)
            {
                // return a HTTP Status 400 (Bad Request) to the client
                return BadRequest(new { Error = ex.Message });
            }
        }

        [Audit]
        [HttpGet("ExternalLogin/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            switch (provider.ToLower())
            {
                case "facebook":
                    // case "google":
                    // case "twitter":
                    // todo: add all supported providers here

                    // Redirect the request to the external provider.
                    var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Token", new { returnUrl });
                    var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                    return Challenge(properties, provider);
                default:
                    // provider not supported
                    return BadRequest(new
                    {
                        Error = String.Format("Provider '{0}' is not supported.", provider)
                    });
            }
        }

        [Audit]
        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(
            string returnUrl = null, string remoteError = null)
        {
            if (!String.IsNullOrEmpty(remoteError))
            {
                // TODO: handle external provider errors
                throw new Exception(String.Format("External Provider error: {0}", remoteError));
            }

            // Extract the login info obtained from the External Provider
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // if there's none, emit an error
                throw new Exception("ERROR: No login info available.");
            }

            // Check if this user already registered himself with this external provider before
            var user = await UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                // If we reach this point, it means that this user never tried to logged in
                // using this external provider. However, it could have used other providers 
                // and /or have a local account. 
                // We can find out if that's the case by looking for his e-mail address.

                // Retrieve the 'emailaddress' claim
                var emailKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
                var email = info.Principal.FindFirst(emailKey).Value;

                // Lookup if there's an username with this e-mail address in the Db
                user = await UserManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // No user has been found: register a new user 
                    // using the info retrieved from the provider
                    DateTime now = DateTime.UtcNow;

                    // Create a unique username using the 'nameidentifier' claim
                    var idKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                    var username = String.Format("{0}{1}{2}",
                        info.LoginProvider,
                        info.Principal.FindFirst(idKey).Value,
                        Guid.NewGuid().ToString("N")
                        );

                    user = new ApplicationUser()
                    {
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = username,
                        Email = email,
                        CreationDateUTC = now,
                        LastModificationDateUTC = now
                    };

                    // Add the user to the Db with a random password
                    await UserManager.CreateAsync(user, DataHelper.GenerateRandomPassword());

                    // Assign the user to the 'RegisteredUser' role.
                    await UserManager.AddToRoleAsync(user, "RegisteredUser");

                    // Remove Lockout and E-Mail confirmation
                    user.EmailConfirmed = true;
                    user.LockoutEnabled = false;

                    // Persist everything into the Db
                    await DbContext.SaveChangesAsync();
                }
                // Register this external provider to the user
                var ir = await UserManager.AddLoginAsync(user, info);
                if (ir.Succeeded)
                {
                    // Persist everything into the Db
                    DbContext.SaveChanges();
                }
                else throw new Exception("Authentication error");
            }

            // create the refresh token
            var rt = CreateRefreshToken("RaidBot", user.Id);

            // add the new refresh token to the DB
            DbContext.Tokens.Add(rt);
            DbContext.SaveChanges();

            // create & return the access token
            var t = await CreateAccessToken(user.Id, rt.Value);

            // output a <SCRIPT> tag to call a JS function 
            // registered into the parent window global scope
            return Content(
                "<script type=\"text/javascript\">" +
                "window.opener.externalProviderLogin(" +
                    JsonConvert.SerializeObject(t, JsonSettings) +
                ");" +
                "window.close();" +
                "</script>",
                "text/html"
                );
        }
#endif

        private async Task<IActionResult> GetToken(McTokenRequest model)
        {
            try
            {
                // check if there's an user with the given username
                var user = await UserManager.FindByNameAsync(model.username);

                // if this is not a username, it's probably an e-mail address
                if (user == null && model.username.Contains("@"))
                {
                    user = await UserManager.FindByEmailAsync(model.username);
                }

                if (user == null || !await UserManager.CheckPasswordAsync(user, model.password))
                {
                    // user does not exists or password mismatch
                    return new UnauthorizedResult();
                }

                if (await UserManager.GetLockoutEnabledAsync(user))
                {
                    // User is locked out
                    return new UnauthorizedResult();
                }

                // username & password matches: create the refresh token
                var rt = CreateRefreshToken(model.client_id, user.Id);

                // add the new refresh token to the DB
                DbContext.Tokens.Add(rt);
                DbContext.SaveChanges();

                // create & return the access token
                var t = await CreateAccessToken(user.Id, rt.Value);
                return Json(t);
            }
            catch (Exception)
            {
                return new UnauthorizedResult();
            }
        }

        private async Task<IActionResult> RefreshToken(McTokenRequest model)
        {
            try
            {
                // check if the received refreshToken exists for the given clientId
                var rt = DbContext.Tokens.FirstOrDefault(t => t.ClientId == model.client_id && t.Value == model.refresh_token);
                if (rt == null)
                {
                    // refresh token not found or invalid (or invalid clientId)
                    return new UnauthorizedResult();
                }

                // check if there's an user with the refresh token's userId
                var user = await UserManager.FindByIdAsync(rt.UserId);
                if (user == null)
                {
                    // UserId not found or invalid
                    return new UnauthorizedResult();
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    // USer is no loner allowed to log in
                    return new UnauthorizedResult();
                }

                // generate a new refresh token
                var rtNew = CreateRefreshToken(rt.ClientId, rt.UserId);

                // invalidate the old refresh token (by deleting it)
                DbContext.Tokens.Remove(rt);

                // add the new refresh token
                DbContext.Tokens.Add(rtNew);

                // persist changes in the DB
                DbContext.SaveChanges();

                // create a new access token...
                var response = await CreateAccessToken(rtNew.UserId, rtNew.Value);

                // ... and send it to the client
                return Json(response);
            }
            catch (Exception)
            {
                return new UnauthorizedResult();
            }
        }

        private Token CreateRefreshToken(string clientId, string userId)
        {
            return new Token()
            {
                ClientId = clientId,
                UserId = userId,
                Type = 0,
                Value = Guid.NewGuid().ToString("N"),
                CreatedDate = DateTime.UtcNow
            };
        }

        private async Task<McTokenResponse> CreateAccessToken(string userId, string refreshToken)
        {
            var user = await UserManager.FindByIdAsync(userId);

            DateTime now = DateTime.UtcNow;

            // add the registered claims for JWT (RFC7519).
            // For more info, see https://tools.ietf.org/html/rfc7519#section-4.1
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName?? String.Empty),
                new Claim(ClaimTypes.Email, user.Email?? String.Empty),
                new Claim("ProfilePictureBase64", user.ProfilePictureBase64 ?? String.Empty),
            };

            var roles = await UserManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            var tokenExpirationMins = Configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
            var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: Configuration["Auth:Jwt:Issuer"],
                audience: Configuration["Auth:Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(tokenExpirationMins)),
                signingCredentials: new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256)
            );
            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new McTokenResponse()
            {
                token = encodedToken,
                expiration = tokenExpirationMins,
                refresh_token = refreshToken
            };
        }
    }
}
