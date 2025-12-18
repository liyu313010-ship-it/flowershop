using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController(IVideoService service) : ControllerBase
    {
        private readonly IVideoService _service = service;

        [HttpGet("home")]
        public async Task<ActionResult<Video>> GetHome()
        {
            var v = await _service.GetBySlotAsync("story");
            if (v == null) return NotFound();
            return Ok(v);
        }

        [HttpGet("slot/{slot}")]
        public async Task<ActionResult<Video>> GetBySlot(string slot)
        {
            var v = await _service.GetBySlotAsync(slot);
            if (v == null) return NotFound();
            return Ok(v);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Video>> GetById(int id)
        {
            var v = await _service.GetByIdAsync(id);
            if (v == null) return NotFound();
            return Ok(v);
        }

        [HttpPost("upload")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Video>> Upload([FromForm] UploadVideoDto dto)
        {
            if (dto.File == null || dto.File.Length == 0) return BadRequest();
            var v = await _service.UploadAsync(dto.File, dto.Title ?? string.Empty, dto.Slot ?? "story");
            return Ok(v);
        }
    }

    public class UploadVideoDto
    {
        public required IFormFile File { get; set; }
        public string? Title { get; set; }
        public string? Slot { get; set; }
    }
}
