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

        public int CategoryId { get; set; }

        // ✅ THAY ĐỔI: SeriesId BẮT BUỘC (vì sách lẻ cũng là bộ 1 tập)
        public int SeriesId { get; set; }
        public int VolumeNumber { get; set; } = 1;  // Mặc định tập 1

        // ✅ THÊM: Phiên bản sách
        public BookEdition Edition { get; set; } = BookEdition.Standard;

        public ReadingStatus ReadingStatus { get; set; } = ReadingStatus.NotStarted;
        public int? Rating { get; set; }
        public string? Notes { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public Series Series { get; set; } = null!;
    }

    public enum BookEdition
    {
        Standard = 0,    // Bản thường
        Special = 1,     // Bản đặc biệt
        Limited = 2,     // Bản giới hạn
        Collector = 3    // Bản sưu tầm
    }

    public enum ReadingStatus
    {
        NotStarted = 0,
        Reading = 1,
        Completed = 2
    }
}
