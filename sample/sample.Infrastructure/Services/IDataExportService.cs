using Microsoft.Extensions.Logging;
using Services.Autofac.Attributes;
using sample.Domain.Entities;

namespace sample.Infrastructure.Services;

/// <summary>
/// Service for handling data export operations
/// </summary>
public interface IDataExportService
{
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data);
    Task<byte[]> ExportToJsonAsync<T>(IEnumerable<T> data);
    Task<string> GenerateReportAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Data export service implementation demonstrating generic service registration
/// </summary>
[TransientService]
public class DataExportService : IDataExportService
{
    private readonly ILogger<DataExportService> _logger;

    public DataExportService(ILogger<DataExportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data)
    {
        _logger.LogInformation("Exporting data to CSV format");
        
        // Simulate CSV generation
        await Task.Delay(100);
        
        var csv = "Sample CSV Data\n";
        var result = System.Text.Encoding.UTF8.GetBytes(csv);
        
        _logger.LogInformation("CSV export completed, size: {Size} bytes", result.Length);
        return result;
    }

    public async Task<byte[]> ExportToJsonAsync<T>(IEnumerable<T> data)
    {
        _logger.LogInformation("Exporting data to JSON format");
        
        // Simulate JSON generation
        await Task.Delay(80);
        
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var result = System.Text.Encoding.UTF8.GetBytes(json);
        
        _logger.LogInformation("JSON export completed, size: {Size} bytes", result.Length);
        return result;
    }

    public async Task<string> GenerateReportAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Generating report from {StartDate} to {EndDate}", startDate, endDate);
        
        // Simulate report generation
        await Task.Delay(300);
        
        var report = $"Report generated for period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}\n" +
                    $"Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
                    "Sample report data...";
        
        _logger.LogInformation("Report generation completed");
        return report;
    }
}
