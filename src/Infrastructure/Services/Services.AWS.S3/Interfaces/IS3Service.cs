using Amazon.S3.Model;

namespace Services.AWS.S3.Interfaces;

public interface IS3Service
{
    /// <summary>
    /// Upload a file to S3 bucket
    /// </summary>
    /// <param name="key">Object key (file path in bucket)</param>
    /// <param name="content">File content as stream</param>
    /// <param name="contentType">Content type of the file</param>
    /// <returns>Upload result with details</returns>
    Task<PutObjectResponse> UploadFileAsync(string key, Stream content, string contentType);

    /// <summary>
    /// Download a file from S3 bucket
    /// </summary>
    /// <param name="key">Object key (file path in bucket)</param>
    /// <returns>File content as stream</returns>
    Task<GetObjectResponse> DownloadFileAsync(string key);

    /// <summary>
    /// Delete a file from S3 bucket
    /// </summary>
    /// <param name="key">Object key (file path in bucket)</param>
    /// <returns>Delete response</returns>
    Task<DeleteObjectResponse> DeleteFileAsync(string key);

    /// <summary>
    /// Check if a file exists in S3 bucket
    /// </summary>
    /// <param name="key">Object key (file path in bucket)</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string key);

    /// <summary>
    /// Get pre-signed URL for temporary access to a file
    /// </summary>
    /// <param name="key">Object key (file path in bucket)</param>
    /// <param name="expiryMinutes">URL expiry time in minutes</param>
    /// <returns>Pre-signed URL</returns>
    Task<string> GetPreSignedUrlAsync(string key, int expiryMinutes = 60);

    /// <summary>
    /// List objects in a directory (prefix) in S3 bucket
    /// </summary>
    /// <param name="prefix">Directory prefix</param>
    /// <param name="recursive">Whether to list objects recursively</param>
    /// <returns>List of object summaries</returns>
    Task<List<S3Object>> ListObjectsAsync(string prefix = "", bool recursive = false);

    /// <summary>
    /// Copy an object within the same bucket or to another bucket
    /// </summary>
    /// <param name="sourceKey">Source object key</param>
    /// <param name="destinationKey">Destination object key</param>
    /// <param name="destinationBucket">Optional destination bucket name</param>
    /// <returns>Copy response</returns>
    Task<CopyObjectResponse> CopyObjectAsync(
        string sourceKey,
        string destinationKey,
        string? destinationBucket = null
    );
}
