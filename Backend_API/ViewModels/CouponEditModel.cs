namespace Backend_API.ViewModels
{
    public class CouponEditModel
    {
        public string Code { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string DiscountType { get; set; } = null!;

        public decimal Discount { get; set; }

        public decimal? MaxReduction { get; set; }

        public int Quantity { get; set; }

        public decimal MinimumRequire { get; set; }
    }
}
