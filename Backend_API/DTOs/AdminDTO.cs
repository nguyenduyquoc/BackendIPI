using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class AdminDTO
    {
        public int? Id { get; set; }

        public int UserId { get; set; }

        public string Role { get; set; } = null!;

        public virtual UserDTO User { get; set; } = null!;
    }
}
