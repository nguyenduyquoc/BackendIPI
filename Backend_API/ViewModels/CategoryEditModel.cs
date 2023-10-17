namespace Backend_API.ViewModels
{
    public class CategoryEditModel
    {
        public string Name { get; set; } = null!;

        public int? ParentId { get; set; }
    }
}
