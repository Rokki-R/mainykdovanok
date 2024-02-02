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

namespace mainykdovanok.Controllers.UserAuthentication
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserRepo _userRepo;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
            _userRepo = new UserRepo();
        }

        [HttpPost("login")]
        public async Task<IActionResult> login (LoginViewModel loginData)
        {
            string sql = "SELECT user_id, name, surname, password_hash, password_salt, verification_token, user_role FROM users WHERE email = @email";
            var parameters = new { email = loginData.Email };
            var result = await _userRepo.LoadData(sql, parameters);

            if (result.Rows.Count == 0)
            {
                return StatusCode(404);
            }

            if (!String.Equals(result.Rows[0]["verification_token"].ToString(), ""))
            {
                return StatusCode(401);
            }

            string hashed_password = result.Rows[0]["password_hash"].ToString();
            string password_salt = result.Rows[0]["password_salt"].ToString();

            bool match = false;
            try
            {
                match = PasswordHash.doesPasswordMatch(loginData.Password, hashed_password, password_salt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while comparing passwords: {ex}", ex);
                return StatusCode(404);
            }

            if (match) 
            {
                // Authenticate the user.
                int userId = Convert.ToInt32(result.Rows[0]["user_id"]);
                string name = result.Rows[0]["name"].ToString();
                string surname = result.Rows[0]["surname"].ToString();
                int user_role = Convert.ToInt32(result.Rows[0]["user_role"]);

                var claims = new List<Claim>
                {
                    new Claim("user_id", userId.ToString()),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Surname, surname),
                    new Claim(ClaimTypes.Email, loginData.Email),
                    new Claim(ClaimTypes.Role, user_role.ToString())

                };
                var identity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                return Ok();
            }

            else
            {
                return StatusCode(404);
            }

        }

        [HttpGet("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

                HttpContext.SignOutAsync();
                _logger.LogInformation($"User #{userId} logged out.");
                return Ok();
            }
            else
            {
                _logger.LogError($"Failed to sign out. Something went wrong - [Authorize] passed, but user might not be logged in?");
                return Unauthorized();
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationViewModel registration)
        {
            // Retrieve a hashed version of the user's plain text password.
            byte[] salt;
            string password_hash = PasswordHash.hashPassword(registration.Password, out salt);
            string password_salt = Convert.ToBase64String(salt);

            byte[] tokenData = new byte[32]; // 256-bit token
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(tokenData);
            }

            string token = BitConverter.ToString(tokenData).Replace("-", ""); // Convert byte array to hex string

            bool success = await _userRepo.SaveData("INSERT INTO users (name, surname, email, password_hash, password_salt, verification_token) VALUES (@name, @surname, @email, @password_hash, @password_salt, @token)",
                    new { registration.Name, registration.Surname, registration.Email, password_hash, password_salt, token });

            if (success)
            {
                string verifyUrl;
                verifyUrl = $"https://localhost:44456/verifyemail?email={HttpUtility.UrlEncode(registration.Email)}&token={HttpUtility.UrlEncode(token)}";

                SendEmail emailer = new SendEmail();
                if (await emailer.verifyEmail(registration.Email, verifyUrl))
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError("Failed to send email");
                    return StatusCode(401);
                }

            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("verifyEmail")]
        public async Task<IActionResult> VerifyEmail(EmailVerificationViewModel emailVerify)
        {
            string sql = "SELECT verification_token FROM users WHERE email = @email AND verification_token = @token";
            var parameters = new { email = emailVerify.Email, token = emailVerify.Token };
            var result = await _userRepo.LoadData(sql, parameters);

            if (result.Rows.Count == 0)
            {
                return StatusCode(404);
            }
            else
            {
                bool success = await _userRepo.SaveData("UPDATE users SET verification_token = NULL WHERE email = @email AND  verification_token = @token",
                    new { email = emailVerify.Email, token = emailVerify.Token });
                if (success)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [HttpGet("isLoggedIn/{requiredRole?}")]
        public IActionResult IsLoggedIn(int requiredRole = 0)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
                int userRole = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.Role).Value);
                string userEmail = HttpContext.User.FindFirst(ClaimTypes.Email).Value;

                if (userRole >= requiredRole)
                {
                    _logger.LogInformation($"User #{userId} with email {userEmail} is logged in and is the required role.");
                    return Ok();
                }
                else
                {
                    _logger.LogInformation($"User #{userId} with email {userEmail} is logged in but is not the required role.");
                    return Unauthorized();
                }
            }
            else
            {
                _logger.LogInformation($"User is not logged in.");
                return Unauthorized();
            }
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
    }
}
