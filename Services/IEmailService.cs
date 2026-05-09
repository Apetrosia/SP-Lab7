namespace GreenSwampApp.Services;

public interface IEmailService
{
    Task SendSubscriptionConfirmationAsync(string toEmail);
}