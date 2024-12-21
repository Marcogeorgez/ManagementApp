using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace LuminaryVisuals.Services;

public class CloudflareR2Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly CloudflareR2Settings _settings;

    public CloudflareR2Service(IAmazonS3 s3Client, IOptions<CloudflareR2Settings> settings)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
    }

    public async Task<string> UploadFileAsync(IBrowserFile file)
    {
        try
        {
            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";

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

            // Construct public URL
            string publicUrl = $"https://{_settings.publicURL}.r2.dev/{fileName}";
            return publicUrl;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Upload error: {ex.Message}");
            throw;
        }
    }
}
public class CloudflareR2Settings
{
    public string AccountId { get; set; }
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string BucketName { get; set; }
    public string publicURL { get; set; }

}