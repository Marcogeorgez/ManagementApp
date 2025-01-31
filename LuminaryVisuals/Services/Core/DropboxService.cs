namespace LuminaryVisuals.Services.Core;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Sharing;

public class DropboxService
{
    private DropboxClient _client;
    private readonly string _refreshToken;
    private readonly string _appKey;
    private readonly string _appSecret;

    // Initialize the service with a refresh token, app key, and app secret
    public DropboxService(string refreshToken, string appKey, string appSecret)
    {
        _refreshToken = refreshToken;
        _appKey = appKey;
        _appSecret = appSecret;
        _client = new DropboxClient(GetAccessToken());
    }

    // Method to refresh the access token (synchronous)
    private string RefreshAccessToken()
    {
        using (var httpClient = new HttpClient())
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", _refreshToken),
                new KeyValuePair<string, string>("client_id", _appKey),
                new KeyValuePair<string, string>("client_secret", _appSecret),
            });

            var response = httpClient.PostAsync("https://api.dropbox.com/oauth2/token", requestContent).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to refresh access token: {responseContent}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            return tokenResponse?.AccessToken;
        }
    }

    // Method to get the access token (refreshes if necessary)
    private string GetAccessToken()
    {
        try
        {
            // Attempt to refresh the access token
            return RefreshAccessToken();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get access token.", ex);
        }
    }

    // Method to ensure the client has a valid access token
    private void EnsureValidAccessToken()
    {
        try
        {
            // Try a simple operation to check if the token is valid
            _client.Users.GetCurrentAccountAsync().Wait();
        }
        catch (AggregateException ex) when (ex.InnerException is DropboxException dex && dex.Message.Contains("expired_access_token"))
        {
            // If the token is expired, refresh it and update the client
            var newAccessToken = GetAccessToken();
            _client = new DropboxClient(newAccessToken);
        }
    }

    // Create a sharable download link for a file or folder
    public string CreateSharableDownloadLinkAsync(string path)
    {
        EnsureValidAccessToken();

        try
        {
            var sharedLink = _client.Sharing.CreateSharedLinkWithSettingsAsync(path).Result;
            return sharedLink.Url;
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsSharedLinkAlreadyExists)
        {
            // If a shared link already exists, return the existing link
            var sharedLinks = _client.Sharing.ListSharedLinksAsync(path).Result;
            return sharedLinks.Links[0].Url;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create a sharable download link.", ex);
        }
    }

    // Create a view-only sharable link for a file or folder
    public string CreateViewOnlySharableLinkAsync(string path)
    {
        EnsureValidAccessToken();

        try
        {
            // Uses the SharedLinkSettings constructor to set the visibility
            var settings = new SharedLinkSettings(
               expires: null,
               linkPassword: null,
               requestedVisibility: RequestedVisibility.Public.Instance);

            var shareResult = _client.Sharing.CreateSharedLinkWithSettingsAsync(path, settings).Result;
            return shareResult.Url;
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsSharedLinkAlreadyExists)
        {
            // If a shared link already exists, return the existing link
            var sharedLinks = _client.Sharing.ListSharedLinksAsync(path).Result;
            return sharedLinks.Links[0].Url;
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsPath)
        {
            // Log the "path not found" error beautifully
            throw new FileNotFoundException($"The path '{path}' was not found.", aex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions
            throw new Exception("Failed to create a view-only sharable link.", ex);
        }
    }

    // Move the contents of a folder to another folder
    public void MoveFolderContentsAsync(string fromPath, string toPath)
    {
        EnsureValidAccessToken();

        try
        {
            // List all files and folders in the source folder
            var listResult = _client.Files.ListFolderAsync(fromPath).Result;

            foreach (var item in listResult.Entries)
            {
                var destinationPath = $"{toPath}/{item.Name}";
                _client.Files.MoveV2Async(item.PathLower, destinationPath).Wait();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to move folder contents.", ex);
        }
    }
}
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

