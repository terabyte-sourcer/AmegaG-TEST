using System.Threading.Channels;
using System.Threading.Tasks;
using FinancialPriceService.Application.Interfaces;
using FinancialPriceService.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinancialPriceService.Tests
{
    public class PriceBroadcastServiceTests
    {
        [Fact]
        public async Task BroadcastPrice_CallsBroadcaster_WhenMessageReceived()
        {
            var mockLogger = new Mock<ILogger<PriceBroadcastService>>();
            var mockWebSocket = new Mock<IWebSocketClient>();
            var mockInstrumentService = new Mock<IInstrumentService>();
            var mockBroadcaster = new Mock<IPriceBroadcaster>();
            var service = new PriceBroadcastService(
                mockLogger.Object,
                mockWebSocket.Object,
                mockInstrumentService.Object,
                mockBroadcaster.Object
            );
            var cts = new System.Threading.CancellationTokenSource();
            var executeTask = service.StartAsync(cts.Token);
            await service.EnqueueTestMessageAsync("test message");
            await Task.Delay(100);
            mockBroadcaster.Verify(b => b.BroadcastPriceAsync(It.IsAny<object>()), Times.AtLeastOnce);
            cts.Cancel();
            await executeTask;
        }
    }

    internal static class PriceBroadcastServiceTestExtensions
    {
        public static async Task EnqueueTestMessageAsync(this PriceBroadcastService service, object message)
        {
            var channelField = typeof(PriceBroadcastService)
                .GetField("_channel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var channel = (Channel<object>)channelField!.GetValue(service)!;
            await channel.Writer.WriteAsync(message);
        }
    }
}
