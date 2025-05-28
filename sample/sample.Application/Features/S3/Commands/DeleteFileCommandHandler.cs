using MediatR;
using Services.AWS.S3.Interfaces;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.S3.Commands;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Response<DeleteFileResponse>>
{
    private readonly IS3Service _s3Service;

    public DeleteFileCommandHandler(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<Response<DeleteFileResponse>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _s3Service.FileExistsAsync(request.Key);
            if (!exists)
            {
                var notFoundResponse = new DeleteFileResponse(
                    Key: request.Key,
                    Success: false,
                    Message: "File not found"
                );
                return ResponseUtils.Error(400, "Files retrieved successfully", notFoundResponse);

            }

            await _s3Service.DeleteFileAsync(request.Key);

            var successResponse = new DeleteFileResponse(
                Key: request.Key,
                Success: true,
                Message: "File deleted successfully"
            );
            return ResponseUtils.Success(successResponse, "File deleted successfully");

        }
        catch (Exception ex)
        {
            var errorResponse = new DeleteFileResponse(
                Key: request.Key,
                Success: false,
                Message: ex.Message
            );
            return ResponseUtils.Error(500, "An error occurred while deleting the file", errorResponse);
        }
    }
}
