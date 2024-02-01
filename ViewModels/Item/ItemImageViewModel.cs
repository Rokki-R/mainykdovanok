using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class ItemImageViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("file")]
        public IFormFile File { get; set; }

        [JsonProperty("data")]
        public byte[] Data { get; set; }
    }
}
