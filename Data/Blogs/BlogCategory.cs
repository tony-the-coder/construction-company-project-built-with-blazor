using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LehmanCustomConstruction.Data.Blogs
{
    public class BlogCategory
    {
        public int ID { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Slug { get; set; }

        public ICollection<BlogPostCategory>? BlogPostCategories { get; set; }
    }
}