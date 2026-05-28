namespace LibraryManagement.Models
{
    public class Series
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public int? TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public bool IsOngoing { get; set; } = false;
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
