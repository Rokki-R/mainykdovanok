using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.User
{
    public class EmailVerificationViewModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
