using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class ItemWinnerViewModel
    {
        [JsonProperty("winnerId")]
        public int WinnerId { get; set; }

        [JsonProperty("itemId")]
        public int ItemId { get; set; }

        [JsonProperty("itemName")]
        public string ItemName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("posterEmail")]
        public string PosterEmail { get; set; }
    }
}
