using Microsoft.AspNetCore.Mvc;
using OkooraProjectFetcher.BackgroundServices;
using OkooraProjectFetcher.Services;

namespace OkooraProjectFetcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {
        private readonly ExchangeRateBackgroundService _backgroundService;

        public ExchangeController(ExchangeRateBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        [HttpGet]
        [Route("rate")]
        public IActionResult GetLatestRate()
        {
            var rate = _backgroundService.GetLatestRate();
            if (rate == null)
                return BadRequest("Exchange rate not available yet.");

            return Ok(rate);
        }
    }
}
