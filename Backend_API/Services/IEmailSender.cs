using Backend_API.DTOs;
namespace Backend_API.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
