using BookfetSystem.Services.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
        {
            var smtp = _configuration.GetSection("Smtp");
            var host = smtp["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtp["Port"] ?? "587");
            var fromEmail = smtp["FromEmail"]?.Trim();
            var fromName = smtp["FromName"] ?? "Bookfet System";
            // App Password: bỏ khoảng trắng (Gmail hiển thị dạng "xxxx xxxx xxxx xxxx")
            var appPassword = (smtp["AppPassword"] ?? "").Replace(" ", "").Trim();

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
            {
                throw new InvalidOperationException("Smtp:FromEmail và Smtp:AppPassword phải được cấu hình trong appsettings.");
            }

            // Gmail yêu cầu TLS 1.2 trở lên
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject
            };
            message.To.Add(toEmail);

            if (!string.IsNullOrEmpty(plainTextBody) && !string.IsNullOrEmpty(htmlBody))
            {
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain"));
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));
            }
            else if (!string.IsNullOrEmpty(htmlBody))
            {
                message.Body = htmlBody;
                message.IsBodyHtml = true;
            }
            else
            {
                message.Body = plainTextBody ?? string.Empty;
                message.IsBodyHtml = false;
            }

            await client.SendMailAsync(message);
        }
    }
}
