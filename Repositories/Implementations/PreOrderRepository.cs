using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations
{
    public class PreOrderRepository : Repository<PreOrder>, IPreOrderRepository
    {
        public PreOrderRepository(BookDbContext context) : base(context)
        {
        }

        public async Task<PreOrder?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Series)
                .Include(p => p.Book)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PreOrder>> GetAllWithDetailsAsync(PreOrderStatus? status)
        {
            var query = _dbSet
                .Include(p => p.Series)
                .Include(p => p.Book)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
