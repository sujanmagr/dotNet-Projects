using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FoodY.Data
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpServer = _configuration.GetValue<string>("EmailSettings:SmtpServer");
                var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
                var smtpUsername = _configuration.GetValue<string>("EmailSettings:SmtpUsername");
                var smtpPassword = _configuration.GetValue<string>("EmailSettings:SmtpPassword");

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true; // Required for secure connection
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    var message = new MailMessage
                    {
                        From = new MailAddress(smtpUsername),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true // Ensure email supports HTML
                    };
                    message.To.Add(new MailAddress(email));

                    await client.SendMailAsync(message); // Send the email
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP error: {smtpEx.Message}");
                throw; // Rethrow for logging or further handling
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                throw; // Rethrow for debugging
            }
        }


    }
}