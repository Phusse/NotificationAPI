namespace NotificationAPI;

// SmsService.cs

public class SmsService : ISmsService
{
    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // This is a mock implementation
        // In a real-world scenario, integrate with an SMS gateway provider
        Console.WriteLine($"Sending SMS to {phoneNumber}: {message}");
        await Task.CompletedTask;
    }
}
