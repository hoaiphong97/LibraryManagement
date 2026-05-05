using LibraryManagement.DTOs;
using LibraryManagement.Models;

namespace LibraryManagement.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(int id);
        Task<IEnumerable<BookDto>> SearchBooksAsync(string? search, int? categoryId, int? seriesId, ReadingStatus? status);
        Task<BookDto> CreateBookAsync(CreateBookDto dto);
        Task UpdateBookAsync(int id, UpdateBookDto dto);
        Task DeleteBookAsync(int id);
    }
}
