namespace Backend_API.DTOs
{
    public class UserAddressList
    {
        public List<UserAddressDTO>? UserAddresses { get; set; }

        public int? TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
