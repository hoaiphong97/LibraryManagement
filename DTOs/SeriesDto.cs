namespace LibraryManagement.DTOs
{
    public class SeriesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }          // ← THÊM: ghi chú bộ sách
        public int? TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int CurrentVolumes { get; set; }
        public List<int> OwnedVolumes { get; set; } = new();   // ← THÊM: tập đang có
        public List<int> MissingVolumes { get; set; } = new(); // tập thiếu
    }

    public class CreateSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }          // ← THÊM
        public int? TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
    }

    public class UpdateSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }          // ← THÊM
        public int? TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
    }
}
