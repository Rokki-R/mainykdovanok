using Newtonsoft.Json;

namespace mainykdovanok.ViewModels
{
    public class ItemCategoryViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
