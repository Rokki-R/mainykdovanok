using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class DeviceImageViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("file")]
        public IFormFile File { get; set; }

        [JsonProperty("data")]
        public byte[] Data { get; set; }
    }
}
