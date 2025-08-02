using Amazon.S3;
using Amazon.S3.Model;
using ManagementApp.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace ManagementApp.Services.Configuration;
public class CloudflareR2Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly CloudflareR2Settings _settings;
    private readonly ILogger<CloudflareR2Service> _logger;
    private readonly IWebHostEnvironment _env;
    public CloudflareR2Service(IAmazonS3 s3Client, CloudflareR2Settings settings, ILogger<CloudflareR2Service> logger, IWebHostEnvironment env)
    {
        _s3Client = s3Client;
        _settings = settings; // No need to use .Value, since settings is directly injected
        _logger = logger;
        _env = env;
    }

    public async Task<string> UploadFileAsync(IBrowserFile file)
    {
        try
        {
            _logger.LogInformation($"Started Uploading file {file.Name}");

            var name = file.Name;
            // Generate a unique filename
            var fileName = $"{DateTime.UtcNow.ToString("dd-HHmmss")}—{name}" ?? $"{Guid.NewGuid()}{Path.GetExtension(name)}";
            string encodedUrl = Uri.EscapeDataString(fileName);
            // Open file stream
            await using var fileStream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB limit

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = file.ContentType,
                DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("File uploaded successfully.");

            string publicUrl;
            // Construct public URL
            if (_settings.Env != "development")
            {
                publicUrl = $"https://{_settings.publicURL}/{encodedUrl}";
            }
            else
            {
                publicUrl = $"https://pub-{_settings.publicURL}.r2.dev/{encodedUrl}";
            }
            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Logged Upload error: {ex.Message}");
            throw;
        }
    }
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            _logger.LogInformation($"Started Uploading file {fileName}");

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType,
                DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("File uploaded successfully.");
            string publicUrl;
            // Construct public URL
            if (_settings.Env != "development")
            {
                publicUrl = $"https://{_settings.publicURL}/{fileName}";
            }
            else
            {
                publicUrl = $"https://pub-{_settings.publicURL}.r2.dev/{fileName}";
            }
            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Logged Upload error: {ex.Message}");
            throw;
        }
    }
}
