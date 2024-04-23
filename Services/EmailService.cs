using System.Net.Mail;

namespace NotificationAPI;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress("dubemmegbo@gmmail.com"), 
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        try
        {
            await _smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Handle exceptions or log errors
            throw new Exception("Failed to send email", ex);
        }
        finally
        {
            message.Dispose();
        }
    }
}
