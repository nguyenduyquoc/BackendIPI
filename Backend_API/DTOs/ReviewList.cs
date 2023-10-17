namespace Backend_API.DTOs
{
    public class ReviewList
    {
        public List<ReviewDTO>? Reviews { get; set; }

        public int? TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
