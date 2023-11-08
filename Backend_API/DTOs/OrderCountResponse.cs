namespace Backend_API.DTOs
{
    public class OrderCountResponse
    {
        public int TotalOrders { get; set; }

        public int ConfirmedOrders { get; set; }

        public int ShippingOrders { get; set; }

        public int DeliveredOrders { get; set; }
    }
}
