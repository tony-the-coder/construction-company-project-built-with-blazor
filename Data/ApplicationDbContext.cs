// File: Data/ApplicationDbContext.cs
using LehmanCustomConstruction.Data.Blogs;
using LehmanCustomConstruction.Data.Common;
// using LehmanCustomConstruction.Data.Portfolio; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace LehmanCustomConstruction.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // --- Blog DbSets ---
        public DbSet<BlogPost> BlogPosts { get; set; } = null!;
        public DbSet<BlogCategory> BlogCategories { get; set; } = null!;
        // <<< REVERTED: Use the original DbSet name your repository expects >>>
        public DbSet<BlogPostCategory> BlogPostCategories { get; set; } = null!;

        // --- Page Content DbSet ---
        public DbSet<PageContent> PageContents { get; set; } = null!;

        // --- Contact Inquiry DbSet ---
        public DbSet<ContactInquiry> ContactInquiries { get; set; } = null!;

        // --- Customer Document DbSet ---
        public DbSet<CustomerDocument> CustomerDocuments { get; set; } = null!;

        // --- Portfolio DbSets (Add later) ---
        // ...

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- Configure PageContent ---
            builder.Entity<PageContent>(entity => { entity.HasKey(p => p.PageKey); });

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

            // --- Configure ContactInquiry ---
            builder.Entity<ContactInquiry>(entity => { entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50); });

            // --- Configure CustomerDocument Relationships ---
            builder.Entity<CustomerDocument>(entity =>
            {
                entity.HasOne(d => d.UploadedBy)
                      .WithMany(u => u.DocumentsUploaded)
                      .HasForeignKey(d => d.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.TargetUser)
                      .WithMany(u => u.DocumentsForUser)
                      .HasForeignKey(d => d.TargetUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Optional: Relationship to PortfolioProject (Uncomment if needed)
                /* ... */
            });

            // --- Configure Portfolio Relationships (Add later) ---
            // ...

            // --- Seed Data ---
            builder.Entity<BlogCategory>().HasData( /* ... your categories ... */ );
            builder.Entity<PageContent>().HasData( /* ... your page content ... */ );
        }
    }
}