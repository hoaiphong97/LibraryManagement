using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces
{
    public interface IWishListRepository : IRepository<WishList>
    {
        Task<IEnumerable<WishList>> GetAllWithCategoryAsync();
    }
}
