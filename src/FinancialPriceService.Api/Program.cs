using FinancialPriceService.Api.Hubs;
using FinancialPriceService.Application.Interfaces;
using FinancialPriceService.Application.Services;
using FinancialPriceService.Infrastructure.WebSocket;
using FinancialPriceService.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddSingleton<IInstrumentService, InstrumentService>();
builder.Services.AddSingleton<IWebSocketClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<BinanceWebSocketClient>>();
    return new BinanceWebSocketClient(logger, "btcusdt"); // <- directly pass symbol here
});
builder.Services.AddSingleton<IPriceBroadcaster, SignalRPriceBroadcaster>();
builder.Services.AddHostedService<PriceBroadcastService>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PriceHub>("/hubs/prices");

app.Run();
