using System;

namespace BookfetSystem.Services.Models.Response
{
    public class TaskTemplateResponse
    {
        public int TaskTemplateId { get; set; }
        public int? OwnerId { get; set; }
        public string? TaskName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
