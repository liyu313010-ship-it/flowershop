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
        public async Task<ActionResult<Video>> Upload([FromForm] IFormFile file, [FromForm] string title, [FromForm] string slot)
        {
            if (file == null || file.Length == 0) return BadRequest();
            var v = await _service.UploadAsync(file, title ?? string.Empty, slot ?? "story");
            return Ok(v);
        }
    }
}
