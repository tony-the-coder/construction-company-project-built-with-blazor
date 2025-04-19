using LehmanCustomConstruction.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// No need for using Microsoft.AspNetCore.Identity here as the ApplicationUser is in the same namespace level

namespace LehmanCustomConstruction.Data.Common // Adjust namespace if your project structure differs
{
    public class CustomerDocument
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Original file name is required.")]
        [StringLength(260, ErrorMessage = "File name is too long.")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(260)]
        public string StoredFileName { get; set; } = string.Empty; // Unique name for storage

        [Required(ErrorMessage = "Content type is required.")]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty; // MIME type

        [Required]
        public long FileSize { get; set; } // Size in bytes

        [Required]
        public DateTime UploadTimestamp { get; set; } = DateTime.UtcNow;

        // --- Foreign Key for the User who uploaded the file ---
        [Required]
        public string UploadedById { get; set; } = string.Empty; // Foreign key property

        // --- Navigation property for the User who uploaded ---
        // This name 'UploadedBy' matches the InverseProperty in ApplicationUser
        public virtual ApplicationUser? UploadedBy { get; set; }

        // --- Foreign Key for the User the document is intended for/associated with ---
        // This could be the same as UploadedById or an Admin/Staff member depending on your logic
        [Required]
        public string TargetUserId { get; set; } = string.Empty; // Foreign key property

        // --- Navigation property for the Target User ---
        // This name 'TargetUser' matches the InverseProperty in ApplicationUser
        public virtual ApplicationUser? TargetUser { get; set; }

        // Optional: Description field
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        // Optional: Link to a specific Project entity if you have one
        // public int? ProjectId { get; set; }
        // [ForeignKey("ProjectId")]
        // public virtual Project? Project { get; set; }
    }
}