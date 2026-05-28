namespace LibraryManagement.Models
{
    public class PreOrder
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int? SeriesId { get; set; }
        public int? VolumeNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public PreOrderStatus Status { get; set; } = PreOrderStatus.Pending;
        public int? BookId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Series? Series { get; set; }
        public Book? Book { get; set; }
    }

    public enum PreOrderStatus
    {
        Pending = 0,
        Arrived = 1,
        Cancelled = 2
    }
}
