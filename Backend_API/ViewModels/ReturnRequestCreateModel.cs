namespace Backend_API.ViewModels
{
    public class ReturnRequestCreateModel
    {
        public int OrderId { get; set; }

        public string ReturnReason { get; set; } = null!;

        public decimal RefundAmount { get; set; }

        public virtual ICollection<ReturnProductModel> ReturnProducts { get; set; } = new List<ReturnProductModel>();

        public virtual ICollection<ReturnRequestImageCreateModel> ReturnRequestImages { get; set; } = new List<ReturnRequestImageCreateModel>();
    }

    public class ReturnProductModel
    {
        public int OrderProductId { get; set; }

        public int ReturnQuantity { get; set; }
    }

    public class ReturnRequestImageCreateModel
    {
        public string Url { get; set; }
    }
}
