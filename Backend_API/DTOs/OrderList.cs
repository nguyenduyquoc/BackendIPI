namespace Backend_API.DTOs
{
    public class OrderList
    {
        public List<OrderDTO>? Orders { get; set; }

        public int? TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
