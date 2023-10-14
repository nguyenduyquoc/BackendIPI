using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ProvinceDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public int CountryId { get; set; }

        public string? CountryName { get; set; }

        public List<DistrictDTO>? Districts { get; set; }
    }
}
