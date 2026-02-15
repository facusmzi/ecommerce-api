using MailKit.Net.Smtp;
using MimeKit;


namespace ECommerce.API.Common.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(configuration["Smtp:Username"]));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    configuration["Smtp:Host"],
                    int.Parse(configuration["Smtp:Port"] ?? "587"),
                    false
                );
                await client.AuthenticateAsync(
                    configuration["Smtp:Username"],
                    configuration["Smtp:Password"]
                );
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // En desarrollo logueamos el error sin romper el flujo,
                // igual que hicimos en el proyecto Auth.
                logger.LogError(ex, "Error enviando email a {To}", to);
            }
        }
    }
}
