using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(BookDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithDetailsAsync()
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Books)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string? search)
        {
            var query = _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Books)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    (c.Description != null && c.Description.Contains(search)));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _dbSet
                .Include(c => c.SubCategories)
                .Include(c => c.Books)
                .Where(c => c.ParentId == null)
                .ToListAsync();
        }

        public async Task<bool> HasBooksAsync(int categoryId)
        {
            return await _context.Books.AnyAsync(b => b.CategoryId == categoryId);
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _dbSet.AnyAsync(c => c.ParentId == categoryId);
        }
    }
}
