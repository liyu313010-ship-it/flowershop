using Microsoft.AspNetCore.Mvc;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegionsController(IWebHostEnvironment environment, ILogger<RegionsController> logger) : ControllerBase
    {
        private readonly IWebHostEnvironment _environment = environment;
        private readonly ILogger<RegionsController> _logger = logger;

        [HttpGet("pcas")]
        public async Task<IActionResult> GetPcas()
        {
            var dir = Path.Combine(_environment.WebRootPath, "regions");
            var filePath = Path.Combine(dir, "pcas-code.json");

            if (System.IO.File.Exists(filePath))
            {
                var content = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(content, "application/json");
            }

            Directory.CreateDirectory(dir);

            try
            {
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(20);
                var resp = await http.GetAsync("https://unpkg.com/china-division@1.0.0/dist/pcas-code.json");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                await System.IO.File.WriteAllTextAsync(filePath, json);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取行政区划数据失败");
                return StatusCode(500, new { message = "获取行政区划数据失败" });
            }
        }
    }
}
