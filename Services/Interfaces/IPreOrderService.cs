using LibraryManagement.DTOs;
using LibraryManagement.Models;

namespace LibraryManagement.Services.Interfaces
{
    public interface IPreOrderService
    {
        Task<IEnumerable<PreOrderDto>> GetAllAsync(PreOrderStatus? status);
        Task<PreOrderDto> GetByIdAsync(int id);
        Task<PreOrderDto> CreateAsync(CreatePreOrderDto dto);
        Task UpdateAsync(int id, UpdatePreOrderDto dto);
        Task DeleteAsync(int id);
        Task<BookDto> ShelveAsync(int id, ShelvePreOrderDto dto);
    }
}
