using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class ExchangeViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("winner_draw_datetime")]
        public DateTime? WinnerDrawDateTime { get; set; }

        [JsonProperty("images")]
        public List<DeviceImageViewModel> Images { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }
}
