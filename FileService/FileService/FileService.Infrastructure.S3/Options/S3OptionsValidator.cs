using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3.Options;

public class S3OptionsValidator : IValidateOptions<S3Options>
{
    public ValidateOptionsResult Validate(string? name, S3Options options)
    {
        var errors = new List<string>();

        if (!IsEndpoint(options.Endpoint))
        {
            errors.Add("S3:Endpoint must be an absolute HTTP(S) URL without credentials, query or fragment.");
        }

        if (!IsEndpoint(options.PublicEndpoint))
        {
            errors.Add("S3:PublicEndpoint must be an absolute HTTP(S) URL without credentials, query or fragment.");
        }

        if (string.IsNullOrWhiteSpace(options.Region) ||
            !Regex.IsMatch(options.Region, "^[a-z0-9-]+$"))
        {
            errors.Add("S3:Region is required and must contain lowercase letters, digits or hyphens.");
        }

        if (!IsBucketName(options.BucketName))
        {
            errors.Add("S3:BucketName must be a valid S3 bucket name.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey) || options.AccessKey.Any(char.IsControl))
        {
            errors.Add("S3:AccessKey is required and must not contain control characters.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey) || options.SecretKey.Any(char.IsControl))
        {
            errors.Add("S3:SecretKey is required and must not contain control characters.");
        }

        if (options.PresignedUrlTtl < TimeSpan.FromSeconds(1) || options.PresignedUrlTtl > TimeSpan.FromDays(7))
        {
            errors.Add("S3:PresignedUrlTtl must be between one second and seven days.");
        }

        if (options.RequestTimeout <= TimeSpan.Zero || options.RequestTimeout > TimeSpan.FromMinutes(5))
        {
            errors.Add("S3:RequestTimeout must be positive and no greater than five minutes.");
        }

        if (options.HealthCheckTimeout <= TimeSpan.Zero || options.HealthCheckTimeout > options.RequestTimeout)
        {
            errors.Add("S3:HealthCheckTimeout must be positive and no greater than RequestTimeout.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private static bool IsEndpoint(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
        string.IsNullOrEmpty(uri.UserInfo) &&
        string.IsNullOrEmpty(uri.Query) &&
        string.IsNullOrEmpty(uri.Fragment);

    private static bool IsBucketName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        Regex.IsMatch(value, "^[a-z0-9][a-z0-9.-]{1,61}[a-z0-9]$") &&
        !value.Contains("..", StringComparison.Ordinal) &&
        !value.Contains(".-", StringComparison.Ordinal) &&
        !value.Contains("-.", StringComparison.Ordinal) &&
        !IPAddress.TryParse(value, out _);
}