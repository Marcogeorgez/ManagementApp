namespace LuminaryVisuals.Services.Helpers;

using System.Text.RegularExpressions;
using LuminaryVisuals.Services.Configuration;

public static class Base64ImageProcessor
{
    private static readonly Regex Base64ImageRegex = new Regex(
        @"data:image/(?<type>[^;]+);base64,(?<data>[A-Za-z0-9+/=]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static async Task<string> ReplaceBase64ImagesWithLinks(string? input, CloudflareR2Service r2Service)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var matches = Base64ImageRegex.Matches(input);
        if (matches.Count == 0)
            return input;

        var updatedContent = input;

        foreach (Match match in matches)
        {
            try
            {
                var fileType = match.Groups["type"].Value;
                var base64Data = match.Groups["data"].Value;

                var imageBytes = Convert.FromBase64String(base64Data);
                using var stream = new MemoryStream(imageBytes);

                var fileExtension = GetFileExtension(fileType);
                var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid().ToString("N")}.{fileExtension}";
                var contentType = $"image/{fileType}";

                var uploadedUrl = await r2Service.UploadFileAsync(stream, fileName, contentType);

                updatedContent = updatedContent.Replace(match.Value, uploadedUrl);
                Console.WriteLine("Image replaced and returened string");
            }
            catch (Exception)
            {
                Console.WriteLine("Error processing base64 image. It might be corrupted or invalid.");
            }
        }
        return updatedContent;
    }

    private static string GetFileExtension(string type)
    {
        return type switch
        {
            "jpeg" => "jpg",
            "jpg" => "jpg",
            "png" => "png",
            "gif" => "gif",
            "bmp" => "bmp",
            "webp" => "webp",
            _ => "img"
        };
    }
}

