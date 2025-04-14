// File: Data/ApplicationDbContext.cs
using LehmanCustomConstruction.Data.Blogs;
using LehmanCustomConstruction.Data.Common; // Ensure this namespace is correct for PageContent
// using LehmanCustomConstruction.Data.Portfolio; // Add later when needed
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System; // Required for DateTime

namespace LehmanCustomConstruction.Data // Adjust namespace if needed
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // --- Blog DbSets ---
        public DbSet<BlogPost> BlogPosts { get; set; } = null!;
        public DbSet<BlogCategory> BlogCategories { get; set; } = null!;
        public DbSet<BlogPostCategory> BlogPostCategories { get; set; } = null!; // Join entity

        // --- Page Content DbSet ---
        public DbSet<PageContent> PageContents { get; set; } = null!;

        // --- Portfolio DbSets (Add later) ---
        // public DbSet<PortfolioProject> PortfolioProjects { get; set; } = null!;
        // public DbSet<PortfolioCategory> PortfolioCategories { get; set; } = null!; 
        // public DbSet<PortfolioImage> PortfolioImages { get; set; } = null!;
        // public DbSet<PortfolioProjectCategoryRel> PortfolioProjectCategoryRelationships { get; set; } = null!; 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // IMPORTANT: Keep this for Identity tables

            // --- Configure PageContent ---
            builder.Entity<PageContent>(entity =>
            {
                entity.HasKey(p => p.PageKey);
            });

            // --- Configure Blog Relationships ---
            builder.Entity<BlogPostCategory>(entity =>
            {
                entity.HasKey(bc => new { bc.BlogPostId, bc.BlogCategoryId });

                entity.HasOne(bc => bc.BlogPost)
                      .WithMany(b => b.BlogPostCategories)
                      .HasForeignKey(bc => bc.BlogPostId);

                entity.HasOne(bc => bc.BlogCategory)
                      .WithMany(c => c.BlogPostCategories)
                      .HasForeignKey(bc => bc.BlogCategoryId);
            });

            // --- Configure Portfolio Relationships (Add later) ---
            // ...

            // --- Seed Data ---
            // Blog Categories 
            builder.Entity<BlogCategory>().HasData(
                new BlogCategory { ID = 1, Name = "Company News", Slug = "company-news" },
                new BlogCategory { ID = 2, Name = "Project Spotlights", Slug = "project-spotlights" },
                new BlogCategory { ID = 3, Name = "Design Ideas", Slug = "design-ideas" },
                new BlogCategory { ID = 4, Name = "Construction Tips", Slug = "construction-tips" }
            );

            // Page Content (Seed the About Us content with a FIXED date)
            builder.Entity<PageContent>().HasData(
                new PageContent
                {
                    PageKey = "AboutUsMain",
                    HtmlContent = @"<p>Lehman Custom Construction is built on a foundation of quality, integrity, and partnership. Founded by Tom Lehman, our passion lies in translating your vision into a home that is both uniquely yours and built to the highest standards of craftsmanship.</p><p>We believe the custom home building process should be collaborative and transparent. From initial concept sketches to the final walkthrough, we work closely with you, ensuring every detail reflects your lifestyle and preferences.</p><h2>Our Approach</h2><p>Our approach combines time-honored building techniques with modern innovations. We partner with skilled architects, designers, and tradespeople who share our commitment to excellence. Key elements include:</p><ul><li><strong>Personalized Design:</strong> Tailoring every aspect to your needs.</li><li><strong>Quality Materials:</strong> Sourcing durable and beautiful materials.</li><li><strong>Transparent Communication:</strong> Keeping you informed every step of the way.</li><li><strong>Expert Project Management:</strong> Ensuring timelines and budgets are respected.</li></ul><p>Building a custom home is a significant journey, and we are honored to be considered as your guide and partner.</p>",
                    // --- FIXED DATE --- Use a specific, consistent date to avoid migration warnings
                    DateModified = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Portfolio Categories Seed Data (Add later if needed)
            // ...
        }
    }
}