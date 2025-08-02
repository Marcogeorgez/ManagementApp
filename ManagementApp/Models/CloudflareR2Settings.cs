namespace ManagementApp.Models;

public record CloudflareR2Settings(string AccountId, string AccessKeyId, string SecretAccessKey, string BucketName, string publicURL, string Env = "production");
