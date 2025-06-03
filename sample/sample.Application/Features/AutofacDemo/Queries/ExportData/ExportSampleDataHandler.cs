using MediatR;
using sample.Infrastructure.Services;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.AutofacDemo.Queries.ExportData;

public class ExportSampleDataHandler(IDataExportService exportService) 
    : IRequestHandler<ExportSampleDataRequest, Response<ExportDataResponse>>
{
    public async Task<Response<ExportDataResponse>> Handle(ExportSampleDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sampleData = new[]
            {
                new { Id = 1, Name = "Sample Item 1", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new { Id = 2, Name = "Sample Item 2", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new { Id = 3, Name = "Sample Item 3", CreatedAt = DateTime.UtcNow.AddDays(-1) }
            };
            
            var jsonData = await exportService.ExportToJsonAsync(sampleData);
            var jsonString = System.Text.Encoding.UTF8.GetString(jsonData);
            
            return ResponseUtils.Success(new ExportDataResponse
            {
                Data = jsonString,
                ExportedAt = DateTime.UtcNow,
                Format = "JSON",
                ItemCount = sampleData.Length
            }, "Data exported successfully");
        }
        catch (Exception ex)
        {
            return ResponseUtils.Error<ExportDataResponse>(400, $"Error exporting data: {ex.Message}");
        }
    }
}
