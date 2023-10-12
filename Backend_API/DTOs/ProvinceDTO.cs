using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ProvinceDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public int CountryId { get; set; }

        public virtual CountryDTO Country { get; set; } = null!;

        public List<DistrictDTO>? Districts { get; set; }
    }
}
