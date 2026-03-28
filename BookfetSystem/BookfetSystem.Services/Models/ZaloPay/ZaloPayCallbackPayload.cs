using System.Text.Json.Serialization;

namespace BookfetSystem.Services.Models.ZaloPay
{
    public class ZaloPayCallbackPayload
    {
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("mac")]
        public string? Mac { get; set; }

        [JsonPropertyName("type")]
        public int? Type { get; set; }
    }
}
