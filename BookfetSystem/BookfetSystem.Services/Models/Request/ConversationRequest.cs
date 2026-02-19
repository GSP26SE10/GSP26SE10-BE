using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class ConversationCreateRequest
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "OwnerId is required.")]
        public int OwnerId { get; set; }
    }

    public class ConversationUpdateRequest
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "OwnerId is required.")]
        public int OwnerId { get; set; }
    }

    public class ConversationFilterRequest
    {
        public int ConversationId { get; set; }
        public int? CustomerId { get; set; }
        public int? OwnerId { get; set; }
    }
}
