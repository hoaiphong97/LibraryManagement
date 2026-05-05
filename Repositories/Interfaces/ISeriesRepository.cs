using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces
{
    public interface ISeriesRepository : IRepository<Series>
    {
        Task<IEnumerable<Series>> GetSeriesWithBooksAsync();
        Task<Series?> GetSeriesWithBooksAsync(int id);
        Task<IEnumerable<Series>> SearchSeriesAsync(string? search);
        Task<bool> HasBooksAsync(int seriesId);
    }
}
