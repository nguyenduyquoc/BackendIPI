namespace Backend_API.DTOs
{
    public class ReturnRequestDTO
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int Status { get; set; }

        public string ReturnReason { get; set; } = null!;

        public decimal RefundAmount { get; set; }

        public string? Response { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual OrderDTO Order { get; set; } = null!;

        public virtual ICollection<ReturnRequestImageDTO> ReturnRequestImages { get; set; } = new List<ReturnRequestImageDTO>();

        public virtual ICollection<OrderProductDTO> ReturnProducts { get; set; } = new List<OrderProductDTO>();
    }
}
