using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using SmoothJourneyAPI.Interfaces;

namespace SmoothJourneyAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        public EmailService(IConfiguration cfg) 
        { 
            _cfg = cfg; 
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            var from = _cfg["Smtp:From"] ?? _cfg["Smtp:User"];
            msg.From.Add(MailboxAddress.Parse(from));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_cfg["Smtp:Host"], int.Parse(_cfg["Smtp:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_cfg["Smtp:User"], _cfg["Smtp:Pass"]);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}
