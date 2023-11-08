namespace Backend_API.ViewModels
{
    public class ReviewCreateModel
    {
        public int OrderProductId { get; set; }

        public decimal Rating { get; set; }

        public string? Comment { get; set; }

    }
}
