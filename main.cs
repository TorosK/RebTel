// C# .NET Core 8 Minimal REST API — "CallRateService"

// File: Program.cs
// A minimal REST API to fetch international calling rates (mocked) for Rebtel users.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services for DI
builder.Services.AddSingleton<ICallRateService, CallRateService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rebtel Call Rate API",
        Version = "v1"
    });
});

var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Welcome to Rebtel's Call Rate API");

// Get call rate by country
app.MapGet("/api/callrate/{countryCode}", async (string countryCode, ICallRateService service) =>
{
    var rate = await service.GetCallRateAsync(countryCode);
    return rate is null
        ? Results.NotFound($"No rate found for country code '{countryCode}'.")
        : Results.Ok(rate);
})
.WithName("GetCallRate")
.WithOpenApi();

app.Run();

// ---- Service Layer ----
public interface ICallRateService
{
    Task<CallRateDto?> GetCallRateAsync(string countryCode);
}

public class CallRateService : ICallRateService
{
    // Mock data: Ideally this would come from MSSQL or an external API
    private static readonly Dictionary<string, CallRateDto> _mockRates = new()
    {
        { "SE", new CallRateDto("SE", "Sweden", 0.03m) },
        { "TH", new CallRateDto("TH", "Thailand", 0.05m) },
        { "US", new CallRateDto("US", "United States", 0.02m) },
    };

    public Task<CallRateDto?> GetCallRateAsync(string countryCode)
    {
        _mockRates.TryGetValue(countryCode.ToUpperInvariant(), out var result);
        return Task.FromResult(result);
    }
}

// ---- DTO (Data Transfer Object) ----
public record CallRateDto(string CountryCode, string CountryName, decimal RatePerMinute);
