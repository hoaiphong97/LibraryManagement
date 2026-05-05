using LibraryManagement.DTOs;

namespace LibraryManagement.Services.Interfaces
{
    public interface IImportService
    {
        Task<ImportResultDto> ImportBooksFromExcelAsync(Stream fileStream);
    }
}
