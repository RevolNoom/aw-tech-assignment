using AwingTest.Server.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AwingTest.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly AwingTestDbContext _dbContext;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            AwingTestDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "weatherforecast")]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
        {
            var forecasts = await _dbContext.WeatherForecasts.ToListAsync();
            return Ok(forecasts);
        }
    }
}