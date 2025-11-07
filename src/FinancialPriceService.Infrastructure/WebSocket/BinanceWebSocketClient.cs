using FinancialPriceService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FinancialPriceService.Infrastructure.WebSocket
{
    public class BinanceWebSocketClient : IWebSocketClient
    {
        private readonly ILogger<BinanceWebSocketClient> _logger;
        private readonly string _symbol;
        private ClientWebSocket? _ws;
        private readonly Uri _uri;
        private CancellationTokenSource? _internalCts;
        public event Func<string, Task>? MessageReceived;

        public BinanceWebSocketClient(ILogger<BinanceWebSocketClient> logger, string symbol)
        {
            _logger = logger;
            _symbol = symbol.ToLower();
            _uri = new Uri($"wss://stream.binance.com:443/ws/{_symbol}@aggTrade");
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ws = new ClientWebSocket();

            try
            {
                _logger.LogInformation("Connecting to {Uri}", _uri);
                await _ws.ConnectAsync(_uri, _internalCts.Token);

                _ = Task.Run(() => ReceiveLoopAsync(_internalCts.Token), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while connecting to Binance WebSocket");
                throw;
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];

            while (!cancellationToken.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                try
                {
                    var ms = new MemoryStream();
                    WebSocketReceiveResult? result;

                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", cancellationToken);
                            return;
                        }
                        ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    var msg = Encoding.UTF8.GetString(ms.ToArray());

                    if (MessageReceived != null)
                        await MessageReceived.Invoke(msg);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Receive loop error");
                    // Attempt reconnect in caller (PriceBroadcastService) or keep loop and short-delay
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_ws != null)
                {
                    if (_ws.State == WebSocketState.Open)
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);

                    _ws.Dispose();
                    _ws = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during disconnect");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _internalCts?.Cancel();
            _internalCts?.Dispose();
        }
    }
}
