using FinancialPriceService.Application.Interfaces;
using FinancialPriceService.Domain.Dtos;
using FinancialPriceService.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Options;
using FinancialPriceService.Api.Hubs; // forward reference - hub lives in Api project (we'll use interface, careful in DI)

namespace FinancialPriceService.Application.Services
{
    /// <summary>
    /// Background service that listens to market-data websocket and broadcasts to SignalR clients.
    /// </summary>
    public class PriceBroadcastService : BackgroundService
    {
        private readonly ILogger<PriceBroadcastService> _logger;
        private readonly IWebSocketClient _webSocketClient;
        private readonly IInstrumentService _instrumentService;
        private readonly IHubContext<PriceHub> _hubContext;
        private readonly Channel<PriceUpdateDto> _channel;

        public PriceBroadcastService(
            ILogger<PriceBroadcastService> logger,
            IWebSocketClient webSocketClient,
            IInstrumentService instrumentService,
            IHubContext<PriceHub> hubContext)
        {
            _logger = logger;
            _webSocketClient = webSocketClient;
            _instrumentService = instrumentService;
            _hubContext = hubContext;

            // single-producer multiple-consumer channel; bounded to avoid runaway memory growth
            _channel = Channel.CreateBounded<PriceUpdateDto>(new BoundedChannelOptions(1024)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });

            _webSocketClient.MessageReceived += OnMessageReceived;
        }

        private async Task OnMessageReceived(string raw)
        {
            // quick parse: handle only aggTrade messages with price p
            try
            {
                var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("p", out var pElem) &&
                    doc.RootElement.TryGetProperty("s", out var sElem))
                {
                    var symbol = sElem.GetString() ?? "UNKNOWN";
                    var pStr = pElem.GetString() ?? "0";
                    if (decimal.TryParse(pStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price))
                    {
                        var update = new PriceUpdateDto { Symbol = symbol.ToUpperInvariant(), Price = price, Timestamp = DateTimeOffset.UtcNow };
                        // enqueue
                        await _channel.Writer.WriteAsync(update);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse websocket message");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start websocket
            try
            {
                await _webSocketClient.ConnectAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect websocket client");
                throw; // let host handle restart policies
            }

            // consumer loop
            var reader = _channel.Reader;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var update = await reader.ReadAsync(stoppingToken);

                    // update local instrument store
                    _instrumentService.UpsertPrice(update.Symbol, update.Price, update.Timestamp);

                    // broadcast to SignalR clients; non-blocking
                    _ = _hubContext.Clients.All.SendAsync("ReceivePrice", update, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling price update");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _webSocketClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error stopping websocket client");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
