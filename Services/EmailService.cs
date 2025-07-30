using System.Net.Mail;
using System.Net;
using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendContactNotificationEmail(ContactMessage message, string toEmail)
        {
            var smtpClient = new SmtpClient(_config["Email:SmtpHost"])
            {
                Port = int.Parse(_config["Email:Port"]),
                Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]),
                Subject = $"New Contact Message from {message.Name}",
                Body = $"Name: {message.Name}\nEmail: {message.Email}\nNumber: {message.Number}\nMessage: {message.Message}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }


}
