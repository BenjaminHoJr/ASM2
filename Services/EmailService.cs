using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class EmailService
    {
        private readonly EmailSetting _settings;
        public EmailService(IOptions<EmailSetting> opts) => _settings = opts.Value;

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            if (!string.IsNullOrWhiteSpace(_settings.Password))
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}