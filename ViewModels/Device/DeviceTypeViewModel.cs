using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class DeviceTypeViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
