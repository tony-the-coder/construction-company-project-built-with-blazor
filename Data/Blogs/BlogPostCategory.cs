using System.ComponentModel.DataAnnotations.Schema;

namespace LehmanCustomConstruction.Data.Blogs
{
    public class BlogPostCategory
    {
        [ForeignKey("BlogPost")]
        public int BlogPostId { get; set; }
        public BlogPost? BlogPost { get; set; }

        [ForeignKey("BlogCategory")]
        public int BlogCategoryId { get; set; }
        public BlogCategory? BlogCategory { get; set; }
    }
}