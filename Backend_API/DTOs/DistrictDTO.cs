using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class DistrictDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal? DeliveryFee { get; set; }

        public string? DeliveryType { get; set; }

        public int ProvinceId { get; set; }

        public string? ProvinceName { get; set; }

        public List<UserAddressDTO>? UserAddresses { get; set; }
    }
}
