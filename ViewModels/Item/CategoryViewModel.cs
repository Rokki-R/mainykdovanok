using Newtonsoft.Json;

namespace mainykdovanok.ViewModels
{
    public class CategoryViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
