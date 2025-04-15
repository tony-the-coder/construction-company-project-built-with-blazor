// File: Data/Common/CustomerDocument.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For ForeignKey attribute

namespace LehmanCustomConstruction.Data.Common // Adjust namespace if needed
{
    public class CustomerDocument
    {
        [Key]
        public int Id { get; set; }

        // --- Who uploaded it? ---
        [Required]
        public string UploadedById { get; set; } = string.Empty; // FK to AspNetUsers table (ApplicationUser.Id)
        // Optional: Navigation property back to the user who uploaded
        [ForeignKey("UploadedById")]
        public virtual ApplicationUser? UploadedBy { get; set; }

        // --- Who is it FOR? ---
        // Link to the Customer this document belongs to (even if uploaded by admin)
        [Required]
        public string TargetUserId { get; set; } = string.Empty; // FK to AspNetUsers table (ApplicationUser.Id)
        // Optional: Navigation property back to the target user
        [ForeignKey("TargetUserId")]
        public virtual ApplicationUser? TargetUser { get; set; }

        // --- When Ready: Link to a specific Project ---
        // public int? PortfolioProjectId { get; set; } // FK to your Project table
        // [ForeignKey("PortfolioProjectId")]
        // public virtual PortfolioProject? PortfolioProject { get; set; }

        // --- File Details ---
        [Required]
        [StringLength(255)] // Max length for original filenames
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)] // Store the unique name used on disk
        public string UniqueFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)] // Store the relative path or key within your secure storage
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; } // e.g., "application/pdf", "image/jpeg"

        public long FileSize { get; set; } // Size in bytes

        public DateTime UploadedTimestamp { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Description { get; set; } // Optional description added during upload

        // Optional: Flag to indicate if admin needs to review/approve customer upload?
        // public bool RequiresAdminApproval { get; set; } = false; 
    }
}