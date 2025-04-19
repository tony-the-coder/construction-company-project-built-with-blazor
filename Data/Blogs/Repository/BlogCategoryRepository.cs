// File Path: Data/Blogs/Repository/BlogCategoryRepository.cs
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Blogs;
using LehmanCustomConstruction.Data.Blogs.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider, CreateScope, GetRequiredService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Repository
{
    public class BlogCategoryRepository : IBlogCategoryRepository
    {
        // --- Inject IServiceProvider ---
        private readonly IServiceProvider _serviceProvider;

        public BlogCategoryRepository(IServiceProvider serviceProvider) // Updated constructor
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        // --- End IServiceProvider Injection ---

        // --- GetAllAsync ---
        public async Task<IEnumerable<BlogCategory>> GetAllAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogCategories
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }
        // -------------------

        // --- GetByIdAsync ---
        public async Task<BlogCategory?> GetByIdAsync(int categoryId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Use FindAsync if you don't need related data immediately
            return await context.BlogCategories.FindAsync(categoryId);
            // Or use FirstOrDefaultAsync if you need to include related posts:
            // return await context.BlogCategories
            //                     .Include(c => c.BlogPostCategories) // Example include
            //                     .FirstOrDefaultAsync(c => c.ID == categoryId);
        }
        // ------------------

        // --- AddAsync ---
        public async Task<BlogCategory> AddAsync(BlogCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Optional: Ensure Slug is set and formatted if necessary before adding
            // category.Slug = GenerateSlug(category.Slug ?? category.Name); // Example slug generation

            await context.BlogCategories.AddAsync(category);
            await context.SaveChangesAsync();
            return category; // category object now has the DB-generated ID
        }
        // --------------

        // --- UpdateAsync ---
        public async Task<BlogCategory> UpdateAsync(BlogCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Optional: Ensure Slug is set/updated and formatted
            // category.Slug = GenerateSlug(category.Slug ?? category.Name); // Example

            context.BlogCategories.Update(category); // Mark entire entity as modified (or attach and set state)

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error updating category: {ex.Message}");
                // Consider logging this properly instead of just Console.WriteLine
                throw; // Rethrow or handle appropriately
            }
            return category;
        }
        // -----------------

        // --- DeleteAsync (with Usage Check) ---
        public async Task<bool> DeleteAsync(int categoryId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Check if the category is associated with any blog posts
            bool isInUse = await context.BlogPostCategories
                                        .AnyAsync(bpc => bpc.BlogCategoryId == categoryId);

            if (isInUse)
            {
                // Category is linked, prevent deletion.
                Console.WriteLine($"Attempted to delete category ID {categoryId} which is currently in use.");
                return false; // Indicate deletion failed due to usage
            }

            // 2. If not in use, proceed to find and delete the category
            var categoryToDelete = await context.BlogCategories.FindAsync(categoryId);

            if (categoryToDelete == null)
            {
                return false; // Category not found
            }

            context.BlogCategories.Remove(categoryToDelete);
            int changes = await context.SaveChangesAsync();
            return changes > 0; // Return true if deletion was successful
        }
        // --- END UPDATED DeleteAsync ---

        // --- GetBySlugAsync ---
        public async Task<BlogCategory?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogCategories
                                 .FirstOrDefaultAsync(c => c.Slug == slug);
        }
        // --------------------
    }
}