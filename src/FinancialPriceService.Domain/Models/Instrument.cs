namespace FinancialPriceService.Domain.Models
{
    public class Instrument
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
