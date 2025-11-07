using System.Text.Json.Serialization;

namespace FinancialPriceService.Infrastructure.WebSocket
{
    // Partial mapping for Binance aggTrade message
    public class BinanceAggTradeDto
    {
        [JsonPropertyName("e")] public string EventType { get; set; } = string.Empty;
        [JsonPropertyName("E")] public long EventTime { get; set; }
        [JsonPropertyName("s")] public string Symbol { get; set; } = string.Empty;
        [JsonPropertyName("p")] public string Price { get; set; } = "0";
        [JsonPropertyName("q")] public string Quantity { get; set; } = "0";
        [JsonPropertyName("T")] public long TradeTime { get; set; }
    }
}
