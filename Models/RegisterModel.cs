using System;
using System.ComponentModel.DataAnnotations;

namespace NotificationAPI
{
    public class RegisterModel
    {
        [Key]
        public Guid UserId { get; set; } 
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
