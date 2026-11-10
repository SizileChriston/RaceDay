namespace RaceDay.Api.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}
