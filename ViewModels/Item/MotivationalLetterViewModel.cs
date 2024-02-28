using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class MotivationalLetterViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("letter")]
        public string Letter { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }
}
