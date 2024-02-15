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
        [JsonProperty("items_won")]
        public int itemsWon { get; set; }
        [JsonProperty("items_gifted")]
        public int itemsGifted { get; set; }
    }
}
