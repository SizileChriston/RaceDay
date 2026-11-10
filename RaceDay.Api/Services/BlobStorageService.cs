using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public sealed class BlobStorageService(IConfiguration configuration) : IBlobStorageService
{
    public async Task<string> UploadImageAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage:ConnectionString.");

        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";
        var service = new BlobServiceClient(connectionString);
        var container = service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var extension = contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Only PNG, JPEG, and WebP images are accepted.")
        };
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(safeName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);
        return blob.Uri.ToString();
    }
}
