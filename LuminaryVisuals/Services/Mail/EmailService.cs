using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
namespace LuminaryVisuals.Services;

public class EmailConfiguration
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public string FromEmail { get; set; }
}
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}


public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;

    public EmailService(EmailConfiguration emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
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
        await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}