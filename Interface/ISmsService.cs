namespace NotificationAPI;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
}
