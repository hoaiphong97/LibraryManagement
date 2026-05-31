namespace LibraryManagement.DTOs
{
    public class WishListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Notes { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateWishListDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Notes { get; set; }
        public int? CategoryId { get; set; }
    }

    public class UpdateWishListDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Notes { get; set; }
        public int? CategoryId { get; set; }
    }
}
