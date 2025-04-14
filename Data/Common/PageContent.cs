// File: Data/Common/PageContent.cs
using System.ComponentModel.DataAnnotations;

namespace LehmanCustomConstruction.Data.Common // Adjust namespace if needed
{
    public class PageContent
    {
        [Key] // Primary Key
        [StringLength(100)] // Limit key length
        public string PageKey { get; set; } = string.Empty; // e.g., "AboutUsMain", "HomePageHero"

        [Required]
        public string HtmlContent { get; set; } = string.Empty; // Stores the HTML from the editor

        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}