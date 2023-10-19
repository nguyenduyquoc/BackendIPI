using System.ComponentModel.DataAnnotations;

namespace Backend_API.ViewModels
{
    public class UserRegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string Fname { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string Lname { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
