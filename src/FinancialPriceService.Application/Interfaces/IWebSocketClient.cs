namespace FinancialPriceService.Application.Interfaces
{
    public interface IWebSocketClient : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken cancellationToken);
        event Func<string, Task>? MessageReceived;
        Task DisconnectAsync();
    }
}
