namespace FileService.Infrastructure.S3.Options;

public class S3Options
{
    public const string SECTION_NAME = "S3";

    public string Endpoint { get; set; } = string.Empty;

    public string PublicEndpoint { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    public string BucketName { get; set; } = "files";

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public bool ForcePathStyle { get; set; }

    public bool CreateBucketIfMissing { get; set; }

    public TimeSpan PresignedUrlTtl { get; set; } = TimeSpan.FromMinutes(15);

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan HealthCheckTimeout { get; set; } = TimeSpan.FromSeconds(2);
}