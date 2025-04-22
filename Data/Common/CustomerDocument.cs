// File Path: LehmanCustomConstruction.Data/Common/CustomerDocument.cs
using LehmanCustomConstruction.Data; // For ApplicationUser
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [ForeignKey], [InverseProperty]

namespace LehmanCustomConstruction.Data.Common // Or adjust if your structure differs
{
    public class CustomerDocument
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Original file name is required.")]
        [StringLength(260, ErrorMessage = "File name is too long.")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stored file name is required.")]
        [StringLength(260)] // Assuming GUID + extension fits
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
        public string UploadedById { get; set; } = string.Empty; // Foreign key property (matches ApplicationUser.Id)

        // --- Navigation property for the User who uploaded ---
        [ForeignKey("UploadedById")]
        [InverseProperty("DocumentsUploaded")] // <<< CORRECTED to match ApplicationUser.cs
        public virtual ApplicationUser? UploadedBy { get; set; }

        // --- Foreign Key for the User the document is intended for/associated with ---
        [Required]
        public string TargetUserId { get; set; } = string.Empty; // Foreign key property (matches ApplicationUser.Id)

        // --- Navigation property for the Target User ---
        [ForeignKey("TargetUserId")]
        [InverseProperty("DocumentsForUser")] // <<< CORRECTED to match ApplicationUser.cs
        public virtual ApplicationUser? TargetUser { get; set; }

        // Optional: Description field
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        // ************************************************
        // --- PROPERTY FOR SOFT DELETE (ADDED BACK) ---
        // ************************************************
        [Required]
        public bool IsDeleted { get; set; } = false; // Default to false (not deleted)

        // ************************************************
        // --- Optional: Timestamp for deletion ---
        // ************************************************
        // public DateTime? DeletedTimestamp { get; set; }

    }
}