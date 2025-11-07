using FinancialPriceService.Domain.Models;

namespace FinancialPriceService.Application.Interfaces
{
    public interface IInstrumentService
    {
        IReadOnlyList<Instrument> GetAll();
        Instrument? GetBySymbol(string symbol);
        void UpsertPrice(string symbol, decimal price, DateTimeOffset timestamp);
    }
}
