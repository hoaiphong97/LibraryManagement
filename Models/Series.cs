namespace LibraryManagement.Models
{
    public class Series
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }          // ← THÊM
        public int? TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
