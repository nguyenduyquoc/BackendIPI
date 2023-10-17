namespace Backend_API.ViewModels
{
    public class ReviewCreateModel
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public decimal Rating { get; set; }

        public string? Comment { get; set; }

    }
}
