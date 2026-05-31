using LibraryManagement.DTOs;

namespace LibraryManagement.Services.Interfaces
{
    public interface IWishListService
    {
        Task<IEnumerable<WishListDto>> GetAllAsync();
        Task<WishListDto> GetByIdAsync(int id);
        Task<WishListDto> CreateAsync(CreateWishListDto dto);
        Task UpdateAsync(int id, UpdateWishListDto dto);
        Task DeleteAsync(int id);
    }
}
