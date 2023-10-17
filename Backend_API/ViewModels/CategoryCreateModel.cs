namespace Backend_API.ViewModels
{
    public class CategoryCreateModel
    {
        public string Name { get; set; } = null!;

        public int? ParentId { get; set; }
    }
}
