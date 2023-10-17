namespace Backend_API.ViewModels
{
    public class UserAddressCreateModel
    {
        public int UserId { get; set; }

        public string? Address { get; set; }

        public int DistrictId { get; set; }
    }
}
