using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Implementations;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Implementations
{
    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IMapper _mapper;

        private readonly IBookRepository _bookRepository;

        public SeriesService(ISeriesRepository seriesRepository, IBookRepository bookRepository, IMapper mapper)
        {
            _seriesRepository = seriesRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SeriesDto>> GetAllSeriesAsync(string? search)
        {
            var series = await _seriesRepository.SearchSeriesAsync(search);
            return _mapper.Map<IEnumerable<SeriesDto>>(series);
        }

        public async Task<SeriesDto> GetSeriesByIdAsync(int id)
        {
            var series = await _seriesRepository.GetSeriesWithBooksAsync(id);

            if (series == null)
                throw new NotFoundException($"Không tìm thấy bộ sách với ID {id}");

            return _mapper.Map<SeriesDto>(series);
        }

        public async Task<SeriesDto> CreateSeriesAsync(CreateSeriesDto dto)
        {
            // Validate
            if (dto.TotalVolumes < 1)
                throw new BadRequestException("Số tập phải >= 1");

            if (dto.OwnedVolumeNumbers.Any(v => v < 1 || v > dto.TotalVolumes))
                throw new BadRequestException($"Số tập đã có phải từ 1 đến {dto.TotalVolumes}");

            // Tạo Series
            var series = _mapper.Map<Series>(dto);
            var createdSeries = await _seriesRepository.AddAsync(series);

            // ✅ Tự động tạo Book cho mỗi tập đã đánh dấu là "có"
            foreach (var volumeNumber in dto.OwnedVolumeNumbers.Distinct())
            {
                var book = new Book
                {
                    Title = $"{dto.Name} - Tập {volumeNumber}",
                    Author = dto.Author,
                    CategoryId = dto.CategoryId,
                    SeriesId = createdSeries.Id,
                    VolumeNumber = volumeNumber,
                    Edition = BookEdition.Standard,
                    ReadingStatus = ReadingStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _bookRepository.AddAsync(book); // hoặc dùng AddAsync nếu cần
            }

            return await GetSeriesByIdAsync(createdSeries.Id);
        }

        // ✅ MỚI: Toggle 1 tập (đánh dấu có/không có)
        public async Task ToggleVolumeAsync(int seriesId, ToggleVolumeDto dto)
        {
            var series = await _seriesRepository.GetSeriesWithBooksAsync(seriesId)
                ?? throw new NotFoundException($"Không tìm thấy bộ sách {seriesId}");

            if (dto.VolumeNumber < 1 || dto.VolumeNumber > series.TotalVolumes)
                throw new BadRequestException($"Tập phải từ 1 đến {series.TotalVolumes}");

            // Tìm book existing với volume + edition này
            var existingBook = series.Books
                .FirstOrDefault(b => b.VolumeNumber == dto.VolumeNumber && b.Edition == dto.Edition);

            if (existingBook != null)
            {
                // Đã có → xóa (đánh dấu không có)
                await _bookRepository.DeleteAsync(existingBook);
            }
            else
            {
                // Chưa có → tạo mới (đánh dấu là đã có)
                var firstBook = series.Books.FirstOrDefault();
                var book = new Book
                {
                    Title = $"{series.Name} - Tập {dto.VolumeNumber}",
                    Author = series.Author,
                    CategoryId = firstBook?.CategoryId ?? 1,  // Lấy category từ book khác trong bộ
                    SeriesId = seriesId,
                    VolumeNumber = dto.VolumeNumber,
                    Edition = dto.Edition,
                    ReadingStatus = ReadingStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _bookRepository.AddAsync(book);
            }
        }

        public async Task UpdateSeriesAsync(int id, UpdateSeriesDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(id);

            if (series == null)
                throw new NotFoundException($"Không tìm thấy bộ sách với ID {id}");

            _mapper.Map(dto, series);
            await _seriesRepository.UpdateAsync(series);
        }

        public async Task DeleteSeriesAsync(int id)
        {
            var series = await _seriesRepository.GetByIdAsync(id);

            if (series == null)
                throw new NotFoundException($"Không tìm thấy bộ sách với ID {id}");

            if (await _seriesRepository.HasBooksAsync(id))
                throw new BadRequestException("Không thể xóa bộ sách đang có sách");

            await _seriesRepository.DeleteAsync(series);
        }
    }
}
