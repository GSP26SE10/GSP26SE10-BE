using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class TaskTemplateCreateRequest
    {
        public string? TaskName { get; set; }
        public bool? IsActive { get; set; }
    }

    public class TaskTemplateUpdateRequest
    {
        public string? TaskName { get; set; }
        public bool? IsActive { get; set; }
    }

    public class TaskTemplateFilterRequest
    {
        public int TaskTemplateId { get; set; }
        public string? TaskName { get; set; }
        public bool? IsActive { get; set; }
    }
}
