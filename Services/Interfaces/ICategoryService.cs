using LibraryManagement.DTOs;

namespace LibraryManagement.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(string? search);
        Task<IEnumerable<CategoryDto>> GetCategoryTreeAsync(string? search);
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task DeleteCategoryAsync(int id);
    }
}
