namespace Fix.Storage.Infrastructure.Options;

/// <summary>MongoDB (seção "Mongo").</summary>
public sealed class MongoOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    public string Database { get; set; } = "fix_storage";
}

/// <summary>S3 (seção "S3"). No ambiente local aponta para o MinIO.</summary>
public sealed class S3Options
{
    public string Bucket { get; set; } = "fix-files";

    public string Region { get; set; } = "us-east-1";

    /// <summary>Endpoint (MinIO). Vazio = AWS.</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Endpoint dos links de download (o navegador não enxerga o nome interno do contêiner).</summary>
    public string? PublicUrl { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    public bool ForcePathStyle { get; set; }

    /// <summary>Cria o bucket na subida (só no ambiente local).</summary>
    public bool CreateBucket { get; set; }
}

/// <summary>RabbitMQ (seção "RabbitMq").</summary>
public sealed class RabbitMqOptions
{
    public string Uri { get; set; } = "amqp://guest:guest@localhost:5672/";

    public string Exchange { get; set; } = "fix.files";

    /// <summary>Linhas executadas ao mesmo tempo. 1 = em ordem (evita disputa na numeração de mandatos e boletas no core).</summary>
    public ushort LinePrefetch { get; set; } = 1;
}

/// <summary>core-service (seção "Core").</summary>
public sealed class CoreOptions
{
    public string Address { get; set; } = "http://localhost:5098";

    public int TimeoutSeconds { get; set; } = 30;
}
