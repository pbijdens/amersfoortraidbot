using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RaidBot.Backend.DB
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Create default Users (if there are none)
            if (!dbContext.Users.Any())
            {
                CreateUsers(dbContext, roleManager, userManager).GetAwaiter().GetResult();
            }
        }

        private static async Task CreateUsers(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            if (dbContext.Users.Count() != 0)
            {
                // Only create the user when there are no users.
                return;
            }

            // local variables
            DateTime createdDate = new DateTime(2016, 03, 01, 12, 30, 00);
            DateTime lastModifiedDate = DateTime.UtcNow;

            string[] roles = SecurityPolicy.AllRoles;

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create the 'default' user to bootstrap the system, do not forget to change this password later.
            var defaultUser = new ApplicationUser()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = "admin",
                Email = "pieter-bas@ijdens.com",
                EmailConfirmed = true,
                DisplayName = "Systeembeheerder",
                CreationDateUTC = createdDate,
                LastModificationDateUTC = lastModifiedDate
            };
            if (await userManager.FindByNameAsync(defaultUser.UserName) == null)
            {
                await userManager.CreateAsync(defaultUser, "Password!!!1");
                foreach (var role in roles)
                {
                    await userManager.AddToRoleAsync(defaultUser, role);
                }

                // Remove Lockout and E-Mail confirmation.
                defaultUser.EmailConfirmed = true;
                defaultUser.LockoutEnabled = false;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
