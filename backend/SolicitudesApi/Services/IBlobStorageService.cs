namespace SolicitudesApi.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

    Uri GenerateReadSasUri(string blobPath, TimeSpan validity);

    Task DeleteAsync(string blobPath, CancellationToken ct = default);
}
