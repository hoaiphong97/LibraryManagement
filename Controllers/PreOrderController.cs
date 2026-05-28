using LibraryManagement.DTOs;
using LibraryManagement.Models;
using LibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreOrderController : ControllerBase
    {
        private readonly IPreOrderService _preOrderService;

        public PreOrderController(IPreOrderService preOrderService)
        {
            _preOrderService = preOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PreOrderDto>>> GetAll([FromQuery] PreOrderStatus? status)
        {
            var preOrders = await _preOrderService.GetAllAsync(status);
            return Ok(preOrders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PreOrderDto>> GetById(int id)
        {
            var preOrder = await _preOrderService.GetByIdAsync(id);
            return Ok(preOrder);
        }

        [HttpPost]
        public async Task<ActionResult<PreOrderDto>> Create(CreatePreOrderDto dto)
        {
            var preOrder = await _preOrderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = preOrder.Id }, preOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePreOrderDto dto)
        {
            await _preOrderService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _preOrderService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/shelve")]
        public async Task<ActionResult<BookDto>> Shelve(int id, ShelvePreOrderDto dto)
        {
            var book = await _preOrderService.ShelveAsync(id, dto);
            return Ok(book);
        }
    }
}
