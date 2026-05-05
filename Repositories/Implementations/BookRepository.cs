using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(BookDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> GetBooksWithDetailsAsync()
        {
            return await _dbSet
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)  // ← THÊM
                .Include(b => b.Series)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<Book?> GetBookWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)  // ← THÊM
                .Include(b => b.Series)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
        public async Task<IEnumerable<Book>> SearchBooksAsync(
            string? search,
            int? categoryId,
            int? seriesId,
            ReadingStatus? status)
        {
            var query = _dbSet
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)  // ← THÊM: load parent
                .Include(b => b.Series)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    (b.Author != null && b.Author.Contains(search)) ||
                    (b.Publisher != null && b.Publisher.Contains(search)));

            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId.Value);

            if (seriesId.HasValue)
                query = query.Where(b => b.SeriesId == seriesId.Value);

            if (status.HasValue)
                query = query.Where(b => b.ReadingStatus == status.Value);

            return await query.OrderBy(b => b.Title).ToListAsync();
        }
    }
}
