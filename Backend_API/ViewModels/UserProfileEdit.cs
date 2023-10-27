namespace Backend_API.ViewModels
{
    public class UserProfileEdit
    {
        public string Fname { get; set; } = null!;

        public string Lname { get; set; } = null!;

        public string? Avatar { get; set; }

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }
    }
}
