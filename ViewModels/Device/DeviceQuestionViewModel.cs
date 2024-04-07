using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class DeviceQuestionViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }
    }
}
