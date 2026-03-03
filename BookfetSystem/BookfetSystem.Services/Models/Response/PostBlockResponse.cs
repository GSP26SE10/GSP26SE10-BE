using System;
using System.Text.Json;

namespace BookfetSystem.Services.Models.Response
{
    public class PostBlockResponse
    {
        public int PostBlockId { get; set; }
        public int PostId { get; set; }
        public string? Type { get; set; }
        public int Position { get; set; }
        public JsonElement? Data { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
