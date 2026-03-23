using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class MessageCreateRequest
    {
        [Required(ErrorMessage = "ConversationId is required.")]
        public int ConversationId { get; set; }

        [Required(ErrorMessage = "SenderId is required.")]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string? Content { get; set; }
        public string? MessageType { get; set; } // TEXT | MENU
        public int? MenuId { get; set; }
    }

    public class MessageUpdateRequest
    {
        [Required(ErrorMessage = "ConversationId is required.")]
        public int ConversationId { get; set; }

        [Required(ErrorMessage = "SenderId is required.")]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string? Content { get; set; }
    }

    public class MessageFilterRequest
    {
        public int MessageId { get; set; }
        public int? ConversationId { get; set; }
        public int? SenderId { get; set; }
        public string? Content { get; set; }
    }
}
