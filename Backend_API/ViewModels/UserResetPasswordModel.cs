using System.ComponentModel.DataAnnotations;
namespace Backend_API.ViewModels
{
    public class UserResetPasswordModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
