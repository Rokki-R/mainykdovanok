using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.User
{
    public class UserViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("surname")]
        public string Surname { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("devices_won")]
        public int devicesWon { get; set; }
        [JsonProperty("devices_gifted")]
        public int devicesGifted { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("role")]
        public int Role { get; set; }
    }
}
