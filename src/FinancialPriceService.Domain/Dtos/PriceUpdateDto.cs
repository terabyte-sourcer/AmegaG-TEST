namespace FinancialPriceService.Domain.Dtos
{
    public class PriceUpdateDto
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
