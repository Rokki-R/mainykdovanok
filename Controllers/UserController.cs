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
                return NotFound(new { message = "Šis naudotojas nėra registruotas sistemoje" });
            }

            //if (!String.Equals(result.Rows[0]["verification_token"].ToString(), ""))
            //{
             //   return StatusCode(401);
           // }

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
            bool emailExists = await _userRepo.CheckEmailExists(registration.Email);
            if (emailExists)
            {
                return BadRequest(new { message = "Šis elektroninis paštas jau yra užregistruotas!" });
            }

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
                //string verifyUrl;
                //verifyUrl = $"https://localhost:44456/verifyemail?email={HttpUtility.UrlEncode(registration.Email)}&token={HttpUtility.UrlEncode(token)}";

                //SendEmail emailer = new SendEmail();
                //if (await emailer.verifyEmail(registration.Email, verifyUrl))
                // {
                //  return Ok();
                //}
                //else
                //{
                //    _logger.LogError("Failed to send email");
                //   return StatusCode(401);
                // }
                return Ok();

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

        [HttpGet("getMyProfileDetails")]
        public async Task<IActionResult> GetMyProfileDetails()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            int userId = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            string sql = "SELECT name, surname, email, items_gifted, items_won FROM users WHERE user_id = @user_id";
            var parameters = new { user_id = userId };
            var result = await _userRepo.LoadData(sql, parameters);

            if (result.Rows.Count == 0)
            {
                return BadRequest();
            }

            string name = result.Rows[0]["name"].ToString();
            string surname = result.Rows[0]["surname"].ToString();
            string email = result.Rows[0]["email"].ToString();
            int itemsGifted = Convert.ToInt32(result.Rows[0]["items_gifted"]);
            int itemsWon = Convert.ToInt32(result.Rows[0]["items_won"]);

            sql = "SELECT image FROM user_profile_images WHERE fk_user = @user_id";
            parameters = new { user_id = userId };
            result = await _userRepo.LoadData(sql, parameters);
            byte[] user_profile_image = null;

            if (result.Rows.Count > 0)
            {
                user_profile_image = (byte[])result.Rows[0]["image"];
            }

            return Ok(new { name, surname, email, itemsGifted, itemsWon, user_profile_image });
        }

        [HttpPost("updateProfileDetails")]
        [Authorize]
        public async Task<IActionResult> UpdateProfileDetails()
        {
            var form = await Request.ReadFormAsync();
            string name = form["name"].ToString();
            string surname = form["surname"].ToString();
            string email = form["email"].ToString();
            string old_password = form["old_password"].ToString();
            string new_password = form["new_password"].ToString();
            IFormFile image = Request.Form.Files.GetFile("user_profile_image");
            byte[] imageBytes = null;

            if (image != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    imageBytes = await ImageCompressor.ResizeCompressImage(image, 128, 128);
                }
            }

            int user_id = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            // Check if there already is a user with the same email.
            string sql = "SELECT user_id FROM users WHERE email = @email";
            var parameters_email = new { email };
            var result_email = await _userRepo.LoadData(sql, parameters_email);

            if (result_email.Rows.Count > 0)
            {
                int existingUserId = Convert.ToInt32(result_email.Rows[0]["user_id"]);
                if (existingUserId != user_id)
                {
                    return BadRequest("Šis el. paštas užimtas!");
                }
            }

            // Retrieve the user's hashed password and salt, then compare it to the new hashed plain text version.
            sql = "SELECT password_hash, password_salt FROM users WHERE user_id = @user_id";
            var parameters_password = new { user_id };
            var result_password = await _userRepo.LoadData(sql, parameters_password);

            string hashed_password = result_password.Rows[0]["password_hash"].ToString();
            string password_salt = result_password.Rows[0]["password_salt"].ToString();

            bool match = false;
            try
            {
                match = PasswordHash.doesPasswordMatch(old_password, hashed_password, password_salt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while comparing passwords during profile update: {ex}", ex);
                return StatusCode(500);
            }

            if (match)
            {
                byte[] salt;
                string password_hash = PasswordHash.hashPassword(new_password, out salt);
                password_salt = Convert.ToBase64String(salt);

                sql = "UPDATE users SET name = @name, surname = @surname, email = @email, password_hash = @password_hash, password_salt = @password_salt WHERE user_id = @user_id";
                var parameters_update = new { name, surname, email, password_hash, password_salt, user_id };
                await _userRepo.SaveData(sql, parameters_update);
            }
            else if (old_password == "" && new_password == "")
            {
                sql = "UPDATE users SET name = @name, surname = @surname, email = @email WHERE user_id = @user_id";
                var parameters_update = new { name, surname, email, user_id };
                await _userRepo.SaveData(sql, parameters_update);
            }
            else
            {
                return BadRequest("Neteisingas slaptažodis!");
            }

            if (image != null)
            {
                sql = "SELECT id FROM user_profile_images WHERE fk_user = @user_id";
                var parameters_user_profile_image = new { user_id };
                var result_profile_image = await _userRepo.LoadData(sql, parameters_user_profile_image);
                if (result_profile_image.Rows.Count > 0)
                {
                    sql = "UPDATE user_profile_images SET image = @user_profile_image WHERE fk_user = @user_id";
                    var parameters_update_user_profile_image = new { user_profile_image = imageBytes, user_id };
                    await _userRepo.SaveData(sql, parameters_update_user_profile_image);
                }
                else
                {
                    sql = "INSERT INTO user_profile_images (image, fk_user) VALUES (@user_profile_image, @user_id)";
                    var parameters_insert_user_profile_image = new { user_profile_image = imageBytes, user_id };
                    await _userRepo.SaveData(sql, parameters_insert_user_profile_image);
                }
            }
            return Ok();
        }
        [HttpGet("getMyProfileImage")]
        public async Task<IActionResult> GetMyProfileImage()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                // Return Ok because we don't want an error to show up in the console.
                // This method for now is only used in the NavMenu, which can be called when the user is not logged in.
                return Ok("./images/profile.png");
            }
            int user_id = Convert.ToInt32(HttpContext.User.FindFirst("user_id").Value);

            string sql = "SELECT image FROM user_profile_images WHERE fk_user = @user_id";
            var parameters = new { user_id };
            var result = await _userRepo.LoadData(sql, parameters);

            if (result.Rows.Count == 0)
            {
                return Ok("./images/profile.png");
            }
            byte[] user_profile_image = (byte[])result.Rows[0]["image"];
            return Ok(new { user_id, user_profile_image });
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

        [HttpGet("getUserProfileImage/{userId}")]
        public async Task<IActionResult> GetUserProfileImage(int userId)
        {
            string sql = "SELECT image FROM user_profile_images WHERE fk_user = @userId";
            var parameters = new { userId };
            var result = await _userRepo.LoadData(sql, parameters);

            if (result.Rows.Count == 0)
            {
                return Ok("./images/profile.png");
            }
            byte[] user_profile_image = (byte[])result.Rows[0]["image"];
            return Ok(new { userId, user_profile_image });
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

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(PasswordResetViewModel resetRequest)
        {
            string sql = "SELECT email FROM users WHERE email = @email";
            var parameters = new { email = resetRequest.Email };
            var result = await _userRepo.LoadData(sql, parameters);

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

                bool success = await _userRepo.SaveData("UPDATE users SET password_change_token = @token, password_change_time = @time WHERE email = @email",
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
            string sql = "SELECT password_change_token, password_change_time FROM users WHERE email = @email AND password_change_token = @token";
            var parameters = new { email = passwordChange.Email, token = passwordChange.Token };
            var result = await _userRepo.LoadData(sql, parameters);

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

                    bool success = await _userRepo.SaveData("UPDATE users SET password_hash = @password_hash, password_salt = @password_salt, password_change_token = NULL, password_change_time = NULL WHERE password_change_token = @token",
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
