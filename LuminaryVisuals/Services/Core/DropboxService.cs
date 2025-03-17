namespace LuminaryVisuals.Services.Core;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Sharing;
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
public class TokenRefreshException : Exception
{
    public TokenRefreshException(string message) : base(message) { }
    public TokenRefreshException(string message, Exception innerException) : base(message, innerException) { }
}
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
            try
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
                    Console.WriteLine($"Failed to refresh access token. Status: {response.StatusCode}, Response: {responseContent}");
                    throw new Exception($"Failed to refresh access token. Please contact manager.");
                }

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                if (tokenResponse?.AccessToken == null)
                {
                    throw new TokenRefreshException("Access token not found in the response.");
                }
                return tokenResponse?.AccessToken;
            }
            catch (HttpRequestException httpEx)
            {
                // Handle network-related errors
                throw new TokenRefreshException("Network error occurred while refreshing the access token.", httpEx);
            }
            catch (JsonException jsonEx)
            {
                // Handle JSON deserialization errors
                throw new TokenRefreshException("Failed to parse the token response.", jsonEx);
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                throw new TokenRefreshException("An unexpected error occurred while refreshing the access token.", ex);
            }
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
            // Check if a shared link already exists
            var sharedLinks = _client.Sharing.ListSharedLinksAsync(path, directOnly: true).Result;
            if (sharedLinks.Links.Count > 0)
            {
                foreach (var link in sharedLinks.Links)
                {
                    // Check if the existing link allows downloads
                    if (link.LinkPermissions?.AllowDownload != true)
                    {
                        // If it doesn't allow downloads, revoke the link
                        _client.Sharing.RevokeSharedLinkAsync(link.Url).Wait();
                    }
                    else
                    {
                        // If it's already downloadable, return the existing link
                        return link.Url;
                    }
                }
            }

            // Create a new shared link with the desired settings
            var settings = new SharedLinkSettings(
                requestedVisibility: RequestedVisibility.Public.Instance);

            var shareResult = _client.Sharing.CreateSharedLinkWithSettingsAsync(path, settings).Result;
            return shareResult.Url;
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsPath)
        {
            // Log the "path not found" error beautifully
            throw new FileNotFoundException($"The path '{path}' was not found. Please recheck if the folder is uploaded or " +
                $"if it's in the correct path.", aex);
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsSettingsError)
        {
            throw new Exception("You don't have permission to do this, please check if Dropbox subscription has run-out and contact manager.");
        }
        catch (Exception ex)
        {
            // Log any other exceptions
            throw new Exception("Failed to create a sharable link. Please contact manager.", ex);
        }
    }

    // Create a view-only sharable link for a file or folder
    public string CreateViewOnlySharableLinkAsync(string path)
    {
        EnsureValidAccessToken();

        try
        {
            // Check if a shared link already exists
            var sharedLinks = _client.Sharing.ListSharedLinksAsync(path, directOnly: true).Result;
            if (sharedLinks.Links.Count > 0)
            {
                foreach (var link in sharedLinks.Links)
                {
                    // Check if the existing link allows downloads
                    if (link.LinkPermissions?.AllowDownload == true)
                    {
                        // If it allows downloads, revoke the link
                        _client.Sharing.RevokeSharedLinkAsync(link.Url).Wait();
                    }
                    else
                    {
                        // If it's already non-downloadable, return the existing link
                        return link.Url;
                    }
                }
            }

            // Create a new shared link with the desired settings
            var settings = new SharedLinkSettings(
                allowDownload: false, // Ensure the new link is non-downloadable
                expires: null,
                linkPassword: null,
                requestedVisibility: RequestedVisibility.Public.Instance);

            var shareResult = _client.Sharing.CreateSharedLinkWithSettingsAsync(path, settings).Result;
            return shareResult.Url;
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsPath)
        {
            // Log the "path not found" error beautifully
            throw new FileNotFoundException($"The path '{path}' was not found. Please recheck if the folder is uploaded or " +
                $"if it's in the correct path.", aex);
        }
        catch (AggregateException ex) when (ex.InnerException is ApiException<CreateSharedLinkWithSettingsError> aex && aex.ErrorResponse.IsSettingsError)
        {
            throw new Exception("You don't have permission to do this, please check if Dropbox subscription has run-out and contact manager.");
        }
        catch (Exception ex)
        {
            // Log any other exceptions
            throw new Exception("Failed to create a view-only sharable link. Please contact manager.", ex);
        }
    }
    // Move the contents of a folder to another folder
    public async Task MoveFolderContentsAsync(string fromPath, string toPath,string projectName)
    {
        EnsureValidAccessToken();

        try
        {
            // List all files and folders in the source folder
            var listResult = await _client.Files.ListFolderAsync(fromPath);

            foreach (var item in listResult.Entries)
            {
                if(item.Name == projectName)
                {
                    var destinationPath = $"{toPath}/{item.Name}";
                    await _client.Files.MoveV2Async(item.PathLower, destinationPath);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to move folder contents.", ex);
        }
    }
    public async Task<string> GetOrCreateSharedLink(string path)
    {
        try
        {
            // Try to get an existing shared link
            var existingLinks = await _client.Sharing.ListSharedLinksAsync(path);
            if (existingLinks.Links.Count > 0)
            {
                return existingLinks.Links[0].Url;
            }

            // If no link exists, create a new one
            var sharedLink = await _client.Sharing.CreateSharedLinkWithSettingsAsync(path);
            return sharedLink.Url;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}


