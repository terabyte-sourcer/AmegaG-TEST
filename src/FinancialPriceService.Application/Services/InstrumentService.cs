using FinancialPriceService.Application.Interfaces;
using FinancialPriceService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FinancialPriceService.Application.Services
{
    public class InstrumentService : IInstrumentService
    {
        private readonly ILogger<InstrumentService> _logger;
        private readonly Dictionary<string, Instrument> _store = new(StringComparer.OrdinalIgnoreCase);

        public InstrumentService(ILogger<InstrumentService> logger)
        {
            _logger = logger;

            AddOrUpdate(new Instrument { Symbol = "EURUSD", Price = 1.00m, LastUpdated = DateTimeOffset.UtcNow });
            AddOrUpdate(new Instrument { Symbol = "USDJPY", Price = 150.00m, LastUpdated = DateTimeOffset.UtcNow });
            AddOrUpdate(new Instrument { Symbol = "BTCUSD", Price = 50000.00m, LastUpdated = DateTimeOffset.UtcNow });
        }

        private void AddOrUpdate(Instrument inst) => _store[inst.Symbol] = inst;

        public IReadOnlyList<Instrument> GetAll() => _store.Values.ToList();

        public Instrument? GetBySymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return null;
            _store.TryGetValue(symbol, out var inst);
            return inst;
        }

        public void UpsertPrice(string symbol, decimal price, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;
            if (!_store.TryGetValue(symbol, out var inst))
            {
                inst = new Instrument { Symbol = symbol.ToUpperInvariant(), Price = price, LastUpdated = timestamp };
                _store[symbol] = inst;
            }
            else
            {
                inst.Price = price;
                inst.LastUpdated = timestamp;
            }

            _logger.LogInformation("Instrument {Symbol} updated to {Price} at {Timestamp}", symbol, price, timestamp);
        }
    }
}
