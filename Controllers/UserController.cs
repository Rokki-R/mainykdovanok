using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mainykdovanok.Tools;
using mainykdovanok.Repositories.User;
using mainykdovanok.ViewModels.UserAuthentication;
using System.Security.Claims;
using mainykdovanok.ViewModels.User;
using System.Security.Cryptography;
using System.Web;
using MySqlX.XDevAPI.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace mainykdovanok.Controllers.UserAuthentication
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserRepo _userRepo;

        public UserController(ILogger<LoginController> logger)
        {
            _logger = logger;
            _userRepo = new UserRepo();
        }

        [HttpGet("getCurrentUserId")]
        public IActionResult GetCurrentUserId()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
                return Ok(userId);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("getMyProfileDetails")]
        public async Task<IActionResult> GetMyProfileDetails()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            string sql = "SELECT name, surname, email, devices_gifted, devices_won FROM user WHERE user_id = @user_id";
            var parameters = new { user_id = userId };
            var result = await _userRepo.LoadData(sql, parameters);

            if (result.Rows.Count == 0)
            {
                return BadRequest();
            }

            string name = result.Rows[0]["name"].ToString();
            string surname = result.Rows[0]["surname"].ToString();
            string email = result.Rows[0]["email"].ToString();
            int devicesGifted = Convert.ToInt32(result.Rows[0]["devices_gifted"]);
            int devicesWon = Convert.ToInt32(result.Rows[0]["devices_won"]);

            return Ok(new { name, surname, email, devicesGifted, devicesWon });
        }

        [HttpPost("updateProfileDetails")]
        public async Task<IActionResult> UpdateProfileDetails()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var form = await Request.ReadFormAsync();
            string name = form["name"].ToString();
            string surname = form["surname"].ToString();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
            {
                return BadRequest(new { message = "Negalima palikti tuščių laukų" });
            }

            int user_id = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            string sql = "UPDATE user SET name = @name, surname = @surname WHERE user_id = @user_id";
            var parameters_update = new { name, surname, user_id };
                await _userRepo.SaveData(sql, parameters_update);

            return Ok();
        }

        [HttpGet("getUserDetails/{userId}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRepo.GetUserById(userId);
                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getUserEmail/{userId?}")]
        public async Task<IActionResult> GetUserEmail(int? userId)
        {
            if (HttpContext.User.Identity.IsAuthenticated && userId == null)
            {
                return Ok(HttpContext.User.FindFirst("email").Value);
            }
            else if (userId != null)
            {
                string email = await _userRepo.GetUserEmail((int)userId);
                return Ok(email);
            }
            return BadRequest();
        }

        private static bool IsEmailValid(string email)
        {
            string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";

            return Regex.IsMatch(email, regex, RegexOptions.IgnoreCase);
        }

        private static bool IsPasswordValid(string password)
        {
            string regex = @"^(?=.*\d)(?=.*[!@#$%^&*+\-])(?=.*[a-z])(?=.*[A-Z]).{8,}$";
            return Regex.IsMatch(password, regex);
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(PasswordResetViewModel resetRequest)
        {
            string sql = "SELECT email FROM user WHERE email = @email";
            var parameters = new { email = resetRequest.Email };
            var result = await _userRepo.LoadData(sql, parameters);
            if (string.IsNullOrWhiteSpace(resetRequest.Email) || !IsEmailValid(resetRequest.Email))
            {
                return BadRequest(new { message = "Neteisingai įvestas el. paštas!" });
            }

            if (result.Rows.Count == 0)
            {
                return StatusCode(404);
            }
            else
            {
                byte[] tokenData = new byte[32]; // 256-bit token
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(tokenData);
                }

                string token = BitConverter.ToString(tokenData).Replace("-", ""); // Convert byte array to hex string
                DateTime changeTimer = DateTime.Now;
                changeTimer = changeTimer.AddHours(1);
                string time = changeTimer.ToString("yyyy-MM-dd HH:mm:ss.fff");

                bool success = await _userRepo.SaveData("UPDATE user SET password_change_token = @token, password_change_time = @time WHERE email = @email",
                                new { token, time, resetRequest.Email });

                if (!success) { return BadRequest(); }


                string resetUrl = $"https://localhost:44492/pakeisti-slaptazodi?email={HttpUtility.UrlEncode(resetRequest.Email)}&token={HttpUtility.UrlEncode(token)}";

                SendEmail emailer = new SendEmail();
                if (await emailer.changePassword(result, resetUrl))
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError("Failed to send email");
                    return StatusCode(401);
                }
            }
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(PasswordChangeViewModel passwordChange)
        {
            string sql = "SELECT password_change_token, password_change_time FROM user WHERE email = @email AND password_change_token = @token";
            var parameters = new { email = passwordChange.Email, token = passwordChange.Token };
            var result = await _userRepo.LoadData(sql, parameters);
            if (string.IsNullOrWhiteSpace(passwordChange.Password) || string.IsNullOrWhiteSpace(passwordChange.ConfirmPassword))
            {
                return BadRequest(new { message = "Privaloma užpildyti abu slaptažodžio laukus!" });
            }
            if (passwordChange.Password != passwordChange.ConfirmPassword)
            {
                return BadRequest(new { message = "Slaptažodžiai privalo sutapti!" });
            }
            if (!IsPasswordValid(passwordChange.Password))
            {
                return BadRequest(new { message = "Slaptažodis turi turėti mažiausiai 8 simbolius, bent vieną didžiają raidę, skaičių ir specialų simbolį!" });
            }
            if (result.Rows.Count == 0)
            {
                return StatusCode(401);
            }
            else
            {
                DateTime timer = DateTime.Parse(result.Rows[0]["password_change_time"].ToString());

                if (timer > DateTime.Now)
                {
                    byte[] salt;
                    string password_hash = PasswordHash.hashPassword(passwordChange.Password, out salt);
                    string password_salt = Convert.ToBase64String(salt);

                    bool success = await _userRepo.SaveData("UPDATE user SET password_hash = @password_hash, password_salt = @password_salt, password_change_token = NULL, password_change_time = NULL WHERE password_change_token = @token",
                    new { password_hash, password_salt, token = passwordChange.Token });

                    if (success)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return StatusCode(300);
                }

            }
        }
    }
}
