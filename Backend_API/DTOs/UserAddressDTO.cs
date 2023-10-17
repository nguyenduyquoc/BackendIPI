using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class UserAddressDTO
    {
        public int? Id { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? Address { get; set; }

        public int DistrictId { get; set; }

        public string? DistrictName { get; set; }

        public int? ProvinceId { get; set; }

        public string? ProvinceName { get; set; }
    }
}
