using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class DeviceWinnerViewModel
    {
        [JsonProperty("winnerId")]
        public int WinnerId { get; set; }

        [JsonProperty("deviceId")]
        public int DeviceId { get; set; }

        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("posterEmail")]
        public string PosterEmail { get; set; }
    }
}
