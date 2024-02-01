using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class ItemQuestionsViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }
    }
}
