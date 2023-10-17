using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class RoleDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public List<AdminDTO> Admins { get; set; }
    }
}
