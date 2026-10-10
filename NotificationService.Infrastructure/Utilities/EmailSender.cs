using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Utilities
{
    public class EmailSender : INotificationService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _fromAddress;

        public EmailSender(IConfiguration configuration)
        {
            _smtpHost = configuration["Smtp:Host"] ?? throw new InvalidOperationException("Configuration value 'Smtp:Host' not found.");
            _smtpPort = int.Parse(configuration["Smtp:Port"] ?? "25"); // Default to port 25
            _fromAddress = configuration["Smtp:FromAddress"] ?? "admin@emailserver.com";
        }

        public async Task NotifyAsync(string toAddress, string subject, string body, CancellationToken cancellationToken)
        {
            // Plain text: the body contains values supplied by users.
            using var mailMessage = new MailMessage(_fromAddress, toAddress, subject, body);
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}
