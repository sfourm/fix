using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Fix.Storage.Application.Abstractions.Storage;
using Fix.Storage.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Fix.Storage.Infrastructure.Storage;

/// <summary>Arquivos originais no S3 (MinIO no ambiente local, com path-style).</summary>
internal sealed class S3ObjectStorage : IObjectStorage, IDisposable
{
    private readonly S3Options options;
    private readonly AmazonS3Client client;
    private readonly AmazonS3Client publicClient;

    public S3ObjectStorage(IOptions<S3Options> options)
    {
        this.options = options.Value;
        client = CreateClient(this.options, this.options.ServiceUrl);
        // O link assinado é calculado localmente; com endpoint público diferente, assina com ele.
        publicClient = string.IsNullOrWhiteSpace(this.options.PublicUrl) || this.options.PublicUrl == this.options.ServiceUrl
            ? client
            : CreateClient(this.options, this.options.PublicUrl);
    }

    public async Task<string> PutAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var stream = new System.IO.MemoryStream(content, writable: false);
        await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = options.Bucket,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false,
        }, cancellationToken);
        return $"s3://{options.Bucket}/{key}";
    }

    public async Task<byte[]> GetAsync(string key, CancellationToken cancellationToken)
    {
        using var response = await client.GetObjectAsync(options.Bucket, key, cancellationToken);
        using var buffer = new System.IO.MemoryStream();
        await response.ResponseStream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }

    public (string Url, DateTimeOffset ExpiresAt) GetDownloadUrl(string key, string fileName, TimeSpan validity)
    {
        var expires = DateTimeOffset.UtcNow.Add(validity);
        var endpoint = options.PublicUrl ?? options.ServiceUrl;
        var request = new GetPreSignedUrlRequest
        {
            BucketName = options.Bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = expires.UtcDateTime,
            Protocol = endpoint?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true ? Protocol.HTTP : Protocol.HTTPS,
        };
        request.ResponseHeaderOverrides.ContentDisposition = "attachment; filename*=UTF-8''" + Uri.EscapeDataString(fileName);
        return (publicClient.GetPreSignedURL(request), expires);
    }

    /// <summary>Cria o bucket se não existir (ambiente local).</summary>
    public async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        try
        {
            await client.PutBucketAsync(new PutBucketRequest { BucketName = options.Bucket }, cancellationToken);
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
        {
        }
    }

    public void Dispose()
    {
        if (!ReferenceEquals(publicClient, client))
        {
            publicClient.Dispose();
        }

        client.Dispose();
    }

    private static AmazonS3Client CreateClient(S3Options options, string? serviceUrl)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = options.ForcePathStyle,
            // MinIO e compatíveis: checksums só quando exigidos.
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
        };
        if (string.IsNullOrWhiteSpace(serviceUrl))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }
        else
        {
            config.ServiceURL = serviceUrl;
            config.AuthenticationRegion = options.Region;
        }

        return string.IsNullOrWhiteSpace(options.AccessKey)
            ? new AmazonS3Client(config)
            : new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
    }
}
