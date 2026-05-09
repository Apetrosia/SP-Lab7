using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GreenSwampApp.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendSubscriptionConfirmationAsync(string toEmail)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Greenswamp", _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Welcome to Greenswamp Pond!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = @"
                <h1>Thanks for subscribing!</h1>
                <p>You've successfully hopped into the Greenswamp community. Stay tuned for ribbiting updates!</p>
                <p>🐸 Cheers,<br/>The Greenswamp Team</p>",
                TextBody = "Thanks for subscribing to Greenswamp! You'll receive updates about campus life."
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Subscription email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}

public class SmtpSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FromEmail { get; set; }
}