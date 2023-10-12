using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class UserAddressDTO
    {
        public int? Id { get; set; }

        public int UserId { get; set; }

        public string? Address { get; set; }

        public int DistrictId { get; set; }

        public virtual DistrictDTO District { get; set; } = null!;

        public virtual UserDTO User { get; set; } = null!;
    }
}
