using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.User
{
    public class PasswordResetViewModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
