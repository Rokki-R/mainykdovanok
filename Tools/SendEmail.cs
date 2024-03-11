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
        public async Task notifyLotteryPosterWin(string email, string itemName, string winnerEmail)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Išrinktas laimėtojas Jūsų skelbimui";
            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūsų skelbimo loterijos <b>„{itemName}“</b> laimėtojas yra <b>{winnerEmail}</b>.</p>" +
                           $"<p>Laimėtojui išsiųstas laiškas su nuoroda į formą, kurioje paprašoma pateikti telefono numerį, kad galėtumėte susisiekti su jais.</p>" +
                           $"<p>Ačiū, kad padedate tausoti aplinką!</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok.lt</p>" +
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

        public async Task notifyLotteryWinner(string email, string itemName, int itemId)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Laimėjote mainykdovanok.lt skelbimo loteriją!";

            string url = $"https://localhost:44492/laimejimas/{itemId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūs tapote laimėtoju skelbimo „<b>{itemName}</b>“ loterijoje!</p>" +
                           $"<p>Kad suderinti pristatymą ar atsiėmimą, prašome eiti į šią nuorodą:</p>" +
                           $"<p><a href='{url}'>{url}</a></p>" +
                           $"<p>Šiame puslapyje turėsite galimybę pateikti savo telefono numerį, kad skelbėjas galėtų su Jumis susisiekti.</p>" +
                           $"<p>Ačiū, kad padedate tausoti aplinką!</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok.lt</p>" +
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
        public async Task notifyUserItemExpiration(string email, string itemName, bool isLottery = false)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Jūsų skelbimo galiojimo laikas pasibaigė";
            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Pasibaigė Jūsų skelbimo <b>„{itemName}“</b> galiojimo laikas.</p>";

            if (isLottery)
                message.Body += $"<p>Dėl nepakankamo dalyvių skaičiaus skelbimas buvo atšauktas.</p>";

            message.Body += $"<p>Linkėjimai,</p>" +
                            $"<p>mainykdovanok.lt</p>" +
                            $"</body></html>";

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to poster notifying of expiration: {0}", ex.Message);
            }
        }

        public async Task notifyLetterWinner(string email, string itemName, int itemId)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Laimėjote mainykdovanok.lt skelbimą!";

            string url = $"https://localhost:44492/laimejimas/{itemId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūs tapote „<b>{itemName}</b>“ skelbimo laimėtoju!</p>" +
                           $"<p>Iš visų motyvacinių laiškų, savininkui labiausiai patiko Jūsų!</p>" +
                           $"<p>Kad suderinti pristatymą ar atsiėmimą, prašome eiti į šią nuorodą:</p>" +
                           $"<p>{url}</p>" +
                           $"<p>Šiame puslapyje turėsite galimybę pateikti savo telefono numerį, kad skelbėjas galėtų su Jumis susisiekti.</p>" +
                           $"<p>Ačiū, kad padedate tausoti aplinką!</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok.lt</p>" +
                           $"</body></html>";
            message.IsBodyHtml = true;

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email to item winner: {0}", ex.Message);
            }
        }
        public async Task<bool> sendWinnerDetails(string email, string itemName, string phoneNumber, string additionalMessage)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Skelbimo laimėtojas atsiuntė susisiekimo duomenis";

            string extraMessageBody = additionalMessage.Trim().Length > 0 ? $"<p>Papildoma žinutė iš laimėtojo: <b>{additionalMessage}</b></p>" : "";
            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūsų skelbimo „<b>{itemName}</b>“ laimėtojas pateikė savo susisiekimo duomenis:</p>" +
                           $"<p>Telefono numeris: <b>{phoneNumber}</b></p>" +
                           $"<p>{extraMessageBody}</p>" +
                           $"<p>Ačiū, kad padedate tausoti aplinką!</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok.lt</p>" +
                           $"</body></html>";
            message.IsBodyHtml = true;

            try
            {
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending winner details email to poster: {0}", ex.Message);
                return false;
            }
        }
        public async Task notifyOfferWinner(string email, string itemName, int itemId, string offerItemName)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Sėkmingi mainai skelbimais mainykdovanok.lt svetainėje!";

            string url = $"https://localhost:44492/laimejimas/{itemId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Su jumis sutiko mainyti „<b>{itemName}</b>“ prietaisą!</p>" +
                           $"<p>Iš visų pasiūlymų, savininkui labiausiai patiko jūsų „<b>{offerItemName}</b>“!" +
                           $"<p>Kad suderinti pristatymą ar atsiėmimą, prašome eiti į šią nuorodą:</p>" +
                           $"<p>{url}</p>" +
                           $"<p>Šiame puslapyje turėsite galimybę pateikti savo telefono numerį, kad skelbėjas galėtų su Jumis susisiekti.</p>" +
                           $"<p>Ačiū, kad padedate tausoti aplinką!</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok.lt</p>" +
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
                $"<p>mainyk.lt</p>" +
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

        public async Task notifyQuestionnaireWinner(string email, string itemName, int itemId)
        {
            message.To.Clear();
            message.To.Add(new MailAddress(email));

            message.Subject = "Laimėjote mainykdovanok.lt skelbimo klausimyną!";

            string url = $"https://localhost:44492/laimejimas/{itemId}";

            message.Body = $"<html><body>" +
                           $"<p>Sveiki,</p>" +
                           $"<p>Jūs tapote laimėtoju skelbimo „<b>{itemName}</b>“ klausimyne!</p>" +
                           $"<p>Iš visų atsakymų, savininkui labiausiai patiko Jūsų!</p>" +
                           $"<p>Kad suderinti pristatymą ar atsiėmimą, prašome eiti į šią nuorodą:</p>" +
                           $"<p><a href='{url}'>{url}</a></p>" +
                           $"<p>Šiame puslapyje turėsite galimybę pateikti savo telefono numerį, kad skelbėjas galėtų su Jumis susisiekti.</p>" +
                           $"<p>Ačiū, kad padedate tausoti aplinką!</p>" +
                           $"<p>Linkėjimai,</p>" +
                           $"<p>mainykdovanok.lt</p>" +
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
