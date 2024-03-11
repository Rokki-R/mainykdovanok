using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class ItemViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fk_user")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("participants")]
        public int? Participants { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("creation_datetime")]
        public DateTime CreationDateTime { get; set; }

        [JsonProperty("end_datetime")]
        public DateTime? EndDateTime { get; set; }

        [JsonProperty("fk_winner")]
        public int? WinnerId { get; set; }

        [JsonProperty("images")]
        public List<ItemImageViewModel> Images { get; set; }
        [JsonProperty("questions")]
        public List<ItemQuestionViewModel>? Questions { get; set; }
    }
}
