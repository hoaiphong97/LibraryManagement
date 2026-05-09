using LibraryManagement.DTOs;
using LibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeriesDto>>> GetSeries([FromQuery] string? search)
        {
            var series = await _seriesService.GetAllSeriesAsync(search);
            return Ok(series);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SeriesDto>> GetSeries(int id)
        {
            var series = await _seriesService.GetSeriesByIdAsync(id);
            return Ok(series);
        }

        [HttpPost]
        public async Task<ActionResult<SeriesDto>> CreateSeries(CreateSeriesDto dto)
        {
            var series = await _seriesService.CreateSeriesAsync(dto);
            return CreatedAtAction(nameof(GetSeries), new { id = series.Id }, series);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSeries(int id, UpdateSeriesDto dto)
        {
            await _seriesService.UpdateSeriesAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeries(int id)
        {
            await _seriesService.DeleteSeriesAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/toggle-volume")]
        public async Task<IActionResult> ToggleVolume(int id, ToggleVolumeDto dto)
        {
            await _seriesService.ToggleVolumeAsync(id, dto);
            return NoContent();
        }
    }
}
