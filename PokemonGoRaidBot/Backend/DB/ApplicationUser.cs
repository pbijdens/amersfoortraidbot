using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaidBot.Backend.DB
{
    /// <summary>
    /// Class representing a single user of the application.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The application user.
        /// </summary>
        public ApplicationUser()
        {
            CreationDateUTC = DateTime.UtcNow;
            Tokens = new List<Token>();
        }

        /// <summary>
        /// Optional display name of the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional profile picture.
        /// </summary>
        public string ProfilePictureBase64 { get; set; }

        /// <summary>
        /// When was this entry created?
        /// </summary>
        [Required]
        public DateTime CreationDateUTC { get; set; }

        /// <summary>
        /// When was this entry last modified?
        /// </summary>
        [Required]
        public DateTime LastModificationDateUTC { get; set; }

        /// <summary>
        /// User ID on Telegram, for contacting the chat bot.
        /// </summary>
        public long TelegramUserID { get; set; }

        /// <summary>
        /// Tokens for this user.
        /// </summary>
        public virtual List<Token> Tokens { get; set; }
    }
}
