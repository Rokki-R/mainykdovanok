using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class DeviceLotteryViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fk_user")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("participants")]
        public int Participants { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}
