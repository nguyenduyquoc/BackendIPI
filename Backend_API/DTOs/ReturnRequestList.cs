namespace Backend_API.DTOs
{
    public class ReturnRequestList
    {
        public List<ReturnRequestDTO> ReturnRequests { get; set; }

        public int? TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
