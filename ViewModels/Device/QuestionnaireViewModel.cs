using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class QuestionnaireViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }
}
