// File: Data/Common/ContactInquiry.cs
using System.ComponentModel.DataAnnotations;

namespace LehmanCustomConstruction.Data.Common // Adjust namespace if needed
{
    public enum InquiryStatus // Enum for clear status tracking
    {
        New,
        Contacted,
        Resolved,
        Archived
    }

    public class ContactInquiry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Phone { get; set; } // Optional phone number

        [StringLength(150)]
        public string? Subject { get; set; } // Optional subject

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public InquiryStatus Status { get; set; } = InquiryStatus.New; // Default to New

        [StringLength(1000)]
        public string? AdminNotes { get; set; } // For internal notes
    }
}