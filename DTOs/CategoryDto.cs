namespace LibraryManagement.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? Description { get; set; }
        public int BookCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? Description { get; set; }
    }
}
