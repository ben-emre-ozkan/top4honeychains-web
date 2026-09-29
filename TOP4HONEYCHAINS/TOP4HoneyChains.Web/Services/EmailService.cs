using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TOP4HoneyChains.Web.Models.Email;

namespace TOP4HoneyChains.Web.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.UserName,
                    _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            await client.SendMailAsync(mail);
        }
    }
}
