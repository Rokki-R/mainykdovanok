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
        public async Task notifyLotteryPosterWin(string email, string deviceName, string winnerEmail)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Išrinktas laimėtojas Jūsų skelbimui";
            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūsų skelbimo loterijos <b>„{deviceName}“</b> laimėtojas yra <b>{winnerEmail}</b>.</p>" +
                           $"<p>Laimėtojui išsiųstas laiškas, kuriame yra pateikiama jūsų informacija, kad laimėtojas galėtų su jumis susisiekti</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok</p>" +
                           $"</body></html>";

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to poster: {0}", ex.Message);
            }
        }

        public async Task notifyLotteryWinner(string email, string ownerEmail, string ownerPhoneNumber, string deviceName, int deviceId)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Laimėjote mainykdovanok sistemos skelbimo loteriją!";

            string url = $"https://localhost:44492/laimejimas/{deviceId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūs tapote laimėtoju skelbimo „<b>{deviceName}</b>“ loterijoje!</p>" +
                           $"<p>Žemiau matysite skelbimo savininko duomenis, kad galėtumėte susisiekti su skelbimo savininku dėl laimėto prietaiso atsiėmimo:</p>" +
                           $"<p>-{ownerEmail}</p>" +
                           $"<p>-{ownerPhoneNumber}</p>" +
                           $"<p>Atsiėmus elektronikos prietaisą, laimėto elektronikos prietaiso atsiėmimą prašome patvirtinti <a href=\"{url}\">čia</a>." +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok</p>" +
                           $"</body></html>";
            message.IsBodyHtml = true;

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to lottery winner: {0}", ex.Message);
            }
        }
        public async Task notifyUserDeviceAboutNoParticipants(string email, string deviceName, bool isLottery = false)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Jūsų loterijos tipo skelbimas";
            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Neatsiradus nei vienam loterijos dalyviui, jūsų skelbimas {deviceName} buvo ištrintas</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok</p>" +
                           $"</body></html>";

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to poster notifying of device deletion because of no participants: {0}", ex.Message);
            }
        }

        public async Task notifyLetterWinner(string email, string ownerEmail, string ownerPhoneNumber, string deviceName, int deviceId)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Laimėjote mainykdovanok skelbimą!";

            string url = $"https://localhost:44492/laimejimas/{deviceId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūs tapote „<b>{deviceName}</b>“ skelbimo laimėtoju!</p>" +
                           $"<p>Iš visų motyvacinių laiškų, savininkui labiausiai patiko Jūsų!</p>" +
                           $"<p>Žemiau matysite skelbimo savininko duomenis, kad galėtumėte susisiekti su skelbimo savininku dėl laimėto prietaiso atsiėmimo:</p>" +
                           $"<p>-{ownerEmail}</p>" +
                           $"<p>-{ownerPhoneNumber}</p>" +
                           $"<p>Atsiėmus elektronikos prietaisą, laimėto elektronikos prietaiso atsiėmimą prašome patvirtinti <a href=\"{url}\">čia</a>." +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok</p>" +
                           $"</body></html>";
            message.IsBodyHtml = true;

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to device winner: {0}", ex.Message);
            }
        }
        public async Task notifyOfferWinner(string email, string ownerEmail, string ownerPhoneNumber, string deviceName, int deviceId, string offerDeviceName)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Sėkmingi mainai skelbimais mainykdovanok sistemoje!";

            string url = $"https://localhost:44492/laimejimas/{deviceId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Su jumis sutiko mainyti „<b>{deviceName}</b>“ prietaisą!</p>" +
                           $"<p>Iš visų pasiūlymų, savininkui labiausiai patiko jūsų „<b>{offerDeviceName}</b>“!" +
                           $"<p>Žemiau matysite skelbimo savininko duomenis, kad galėtumėte susisiekti su skelbimo savininku dėl laimėto prietaiso atsiėmimo:</p>" +
                           $"<p>-{ownerEmail}</p>" +
                           $"<p>-{ownerPhoneNumber}</p>" +
                           $"<p>Atsiėmus elektronikos prietaisą, laimėto elektronikos prietaiso atsiėmimą prašome patvirtinti <a href=\"{url}\">čia</a>." +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok</p>" +
                           $"</body></html>";
            message.IsBodyHtml = true;

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to motivational letter winner: {0}", ex.Message);
            }
        }
        public async Task<bool> changePassword(DataTable result, string resetURL)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(result.Rows[0]["email"].ToString()));

            message.Subject = "Pasikeiskite slaptažodį";
            message.Body = $"<html><body><p>Sveiki,</p>" +
                $"<p>Norėdami pasikeisti slaptažodį, spauskite <a href=\"{resetURL}\">čia</a>.</p>" +
                $"<p>Linkėjimai,</p>" +
                $"<p>mainykdovanok</p>" +
                $"</body></html>";

            try
            {
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email for password change: {0}", ex.Message);
                return false;
            }
        }

        public async Task notifyQuestionnaireWinner(string email, string ownerEmail, string ownerPhoneNumber, string deviceName, int deviceId)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Laimėjote mainykdovanok skelbimo klausimyną!";

            string url = $"https://localhost:44492/laimejimas/{deviceId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūs tapote laimėtoju skelbimo „<b>{deviceName}</b>“ klausimyne!</p>" +
                           $"<p>Iš visų atsakymų, savininkui labiausiai patiko Jūsų!</p>" +
                           $"<p>Žemiau matysite skelbimo savininko duomenis, kad galėtumėte susisiekti su skelbimo savininku dėl laimėto prietaiso atsiėmimo:</p>" +
                           $"<p>-{ownerEmail}</p>" +
                           $"<p>-{ownerPhoneNumber}</p>" +
                           $"<p>Atsiėmus elektronikos prietaisą, laimėto elektronikos prietaiso atsiėmimą prašome patvirtinti <a href=\"{url}\">čia</a>." +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok</p>" +
                           $"</body></html>";
            message.IsBodyHtml = true;

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to questionnaire winner: {0}", ex.Message);
            }
        }
    }
}
