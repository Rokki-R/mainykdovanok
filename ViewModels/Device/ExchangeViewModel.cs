using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class ExchangeViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("images")]
        public List<DeviceImageViewModel> Images { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }
        [JsonProperty("devices_won")]
        public int DevicesWon { get; set; }
        [JsonProperty("devices_gifted")]
        public int DevicesGifted { get; set; }
    }
}
