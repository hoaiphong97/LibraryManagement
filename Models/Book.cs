namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int? PublishedYear { get; set; }

        // Category (BẮT BUỘC)
        public int CategoryId { get; set; }

        // Series (TÙY CHỌN - chỉ có nếu thuộc bộ sách)
        public int? SeriesId { get; set; }
        public int? VolumeNumber { get; set; }

        public ReadingStatus ReadingStatus { get; set; } = ReadingStatus.NotStarted;
        public int? Rating { get; set; }
        public string? Notes { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Category Category { get; set; } = null!;
        public Series? Series { get; set; }
    }

    public enum ReadingStatus
    {
        NotStarted = 0,
        Reading = 1,
        Completed = 2
    }
}
