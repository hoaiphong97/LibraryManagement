using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations
{
    public class SeriesRepository : Repository<Series>, ISeriesRepository
    {
        public SeriesRepository(BookDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Series>> GetSeriesWithBooksAsync()
        {
            return await _dbSet
                .Include(s => s.Books)
                .ToListAsync();
        }

        public async Task<Series?> GetSeriesWithBooksAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Series>> SearchSeriesAsync(string? search)
        {
            var query = _dbSet
                .Include(s => s.Books)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.Name.Contains(search) ||
                    (s.Author != null && s.Author.Contains(search)) ||
                    (s.Description != null && s.Description.Contains(search)));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> HasBooksAsync(int seriesId)
        {
            return await _context.Books.AnyAsync(b => b.SeriesId == seriesId);
        }
    }
}
