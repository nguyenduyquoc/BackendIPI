using System.ComponentModel.DataAnnotations;

namespace Backend_API.ViewModels
{
    public class UserLoginModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
