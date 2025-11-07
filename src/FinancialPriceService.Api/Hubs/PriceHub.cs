using Microsoft.AspNetCore.SignalR;

namespace FinancialPriceService.Api.Hubs
{
    public class PriceHub : Hub
    {
        public async Task SubscribeToInstrument(string symbol)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, symbol.ToUpperInvariant());
        }

        public async Task UnsubscribeFromInstrument(string symbol)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol.ToUpperInvariant());
        }
    }
}
