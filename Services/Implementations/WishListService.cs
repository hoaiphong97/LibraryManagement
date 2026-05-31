using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Implementations
{
    public class WishListService : IWishListService
    {
        private readonly IWishListRepository _repo;
        private readonly IMapper _mapper;

        public WishListService(IWishListRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WishListDto>> GetAllAsync()
        {
            var items = await _repo.GetAllWithCategoryAsync();
            return _mapper.Map<IEnumerable<WishListDto>>(items);
        }

        public async Task<WishListDto> GetByIdAsync(int id)
        {
            var item = await _repo.GetAllWithCategoryAsync();
            var found = item.FirstOrDefault(w => w.Id == id)
                ?? throw new NotFoundException($"Không tìm thấy wish list item {id}");
            return _mapper.Map<WishListDto>(found);
        }

        public async Task<WishListDto> CreateAsync(CreateWishListDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new BadRequestException("Tên sách không được để trống");

            var entity = _mapper.Map<WishList>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            var created = await _repo.AddAsync(entity);
            return await GetByIdAsync(created.Id);
        }

        public async Task UpdateAsync(int id, UpdateWishListDto dto)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy wish list item {id}");
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy wish list item {id}");
            await _repo.DeleteAsync(entity);
        }
    }
}
