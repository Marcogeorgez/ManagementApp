using ManagementApp.Data.Entities;
using ManagementApp.Services.Configuration;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

public class InvoiceService
{
    private readonly NavigationManager navigationManager;
    private readonly ILogger<InvoiceService> logger;
    private readonly CloudflareR2Service cloudflareR2Service;
    private readonly string key = Environment.GetEnvironmentVariable("Generate-Invoice-API-Key")!;
    public InvoiceService(NavigationManager navigationManager, ILogger<InvoiceService> logger, CloudflareR2Service cloudflareR2Service)
    {
        this.navigationManager = navigationManager;
        this.logger = logger;
        this.cloudflareR2Service = cloudflareR2Service;

    }
    public async Task<string> GenerateInvoicePdfAsync(List<Project> Items, PayoneerSettings? setting)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                string BillAddressTo = string.Join("\n", new[]
                {
                    setting.CompanyName,
                    setting.Address,
                    !string.IsNullOrWhiteSpace(setting.TaxId) ? $"Tax ID: {setting.TaxId}" : null,
                    setting.Email
                }.Where(s => !string.IsNullOrWhiteSpace(s)));

                var formFields = new Dictionary<string, string>
                {
                    { "from", "Marco George \n Cairo, Egypt" },
                    { "to", BillAddressTo },
                    { "logo", $"{navigationManager.BaseUri}Logo192-Dark.png" },
                    { "date", DateTime.Now.ToString("MMM dd, yyyy") },
                    { "terms", "All transaction fees must be paid by the client. Company should receive the full amount in the description." },
                };
                // Line item
                decimal total = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].ClientBillableAmount == null)
                    {
                        continue;
                    }
                    var clientBillableAmount = Math.Round(Items[i].ClientBillableAmount ?? 0, MidpointRounding.AwayFromZero);
                    total += clientBillableAmount;
                    formFields.Add($"items[{i}][name]", Items[i].ProjectName);
                    formFields.Add($"items[{i}][quantity]", "1");
                    formFields.Add($"items[{i}][unit_cost]", clientBillableAmount.ToString());
                }

                var content = new FormUrlEncodedContent(formFields);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://invoice-generator.com")
                {
                    Content = content
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

                // Accept PDF as response
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode}");
                }
                await using var pdfStream = await response.Content.ReadAsStreamAsync();
                string fileName = $"invoice-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

                string url = await cloudflareR2Service.UploadFileAsync(pdfStream, fileName, "application/pdf");
                return url;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error generating invoice: {ex.Message}");
                return null;
            }
        }
    }
}
