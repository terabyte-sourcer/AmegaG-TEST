using FinancialPriceService.Application.Services;
using FinancialPriceService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinancialPriceService.Tests
{
    public class InstrumentServiceTests
    {
        [Fact]
        public void UpsertPrice_AddsNewInstrument_WhenNotExists()
        {
            var logger = Mock.Of<ILogger<InstrumentService>>();
            var svc = new InstrumentService(logger);

            svc.UpsertPrice("NEW", 123.45m, DateTimeOffset.UtcNow);

            var inst = svc.GetBySymbol("NEW");
            Assert.NotNull(inst);
            Assert.Equal(123.45m, inst!.Price);
        }

        [Fact]
        public void UpsertPrice_UpdatesExistingInstrument()
        {
            var logger = Mock.Of<ILogger<InstrumentService>>();
            var svc = new InstrumentService(logger);

            svc.UpsertPrice("BTCUSD", 60000m, DateTimeOffset.UtcNow);
            var inst = svc.GetBySymbol("BTCUSD");
            Assert.NotNull(inst);
            Assert.Equal(60000m, inst!.Price);
        }
    }
}
