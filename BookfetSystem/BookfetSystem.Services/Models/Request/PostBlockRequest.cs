using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class PostBlockCreateRequest
    {
        [Required(ErrorMessage = "PostId is required.")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [EnumDataType(typeof(PostBlockType), ErrorMessage = "Invalid block type.")]
        public PostBlockType Type { get; set; }

        public int Position { get; set; }

        public JsonElement? Data { get; set; }
    }

    public class PostBlockUpdateRequest
    {
        [Required(ErrorMessage = "Type is required.")]
        [EnumDataType(typeof(PostBlockType), ErrorMessage = "Invalid block type.")]
        public PostBlockType Type { get; set; }

        public int Position { get; set; }

        public JsonElement? Data { get; set; }
    }

    public class PostBlockFilterRequest
    {
        public int PostBlockId { get; set; }
        public int PostId { get; set; }
        /// <summary>
        /// Filter by block type enum value.
        /// </summary>
        public PostBlockType? Type { get; set; }
    }
}
