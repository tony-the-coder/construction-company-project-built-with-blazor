using LehmanCustomConstruction.Data.Blogs;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LehmanCustomConstruction.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<BlogPost> BlogPosts { get; set; } = null!;
    public DbSet<BlogPostCategory> BlogPostCategories { get; set; } = null!;
    public DbSet<BlogCategory> BlogCategories { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Define composite primary key for BlogPostCategory
        builder.Entity<BlogPostCategory>()
            .HasKey(bc => new { bc.BlogPostId, bc.BlogCategoryId });

        // Define relationship between BlogPostCategory and BlogPost
        builder.Entity<BlogPostCategory>()
            .HasOne(bc => bc.BlogPost)
            .WithMany(b => b.BlogPostCategories)
            .HasForeignKey(bc => bc.BlogPostId);

        // Define relationship between BlogPostCategory and BlogCategory
        builder.Entity<BlogPostCategory>()
            .HasOne(bc => bc.BlogCategory)
            .WithMany(c => c.BlogPostCategories)
            .HasForeignKey(bc => bc.BlogCategoryId);
    }
}
