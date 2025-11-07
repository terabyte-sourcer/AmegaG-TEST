namespace FinancialPriceService.Application.Interfaces
{
    /// <summary>
    /// Abstracts a websocket client (server-provider) so it can be mocked in tests.
    /// Emits raw JSON messages.
    /// </summary>
    public interface IWebSocketClient : IAsyncDisposable
    {
        /// <summary>
        /// Connects and begins receiving messages.
        /// </summary>
        Task ConnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Subscribes to raw JSON messages pushed by provider.
        /// </summary>
        event Func<string, Task>? MessageReceived;

        /// <summary>
        /// Graceful disconnect.
        /// </summary>
        Task DisconnectAsync();
    }
}
