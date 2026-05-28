using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Implementations
{
    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

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
            if (dto.TotalVolumes < 1)
                throw new BadRequestException("Số tập phải >= 1");

            if (dto.OwnedVolumeNumbers.Any(v => v < 1 || v > dto.TotalVolumes))
                throw new BadRequestException($"Số tập đã có phải từ 1 đến {dto.TotalVolumes}");

            var series = _mapper.Map<Series>(dto);
            var createdSeries = await _seriesRepository.AddAsync(series);

            // Tạo Book record cho mỗi tập đã đánh dấu là có
            foreach (var volumeNumber in dto.OwnedVolumeNumbers.Distinct().OrderBy(v => v))
            {
                var book = new Book
                {
                    Title = dto.TotalVolumes == 1
                        ? dto.Name
                        : $"{dto.Name} - Tập {volumeNumber}",
                    Author = dto.Author,
                    CategoryId = dto.CategoryId,
                    SeriesId = createdSeries.Id,
                    VolumeNumber = volumeNumber,
                    Edition = BookEdition.Standard,
                    ReadingStatus = ReadingStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _bookRepository.AddAsync(book);
            }

            return await GetSeriesByIdAsync(createdSeries.Id);
        }

        public async Task ToggleVolumeAsync(int seriesId, ToggleVolumeDto dto)
        {
            var series = await _seriesRepository.GetSeriesWithBooksAsync(seriesId)
                ?? throw new NotFoundException($"Không tìm thấy bộ sách {seriesId}");

            if (dto.VolumeNumber < 1 || dto.VolumeNumber > series.TotalVolumes)
                throw new BadRequestException($"Tập phải từ 1 đến {series.TotalVolumes}");

            var existingBook = series.Books
                .FirstOrDefault(b => b.VolumeNumber == dto.VolumeNumber && b.Edition == dto.Edition);

            if (existingBook != null)
            {
                await _bookRepository.DeleteAsync(existingBook);
            }
            else
            {
                var firstBook = series.Books.FirstOrDefault();
                var categoryId = series.CategoryId ?? firstBook?.CategoryId ?? 1;
                var book = new Book
                {
                    Title = series.TotalVolumes == 1
                        ? series.Name
                        : $"{series.Name} - Tập {dto.VolumeNumber}",
                    Author = series.Author,
                    CategoryId = categoryId,
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

            // Cascade delete tự động xóa Books liên quan (configured in DB)
            await _seriesRepository.DeleteAsync(series);
        }
    }
}
