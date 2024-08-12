using System.Net;
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
            _smtpHost = configuration["Smtp:Host"] ?? throw new ArgumentNullException(nameof(_smtpHost), "SMTP Host configuration is missing");
            _smtpPort = int.Parse(configuration["Smtp:Port"] ?? "25"); // Default to port 25
            _fromAddress = configuration["Smtp:FromAddress"] ?? "admin@emailserver.com";
        }

        public void Notify(string toAddress, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(new MailAddress(toAddress));

            using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                smtpClient.Send(mailMessage);
            }
        }
    }
}
