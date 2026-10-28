using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace SolicitudesApi.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _container;
    private readonly string _accountName;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration cfg)
    {
        var serviceUrl = cfg["AzureStorage:ServiceUrl"];
        _containerName = cfg["AzureStorage:ContainerName"] ?? "solicitudes-docs";

        if (string.IsNullOrWhiteSpace(serviceUrl))
            throw new InvalidOperationException(
                "Falta AzureStorage:ServiceUrl (p.ej. https://<cuenta>.blob.core.windows.net). " +
                "Las credenciales se obtienen vía DefaultAzureCredential — no uses claves.");

        var uri = new Uri(serviceUrl);
        _accountName = uri.Host.Split('.')[0];

        _serviceClient = new BlobServiceClient(uri, new DefaultAzureCredential());
        _container = _serviceClient.GetBlobContainerClient(_containerName);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var safeName = Path.GetFileName(fileName).Replace(" ", "_");
        var blobPath = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}_{safeName}";
        var blob = _container.GetBlobClient(blobPath);

        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
        return blobPath;
    }

    public Uri GenerateReadSasUri(string blobPath, TimeSpan validity)
    {
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-2);
        var expiresOn = DateTimeOffset.UtcNow.Add(validity);

        var userDelegationKey = _serviceClient
            .GetUserDelegationKey(startsOn, expiresOn);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobPath,
            Resource = "b",
            StartsOn = startsOn,
            ExpiresOn = expiresOn,
            Protocol = SasProtocol.Https
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasParameters = sasBuilder.ToSasQueryParameters(userDelegationKey.Value, _accountName);

        var blob = _container.GetBlobClient(blobPath);
        var uriBuilder = new BlobUriBuilder(blob.Uri) { Sas = sasParameters };
        return uriBuilder.ToUri();
    }

    public async Task DeleteAsync(string blobPath, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobPath);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
