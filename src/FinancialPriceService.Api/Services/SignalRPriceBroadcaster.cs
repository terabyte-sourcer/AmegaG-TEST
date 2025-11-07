using FinancialPriceService.Application.Interfaces;
using FinancialPriceService.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FinancialPriceService.Api.Services
{
    public class SignalRPriceBroadcaster : IPriceBroadcaster
    {
        private readonly IHubContext<PriceHub> _hubContext;

        public SignalRPriceBroadcaster(IHubContext<PriceHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastPriceAsync(object update)
        {
            await _hubContext.Clients.All.SendAsync("ReceivePrice", update);
        }
    }
}
