using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Implementations
{
    public class PreOrderService : IPreOrderService
    {
        private readonly IPreOrderRepository _preOrderRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public PreOrderService(
            IPreOrderRepository preOrderRepository,
            ISeriesRepository seriesRepository,
            IBookRepository bookRepository,
            IMapper mapper)
        {
            _preOrderRepository = preOrderRepository;
            _seriesRepository = seriesRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PreOrderDto>> GetAllAsync(PreOrderStatus? status)
        {
            var preOrders = await _preOrderRepository.GetAllWithDetailsAsync(status);
            return _mapper.Map<IEnumerable<PreOrderDto>>(preOrders);
        }

        public async Task<PreOrderDto> GetByIdAsync(int id)
        {
            var preOrder = await _preOrderRepository.GetByIdWithDetailsAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy pre-order với ID {id}");

            return _mapper.Map<PreOrderDto>(preOrder);
        }

        public async Task<PreOrderDto> CreateAsync(CreatePreOrderDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new BadRequestException("Tên sách không được để trống");

            if (dto.SeriesId.HasValue && !await _seriesRepository.ExistsAsync(dto.SeriesId.Value))
                throw new BadRequestException($"Không tìm thấy bộ sách với ID {dto.SeriesId.Value}");

            var preOrder = _mapper.Map<PreOrder>(dto);
            preOrder.Status = PreOrderStatus.Pending;
            preOrder.CreatedAt = DateTime.UtcNow;
            preOrder.UpdatedAt = DateTime.UtcNow;

            var created = await _preOrderRepository.AddAsync(preOrder);
            return await GetByIdAsync(created.Id);
        }

        public async Task UpdateAsync(int id, UpdatePreOrderDto dto)
        {
            var preOrder = await _preOrderRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy pre-order với ID {id}");

            if (preOrder.Status == PreOrderStatus.Arrived)
                throw new BadRequestException("Không thể sửa pre-order đã lên kệ");

            if (dto.SeriesId.HasValue && !await _seriesRepository.ExistsAsync(dto.SeriesId.Value))
                throw new BadRequestException($"Không tìm thấy bộ sách với ID {dto.SeriesId.Value}");

            _mapper.Map(dto, preOrder);
            preOrder.UpdatedAt = DateTime.UtcNow;

            await _preOrderRepository.UpdateAsync(preOrder);
        }

        public async Task DeleteAsync(int id)
        {
            var preOrder = await _preOrderRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy pre-order với ID {id}");

            if (preOrder.Status == PreOrderStatus.Arrived)
                throw new BadRequestException("Không thể xóa pre-order đã lên kệ");

            await _preOrderRepository.DeleteAsync(preOrder);
        }

        public async Task<BookDto> ShelveAsync(int id, ShelvePreOrderDto dto)
        {
            var preOrder = await _preOrderRepository.GetByIdWithDetailsAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy pre-order với ID {id}");

            if (preOrder.Status != PreOrderStatus.Pending)
                throw new BadRequestException("Chỉ có thể lên kệ pre-order đang ở trạng thái chờ");

            // SeriesId ưu tiên: từ dto trước, nếu không có thì lấy từ PreOrder
            var seriesId = dto.SeriesId ?? preOrder.SeriesId
                ?? throw new BadRequestException("Cần cung cấp SeriesId để lên kệ (pre-order chưa được liên kết với bộ sách nào)");

            var series = await _seriesRepository.GetSeriesWithBooksAsync(seriesId)
                ?? throw new NotFoundException($"Không tìm thấy bộ sách với ID {seriesId}");

            var volumeNumber = dto.VolumeNumber ?? preOrder.VolumeNumber ?? 1;

            if (volumeNumber < 1 || volumeNumber > (series.TotalVolumes ?? int.MaxValue))
                throw new BadRequestException($"Số tập không hợp lệ");

            // Kiểm tra tập + edition này đã có trên kệ chưa
            var duplicate = series.Books.FirstOrDefault(b => b.VolumeNumber == volumeNumber && b.Edition == dto.Edition);
            if (duplicate != null)
                throw new BadRequestException($"Tập {volumeNumber} ({dto.Edition}) đã có trên kệ");

            var categoryId = series.CategoryId ?? series.Books.FirstOrDefault()?.CategoryId ?? 1;

            var book = new Book
            {
                Title = (series.TotalVolumes ?? 1) == 1
                    ? series.Name
                    : $"{series.Name} - Tập {volumeNumber}",
                Author = preOrder.Author ?? series.Author,
                Publisher = preOrder.Publisher ?? series.Publisher,
                CategoryId = categoryId,
                SeriesId = series.Id,
                VolumeNumber = volumeNumber,
                Edition = dto.Edition,
                ReadingStatus = ReadingStatus.NotStarted,
                Notes = preOrder.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBook = await _bookRepository.AddAsync(book);

            preOrder.Status = PreOrderStatus.Arrived;
            preOrder.BookId = createdBook.Id;
            preOrder.SeriesId = series.Id;
            preOrder.UpdatedAt = DateTime.UtcNow;
            await _preOrderRepository.UpdateAsync(preOrder);

            var bookWithDetails = await _bookRepository.GetBookWithDetailsAsync(createdBook.Id);
            return _mapper.Map<BookDto>(bookWithDetails);
        }
    }
}
