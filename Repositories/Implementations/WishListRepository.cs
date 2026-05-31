using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations
{
    public class WishListRepository : Repository<WishList>, IWishListRepository
    {
        public WishListRepository(BookDbContext context) : base(context) { }

        public async Task<IEnumerable<WishList>> GetAllWithCategoryAsync()
        {
            return await _dbSet
                .Include(w => w.Category)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }
    }
}
