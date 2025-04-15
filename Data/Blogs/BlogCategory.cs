// File: Data/Blogs/BlogCategory.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LehmanCustomConstruction.Data.Blogs
{
    public class BlogCategory
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100)] // Add max length
        public string Name { get; set; } = string.Empty; // Initialize non-nullable string

        [Required(ErrorMessage = "Category Slug is required.")] // Slug should usually be required too
        [StringLength(150)] // Add max length
        public string Slug { get; set; } = string.Empty; // Initialize non-nullable string

        // --- INITIALIZE and make VIRTUAL ---
        public virtual ICollection<BlogPostCategory> BlogPostCategories { get; set; } = new List<BlogPostCategory>();
    }
}