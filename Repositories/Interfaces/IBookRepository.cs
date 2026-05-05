using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksWithDetailsAsync();
        Task<Book?> GetBookWithDetailsAsync(int id);
        Task<IEnumerable<Book>> SearchBooksAsync(string? search, int? categoryId, int? seriesId, ReadingStatus? status);
    }
}
