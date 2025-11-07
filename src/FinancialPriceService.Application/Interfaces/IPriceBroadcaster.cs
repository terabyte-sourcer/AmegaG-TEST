namespace FinancialPriceService.Application.Interfaces
{
    public interface IPriceBroadcaster
    {
        Task BroadcastPriceAsync(object update);
    }
}
