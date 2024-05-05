using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class MotivationalLetterViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("letter")]
        public string Letter { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("surname")]
        public string Surname { get; set; }
        [JsonProperty("devices_won")]
        public int DevicesWon { get; set; }
        [JsonProperty("devices_gifted")]
        public int DevicesGifted { get; set; }
    }
}
