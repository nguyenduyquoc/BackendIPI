using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class CountryDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public List<ProvinceDTO>? Provinces { get; set; }
    }
}
