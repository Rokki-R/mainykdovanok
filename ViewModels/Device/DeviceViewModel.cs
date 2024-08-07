﻿using Newtonsoft.Json;

namespace mainykdovanok.ViewModels.Device
{
    public class DeviceViewModel
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

        [JsonProperty("winner_draw_datetime")]
        public DateTime? WinnerDrawDateTime { get; set; }

        [JsonProperty("fk_winner")]
        public int? WinnerId { get; set; }

        [JsonProperty("images")]
        public List<DeviceImageViewModel> Images { get; set; }
        [JsonProperty("questions")]
        public List<DeviceQuestionViewModel>? Questions { get; set; }
    }
}
