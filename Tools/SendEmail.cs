using System.Net.Mail;
using System.Net;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Web;

namespace mainykdovanok.Tools
{
    public class SendEmail
    {
        private Serilog.ILogger _logger;
        private string fromMail = "mainykdovanok@gmail.com";
        private string fromPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
        private SmtpClient smtpClient;
        private MailMessage message;

        public SendEmail()
        {
            CreateLogger();

            smtpClient = new SmtpClient();
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(fromMail, fromPassword);
            smtpClient.Host = "smtp.gmail.com";

            message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.IsBodyHtml = true;
        }

        private void CreateLogger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public async Task<bool> verifyEmail(string email, string verifyURL)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Patvirtinkite savo el. pašto adresą";
            message.Body = $"<html><body><p>Sveiki,</p>" +
                $"<p>Norint naudotis mainykdovanok.lt svetainę, turite patvirtinti savo el. paštą. Tai galite padaryti paspaudę šią nuorodą: {verifyURL}</p>" +
                $"<p>Linkėjimai,</p>" +
                $"<p>mainykdovanok.lt</p>" +
                $"</body></html>";

            try
            {
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email for verification: {0}", ex.Message);
                return false;
            }
        }
    }
}
