using LibraryManagement.Models;

namespace LibraryManagement.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int? PublishedYear { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryPath { get; set; } // VD: "Văn học > Nước ngoài > Trinh thám"
        public int? SeriesId { get; set; }
        public string? SeriesName { get; set; }
        public int? VolumeNumber { get; set; }
        public ReadingStatus ReadingStatus { get; set; }
        public string ReadingStatusText { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Notes { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateBookDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int? PublishedYear { get; set; }
        public int CategoryId { get; set; }
        public int? SeriesId { get; set; }
        public int? VolumeNumber { get; set; }
        public ReadingStatus ReadingStatus { get; set; }
        public int? Rating { get; set; }
        public string? Notes { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }

    public class UpdateBookDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int? PublishedYear { get; set; }
        public int CategoryId { get; set; }
        public int? SeriesId { get; set; }
        public int? VolumeNumber { get; set; }
        public ReadingStatus ReadingStatus { get; set; }
        public int? Rating { get; set; }
        public string? Notes { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}
