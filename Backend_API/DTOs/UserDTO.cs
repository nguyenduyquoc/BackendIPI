using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class UserDTO
    {
        public int? Id { get; set; }

        public string Fname { get; set; } = null!;

        public string Lname { get; set; } = null!;

        public string? Avatar { get; set; }

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public bool Subscribe { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public List<OrderDTO>? Orders { get; set; }

        public List<UserAddressDTO>? UserAddresses { get; set; }
    }
}
