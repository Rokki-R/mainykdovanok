using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.User
{
    public class PasswordChangeViewModel
    {
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("confirm_password")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
