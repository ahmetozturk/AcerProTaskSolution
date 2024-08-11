using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net.Mail;
using System.Net;

namespace AcerProTask.Services
{
    public static class EmailService
    {

        public static async Task SendEmailAsync(string toAddress, string subject, string body)
        {
            var fromAddress = "your-email@gmail.com";
            var fromPassword = "your-app-password"; 

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, 
                Credentials = new NetworkCredential(fromAddress, fromPassword),
                EnableSsl = true, 
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress, "Your Name"), 
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toAddress);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
