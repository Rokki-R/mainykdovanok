using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class DeviceConfirmViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("userId")]
        public int OwnerId { get; set; }
        [JsonProperty("winnerId")]
        public int WinnerId { get; set; }

    }
}
