using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetCategoriesWithDetailsAsync();
        Task<Category?> GetCategoryWithDetailsAsync(int id);
        Task<IEnumerable<Category>> SearchCategoriesAsync(string? search);
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<bool> HasBooksAsync(int categoryId);
        Task<bool> HasSeriesAsync(int categoryId);
        Task<bool> HasSubCategoriesAsync(int categoryId);
    }
}
