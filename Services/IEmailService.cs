using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Services
{
    public interface IEmailService
    {
        Task SendContactNotificationEmail(ContactMessage message, string toEmail);
    }


}
