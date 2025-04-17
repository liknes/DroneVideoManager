using Microsoft.AspNetCore.Mvc;
using DroneVideoManager.FlightRecordApi.Services;

namespace DroneVideoManager.FlightRecordApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightRecordController : ControllerBase
    {
        private readonly ILogger<FlightRecordController> _logger;
        private readonly FlightRecordParserService _parserService;

        public FlightRecordController(ILogger<FlightRecordController> logger, FlightRecordParserService parserService)
        {
            _logger = logger;
            _parserService = parserService;
        }

        [HttpPost("parse")]
        public async Task<IActionResult> ParseFlightRecord(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var result = await _parserService.ParseFlightRecordAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing flight record");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
} 