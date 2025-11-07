using FinancialPriceService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPriceService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstrumentsController : ControllerBase
    {
        private readonly IInstrumentService _instrumentService;

        public InstrumentsController(IInstrumentService instrumentService)
        {
            _instrumentService = instrumentService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var instruments = _instrumentService.GetAll();
            return Ok(instruments);
        }

        [HttpGet("{symbol}")]
        public IActionResult GetBySymbol(string symbol)
        {
            var instrument = _instrumentService.GetBySymbol(symbol);
            if (instrument == null)
                return NotFound();

            return Ok(instrument);
        }
    }
}
