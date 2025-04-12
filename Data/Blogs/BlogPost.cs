// File: Data/Blogs/BlogPost.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LehmanCustomConstruction.Data.Blogs
{
    public class BlogPost
    {
        public int ID { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;
        public string? Author { get; set; }
        public string? ImageUrl { get; set; }
        public string? Slug { get; set; }
        public string? Excerpt { get; set; }

    
        public bool IsPublished { get; set; } = false; 
        public ICollection<BlogPostCategory>? BlogPostCategories { get; set; }
    }
}