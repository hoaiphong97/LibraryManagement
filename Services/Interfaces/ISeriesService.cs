using LibraryManagement.DTOs;

namespace LibraryManagement.Services.Interfaces
{
    public interface ISeriesService
    {
        Task<IEnumerable<SeriesDto>> GetAllSeriesAsync(string? search);
        Task<SeriesDto> GetSeriesByIdAsync(int id);
        Task<SeriesDto> CreateSeriesAsync(CreateSeriesDto dto);
        Task UpdateSeriesAsync(int id, UpdateSeriesDto dto);
        Task DeleteSeriesAsync(int id);
        Task ToggleVolumeAsync(int seriesId, ToggleVolumeDto dto);
    }
}
