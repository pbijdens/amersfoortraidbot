using Botje.Messaging.Models;
using Microsoft.AspNetCore.Identity;
using Ninject;
using RaidBot.Backend.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaidBot.Backend.Bot.PokemonRaidBot.ChatCommands
{
    public class Register : ChatCommandModuleBase
    {
        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        public override void ProcessCommand(Source source, Message message, string command, string[] args)
        {
            switch (command)
            {
                case "/reg":
                case "/registreer":
                    if (source == Source.Private)
                    {
                        DoRegisterCommand(message, command, args);
                    }
                    break;
            }
        }

        private async void DoRegisterCommand(Message message, string command, string[] args)
        {
            long id = message.From.ID;
            var userManager = GetService<UserManager<ApplicationUser>>();
            var dbContext = GetService<ApplicationDbContext>();

            var currentUser = dbContext.Users.Where(x => x.TelegramUserID == message.From.ID).FirstOrDefault();
            if (null == currentUser)
            {
                string username = CreateUniqueUsernameForTelegramUser(dbContext, message.From);
                string password = CreatePassword();

                // create a new user
                var user = new ApplicationUser
                {
                    UserName = username,
                    DisplayName = message.From.DisplayName(),
                    ProfilePictureBase64 = "",
                    Email = null,
                    EmailConfirmed = false,
                    CreationDateUTC = DateTime.UtcNow,
                    LastModificationDateUTC = DateTime.UtcNow,
                    LockoutEnabled = false,
                    TelegramUserID = message.From.ID,
                };

                try
                {
                    var identityResult = await userManager.CreateAsync(user, password);
                    if (identityResult.Succeeded)
                    {
                        // unlock the user account
                        await userManager.SetLockoutEnabledAsync(user, false);
                        Client.SendMessageToChat(id, $"Er is een nieuw account voor je aangemaakt.\nJe gebruikersnaam is: <code>{_HTML_(username)}</code>\nJe wachtwoord is: <code>{password}</code>\nKlik <a href=\"{_HTML_(Setting.BotAddress)}\">hier</a> om naar de site te gaan.", "HTML", true, false);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var error in identityResult.Errors ?? new List<IdentityError>())
                        {
                            sb.AppendLine($"- {_HTML_(error.Code)}: {_HTML_(error.Description)}");
                        }
                        Client.SendMessageToChat(id, $"Er is iets misgegaan waardoor we geen account voor je hebben kunnen aanmaken:\n{sb.ToString()}", "HTML", true, false);
                    }
                }
                catch (Exception ex)
                {
                    Client.SendMessageToChat(id, $"Er is iets misgegaan waardoor we geen account voor je hebben kunnen aanmaken:\n{_HTML_(ex.ToString())}", "HTML", true, false);
                }
            }
            else
            {
                try
                {
                    string password = CreatePassword();

                    var dbUser = await userManager.FindByIdAsync(currentUser.UserName);

                    currentUser.PasswordHash = userManager.PasswordHasher.HashPassword(currentUser, password);
                    var identityResult = await userManager.UpdateAsync(currentUser);
                    if (identityResult.Succeeded)
                    {
                        Client.SendMessageToChat(id, $"Er is een nieuw account voor je aangemaakt.\nJe gebruikersnaam is: <code>{_HTML_(currentUser.UserName)}</code>\nJe nieuwe wachtwoord is: <code>{password}</code>\nKlik <a href=\"{_HTML_(Setting.BotAddress)}\">hier</a> om naar de site te gaan.", "HTML", true, false);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var error in identityResult.Errors ?? new List<IdentityError>())
                        {
                            sb.AppendLine($"- {_HTML_(error.Code)}: {_HTML_(error.Description)}");
                        }
                        Client.SendMessageToChat(id, $"Er is iets niet helemaal goedgegaan:\n{sb.ToString()}", "HTML", true, false);
                    }
                }
                catch (Exception ex)
                {
                    Client.SendMessageToChat(id, $"Er is iets niet helemaal goedgegaan:\n<pre>{_HTML_(ex.ToString())}</pre>", "HTML", true, false);
                }

            }
        }

        private string CreatePassword()
        {
            string consonants = "qwrtpsdfghjklzxcvbnm";
            string vowels = "aeiouy";
            string numbers = "01234567890";
            return $"{Char.ToUpper(consonants.Random())}{vowels.Random()}{Char.ToUpper(consonants.Random())}{vowels.Random()}{numbers.Random()}{numbers.Random()}{numbers.Random()}{numbers.Random()}";
        }

        private string CreateUniqueUsernameForTelegramUser(ApplicationDbContext dbContext, User from)
        {
            int suffix = 0;
            string name = Regex.Replace(from.UsernameOrName(), "[^a-z0-9-_]", "", RegexOptions.IgnoreCase);
            string result = name;
            do
            {
                var existingUser = dbContext.Users.Where(x => string.Equals(x.UserName, result, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (null != existingUser)
                {
                    result = $"{name}{suffix++}";
                }
                else
                {
                    return result;
                }
            } while (true);
        }

        public T GetService<T>() where T : class
        {
            return ServiceProvider.GetService(typeof(T)) as T;
        }
    }

    public static class RegisterExtensions
    {
        private static Random rnd = new Random();

        public static char Random(this string s)
        {
            return s[rnd.Next(s.Length)];
        }
    }
}
