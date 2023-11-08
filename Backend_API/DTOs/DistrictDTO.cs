using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class DistrictDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public int ProvinceId { get; set; }

        public string? ProvinceName { get; set; }

        public virtual ICollection<DeliveryServiceDTO> DeliveryServices { get; set; } = new List<DeliveryServiceDTO>();
    }
}
