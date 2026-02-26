namespace NgrApi.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType);
    Task<Stream?> DownloadAsync(string containerName, string blobName);
    Task<bool> DeleteAsync(string containerName, string blobName);
    Task<bool> ExistsAsync(string containerName, string blobName);
}
