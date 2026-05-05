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
        private readonly IMapper _mapper;

        public SeriesService(ISeriesRepository seriesRepository, IMapper mapper)
        {
            _seriesRepository = seriesRepository;
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
            var series = _mapper.Map<Series>(dto);
            var createdSeries = await _seriesRepository.AddAsync(series);

            return await GetSeriesByIdAsync(createdSeries.Id);
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
