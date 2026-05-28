using LibraryManagement.Models;

namespace LibraryManagement.DTOs
{
    public class PreOrderDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int? SeriesId { get; set; }
        public string? SeriesName { get; set; }
        public int? VolumeNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public PreOrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public int? BookId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePreOrderDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int? SeriesId { get; set; }
        public int? VolumeNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedDate { get; set; }
    }

    public class UpdatePreOrderDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int? SeriesId { get; set; }
        public int? VolumeNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public PreOrderStatus Status { get; set; }
    }

    public class ShelvePreOrderDto
    {
        public int? SeriesId { get; set; }       // bắt buộc nếu PreOrder chưa có SeriesId
        public int? VolumeNumber { get; set; }    // override số tập nếu cần
        public BookEdition Edition { get; set; } = BookEdition.Standard;
    }
}
