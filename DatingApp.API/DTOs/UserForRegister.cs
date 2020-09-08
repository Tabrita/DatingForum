using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserForRegister
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(255, MinimumLength=8, ErrorMessage = "Password should not be less than eight characters")]        
        public string Password { get; set; }
    }
}