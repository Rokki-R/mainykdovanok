using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Item
{
    public class CommentViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("userId")]
        public int UserId { get; set; }
        [JsonProperty("itemId")]
        public int ItemId { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("userSurname")]
        public string UserSurname { get; set; }
        [JsonProperty("post_datetime")]
        public DateTime PostDateTime { get; set; }
    }
}
