using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(string? search)
        {
            var categories = await _categoryRepository.SearchCategoriesAsync(search);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoryTreeAsync(string? search)
        {
            var allCategories = await _categoryRepository.SearchCategoriesAsync(search);
            var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();

            return rootCategories.Select(c => MapToDtoWithChildren(c, allCategories.ToList()));
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryWithDetailsAsync(id);

            if (category == null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID {id}");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Validate parent category exists if provided
            if (dto.ParentId.HasValue && !await _categoryRepository.ExistsAsync(dto.ParentId.Value))
                throw new BadRequestException($"Không tìm thấy thể loại cha với ID {dto.ParentId.Value}");

            var category = _mapper.Map<Category>(dto);
            var createdCategory = await _categoryRepository.AddAsync(category);

            return await GetCategoryByIdAsync(createdCategory.Id);
        }

        public async Task UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID {id}");

            // Validate parent category exists if provided
            if (dto.ParentId.HasValue && !await _categoryRepository.ExistsAsync(dto.ParentId.Value))
                throw new BadRequestException($"Không tìm thấy thể loại cha với ID {dto.ParentId.Value}");

            // Prevent circular reference
            if (dto.ParentId == id)
                throw new BadRequestException("Thể loại không thể là cha của chính nó");

            _mapper.Map(dto, category);
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID {id}");

            if (await _categoryRepository.HasSeriesAsync(id))
                throw new BadRequestException("Không thể xóa thể loại đang có bộ sách. Hãy chuyển các bộ sách sang thể loại khác trước.");

            if (await _categoryRepository.HasSubCategoriesAsync(id))
                throw new BadRequestException("Không thể xóa thể loại đang có thể loại con");

            await _categoryRepository.DeleteAsync(category);
        }

        private CategoryDto MapToDtoWithChildren(Category category, List<Category> allCategories)
        {
            var dto = _mapper.Map<CategoryDto>(category);
            var children = allCategories.Where(c => c.ParentId == category.Id).ToList();
            dto.SubCategories = children.Select(c => MapToDtoWithChildren(c, allCategories)).ToList();
            return dto;
        }
    }
}
