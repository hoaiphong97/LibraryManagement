using LibraryManagement.Models;

namespace LibraryManagement.DTOs
{
    public class SeriesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public int TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public bool IsOngoing { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int CurrentVolumes { get; set; }
        public List<int> OwnedVolumes { get; set; } = new();
        public List<int> MissingVolumes { get; set; } = new();
        public List<VolumeInfoDto> Volumes { get; set; } = new();
    }

    public class VolumeInfoDto
    {
        public int VolumeNumber { get; set; }
        public bool IsOwned { get; set; }
        public List<BookEditionDto> Editions { get; set; } = new();
    }

    public class BookEditionDto
    {
        public int BookId { get; set; }
        public BookEdition Edition { get; set; }
        public string EditionText { get; set; } = string.Empty;
        public ReadingStatus ReadingStatus { get; set; }
    }

    // ✅ MỚI: DTO tạo bộ với danh sách tập đã có
    public class CreateSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public int TotalVolumes { get; set; } = 1;
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public bool IsOngoing { get; set; } = false;
        public int CategoryId { get; set; }  // ✅ Thể loại của bộ
        public List<int> OwnedVolumeNumbers { get; set; } = new();  // ✅ Tập đã có
    }

    public class UpdateSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public int TotalVolumes { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public bool IsOngoing { get; set; }
        public int? CategoryId { get; set; }
    }

    // ✅ MỚI: DTO để toggle 1 tập (đã có / chưa có)
    public class ToggleVolumeDto
    {
        public int VolumeNumber { get; set; }
        public BookEdition Edition { get; set; } = BookEdition.Standard;
    }
}
