using System.ComponentModel.DataAnnotations;

namespace NotificationAPI
{
    public class UserLogin
    {
        [Key]
        public int Id { get; set; } 

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
