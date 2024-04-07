using Newtonsoft.Json;

namespace mainykdovanok.ViewModels
{
    public class DeviceCategoryViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
