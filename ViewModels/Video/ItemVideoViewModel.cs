using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Video
{
    public class ItemVideoViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("file")]
        public IFormFile File { get; set; }

        [JsonProperty("data")]
        public byte[] Data { get; set; }
    }
}
