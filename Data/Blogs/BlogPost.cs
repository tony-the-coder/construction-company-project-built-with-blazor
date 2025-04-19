// File: Data/Blogs/BlogPost.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LehmanCustomConstruction.Data.Blogs
{
    public class BlogPost
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Title is required.")] // Add validation
        [StringLength(200)] // Add max length
        public string Title { get; set; } = string.Empty; // Initialize non-nullable string

        public string? Content { get; set; } // Keep nullable if content can be empty initially

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Author { get; set; }

        [StringLength(500)] // URL max length
        [Url(ErrorMessage = "Please enter a valid URL.")] // Add URL validation
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Slug is required.")]
        [StringLength(250)] // Add max length
        public string Slug { get; set; } = string.Empty; // Initialize non-nullable string

        [Required(ErrorMessage = "Excerpt is required.")]
        [StringLength(500)] // Add max length
        public string Excerpt { get; set; } = string.Empty; // Initialize non-nullable string

        public bool IsPublished { get; set; } = false;

        // --- INITIALIZE and make VIRTUAL ---
        public virtual ICollection<BlogPostCategory> BlogPostCategories { get; set; } = new List<BlogPostCategory>();
    }
}