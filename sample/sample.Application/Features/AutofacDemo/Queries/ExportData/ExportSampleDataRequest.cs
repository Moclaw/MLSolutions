using MediatR;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Queries.ExportData;

public class ExportSampleDataRequest : IRequest<Response<ExportDataResponse>>
{
}
