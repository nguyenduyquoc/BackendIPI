namespace Backend_API.ViewModels
{
    public class ProductFiterRequest
    {
        public int? Page { get; set; }

        public int? PageSize { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public List<int>? CategoryIds { get; set; }

        public List<int>? AuthorIds { get; set; }

        public List<int>? PublisherIds { get; set; }

        public List<int>? PublishYears { get; set; }

        public string? SortBy { get; set; }

        public string? SearchQuery { get; set; }

        public int? Status { get; set; }
    }
}
