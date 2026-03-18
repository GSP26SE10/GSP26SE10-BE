using System;
using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class ContactRequestCreateRequest
    {
        public int? CustomerId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }
    }

    public class ContactRequestUpdateRequest
    {
        [EnumDataType(typeof(ContactRequestStatus))]
        public ContactRequestStatus Status { get; set; }
    }

    public class ContactRequestFilterRequest
    {
        public int ContactRequestId { get; set; }
        public int? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public ContactRequestStatus? Status { get; set; }
    }
}