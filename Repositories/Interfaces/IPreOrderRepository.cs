using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces
{
    public interface IPreOrderRepository : IRepository<PreOrder>
    {
        Task<PreOrder?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<PreOrder>> GetAllWithDetailsAsync(PreOrderStatus? status);
    }
}
