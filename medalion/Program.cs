using medalion.Data;
using medalion.Services;
using medalion.Services.Alpaca.Interfaces;
using medalion.Services.Polymarket.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Database configuration
builder.Services.AddDbContext<TradingBotDbContext>(options =>
{
    // Use in-memory database for demo/development
    options.UseInMemoryDatabase("TradingBotDb");
    // For production, use SQL Server or PostgreSQL:
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});

// Register trading services
builder.Services.AddScoped<IDashboardStateService, DashboardStateService>();

// Optional: Register Alpaca and Polymarket services if configured
// builder.Services.AddScoped<IAlpacaApiClient, AlpacaApiClient>();
// builder.Services.AddScoped<IPolymarketWebSocketService, PolymarketWebSocketService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
