using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
namespace LuminaryVisuals.Services;

public class EmailConfiguration
{
    public string SmtpServer { get; set; } = default!;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = default!;
    public string SmtpPassword { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
}
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}


public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;

    public EmailService(IOptions<EmailConfiguration> emailConfig)
    {
        _emailConfig = emailConfig.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Luminary Visuals", _emailConfig.FromEmail));
            message.To.Add(new MailboxAddress($"{to}", to));
            message.Subject = subject;

            var bodyPart = new TextPart(isHtml ? "html" : "plain")
            {
                Text = body
            };
            message.Body = bodyPart;

            using var client = new SmtpClient();

            // Add logging or console output here to debug
            Console.WriteLine($"Attempting to connect to {_emailConfig.SmtpServer}:{_emailConfig.SmtpPort}");

            await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Failed to send email: {ex.Message}");
            throw; // Re-throw to maintain the error state
        }
    }
}