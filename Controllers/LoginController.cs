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

namespace mainykdovanok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserRepo _userRepo;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
            _userRepo = new UserRepo();
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

        [HttpPost("login")]
        public async Task<IActionResult> login(LoginViewModel loginData)
        {
            string sql = "SELECT user_id, name, surname, password_hash, password_salt, verification_token, user_role, fk_user_status FROM user WHERE email = @email";
            var parameters = new { email = loginData.Email };
            var result = await _userRepo.LoadData(sql, parameters);

            if (string.IsNullOrWhiteSpace(loginData.Email) || string.IsNullOrWhiteSpace(loginData.Password))
            {
                return BadRequest(new { message = "Visi laukai turi būti užpildyti!" });
            }

            if (!IsEmailValid(loginData.Email))
            {
                return BadRequest(new { message = "Neteisingas el. pašto formatas!" });
            }

            if (result.Rows.Count == 0)
            {
                return NotFound(new { message = "Šis naudotojas nėra registruotas sistemoje" });
            }

            int userStatus = Convert.ToInt32(result.Rows[0]["fk_user_status"]);
            if (userStatus == 2)
            {
                return Unauthorized(new { message = "Jūsų paskyra yra užblokuota. Susisiekite su administratoriumi." });
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
                return Unauthorized(new { message = "Neteisingas slaptažodis!" });
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

            if (string.IsNullOrWhiteSpace(registration.Name) || string.IsNullOrWhiteSpace(registration.Surname) || string.IsNullOrWhiteSpace(registration.Email) || string.IsNullOrWhiteSpace(registration.Password) || string.IsNullOrWhiteSpace(registration.ConfirmPassword))
            {
                return BadRequest(new { message = "Visi laukai turi būti užpildyti!" });
            }

            if (!IsEmailValid(registration.Email))
            {
                return BadRequest(new { message = "Neteisingas el. pašto formatas!" });
            }

            if (!IsPasswordValid(registration.Password))
            {
                return BadRequest(new { message = "Slaptažodis turi turėti mažiausiai 8 simbolius, bent vieną didžiają raidę, skaičių ir specialų simbolį!" });
            }

            if (registration.Password != registration.ConfirmPassword)
            {
                return BadRequest(new { message = "Slaptažodžiai turi sutapti!" });
            }

            bool emailExists = await _userRepo.CheckEmailExists(registration.Email);
            if (emailExists)
            {
                return BadRequest(new { message = "Šis elektroninis paštas jau yra užregistruotas!" });
            }

            byte[] salt;
            string password_hash = PasswordHash.hashPassword(registration.Password, out salt);
            string password_salt = Convert.ToBase64String(salt);

            byte[] tokenData = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(tokenData);
            }

            string token = BitConverter.ToString(tokenData).Replace("-", "");

            bool success = await _userRepo.SaveData("INSERT INTO user (name, surname, email, password_hash, password_salt, verification_token) VALUES (@name, @surname, @email, @password_hash, @password_salt, @token)",
                    new { registration.Name, registration.Surname, registration.Email, password_hash, password_salt, token });

            if (success)
            {
                return Ok();

            }
            else
            {
                return BadRequest();
            }
        }


        [HttpGet("isLoggedIn")]
        public IActionResult IsLoggedIn()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);
                int userRole = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.Role).Value);
                string userEmail = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
                _logger.LogInformation($"User #{userId} with email {userEmail} is logged in and is the required role.");
                return Ok(new { UserRole = userRole });
            }
            else
            {
                _logger.LogInformation($"User is not logged in.");
                return Unauthorized();
            }
        }
    }
}
