using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Services.AWS.S3.Configurations;
using Services.AWS.S3.Interfaces;

namespace Services.AWS.S3.Services;

public class S3Service : IS3Service, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Configuration _config;
    private bool _disposed;

    public S3Service(IOptions<S3Configuration> config)
    {
        _config = config.Value;

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_config.Region),
        };

        if (_config.UseCredentialsFile)
        {
            _s3Client = new AmazonS3Client(s3Config);
        }
        else
        {
            _s3Client = new AmazonS3Client(_config.AccessKey, _config.SecretKey, s3Config);
        }
    }

    public async Task<PutObjectResponse> UploadFileAsync(
        string key,
        Stream content,
        string contentType
    )
    {
        var request = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        };

        return await _s3Client.PutObjectAsync(request);
    }

    public async Task<GetObjectResponse> DownloadFileAsync(string key)
    {
        var request = new GetObjectRequest { BucketName = _config.BucketName, Key = key };

        return await _s3Client.GetObjectAsync(request);
    }

    public async Task<DeleteObjectResponse> DeleteFileAsync(string key)
    {
        var request = new DeleteObjectRequest { BucketName = _config.BucketName, Key = key };

        return await _s3Client.DeleteObjectAsync(request);
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _config.BucketName,
                Key = key,
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<string> GetPreSignedUrlAsync(string key, int expiryMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _config.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public async Task<List<S3Object>> ListObjectsAsync(string prefix = "", bool recursive = false)
    {
        var results = new List<S3Object>();
        var request = new ListObjectsV2Request
        {
            BucketName = _config.BucketName,
            Prefix = prefix,
            Delimiter = recursive ? null : "/",
        };

        ListObjectsV2Response response;
        do
        {
            response = await _s3Client.ListObjectsV2Async(request);
            results.AddRange(response.S3Objects);
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated ?? false);

        return results;
    }

    public async Task<CopyObjectResponse> CopyObjectAsync(
        string sourceKey,
        string destinationKey,
        string? destinationBucket = null
    )
    {
        var request = new CopyObjectRequest
        {
            SourceBucket = _config.BucketName,
            SourceKey = sourceKey,
            DestinationBucket = destinationBucket ?? _config.BucketName,
            DestinationKey = destinationKey,
        };

        return await _s3Client.CopyObjectAsync(request);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _s3Client?.Dispose();
        }

        _disposed = true;
    }
}
