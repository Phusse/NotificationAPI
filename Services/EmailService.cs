using PostmarkDotNet;

public class PostmarkEmailService
{
    private readonly PostmarkClient _client;

    public PostmarkEmailService(string apiKey)
    {
        _client = new PostmarkClient(apiKey);
    }

    public async Task<bool> SendEmailAsync(string from, string to, string subject, string body)
    {
        try
        {
            var message = new PostmarkMessage
            {
                From = from,
                To = to,
                Subject = subject,
                HtmlBody = body
            };

            var response = await _client.SendMessageAsync(message);
            return response.Status == PostmarkStatus.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
            return false;
        }
    }
}
