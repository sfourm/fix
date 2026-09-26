namespace Fix.Storage.Application.Abstractions.Storage;

/// <summary>Armazenamento dos arquivos originais (S3; MinIO no ambiente local).</summary>
public interface IObjectStorage
{
    /// <summary>Grava e devolve a URL s3://bucket/chave.</summary>
    Task<string> PutAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken);

    Task<byte[]> GetAsync(string key, CancellationToken cancellationToken);

    /// <summary>Link temporário para baixar com o nome original.</summary>
    (string Url, DateTimeOffset ExpiresAt) GetDownloadUrl(string key, string fileName, TimeSpan validity);
}
