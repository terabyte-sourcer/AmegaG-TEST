using FinancialPriceService.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace FinancialPriceService.Application.Services
{
    public class PriceBroadcastService : BackgroundService
    {
        private readonly ILogger<PriceBroadcastService> _logger;
        private readonly IWebSocketClient _webSocketClient;
        private readonly IInstrumentService _instrumentService;
        private readonly IPriceBroadcaster _broadcaster;
        private readonly Channel<object> _channel;

        public PriceBroadcastService(
            ILogger<PriceBroadcastService> logger,
            IWebSocketClient webSocketClient,
            IInstrumentService instrumentService,
            IPriceBroadcaster broadcaster)
        {
            _logger = logger;
            _webSocketClient = webSocketClient;
            _instrumentService = instrumentService;
            _broadcaster = broadcaster;

            _channel = Channel.CreateBounded<object>(new BoundedChannelOptions(1024)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });

            _webSocketClient.MessageReceived += async (msg) =>
            {
                await _channel.Writer.WriteAsync(msg);
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _webSocketClient.ConnectAsync(stoppingToken);

            var reader = _channel.Reader;
            while (!stoppingToken.IsCancellationRequested)
            {
                var update = await reader.ReadAsync(stoppingToken);

                await _broadcaster.BroadcastPriceAsync(update);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _webSocketClient.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
