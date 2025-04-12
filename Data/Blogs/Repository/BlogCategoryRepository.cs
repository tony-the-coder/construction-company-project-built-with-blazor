// File Path: Data/Blogs/Repository/BlogCategoryRepository.cs
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Blogs;
using LehmanCustomConstruction.Data.Blogs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System; // Required for ArgumentNullException
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Repository
{
    public class BlogCategoryRepository : IBlogCategoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BlogCategoryRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // --- GetAllAsync ---
        public async Task<IEnumerable<BlogCategory>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogCategories
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }
        // -------------------

        // --- GetByIdAsync ---
        public async Task<BlogCategory?> GetByIdAsync(int categoryId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
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

            await using var context = await _contextFactory.CreateDbContextAsync();
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

            await using var context = await _contextFactory.CreateDbContextAsync();

            // Optional: Ensure Slug is set/updated and formatted
            // category.Slug = GenerateSlug(category.Slug ?? category.Name); // Example

            context.Entry(category).State = EntityState.Modified; // Mark entire entity as modified

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error updating category: {ex.Message}");
                throw; // Rethrow for now
            }
            return category;
        }
        // -----------------

        // --- UPDATED DeleteAsync with Usage Check ---
        public async Task<bool> DeleteAsync(int categoryId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Check if the category is associated with any blog posts
            bool isInUse = await context.BlogPostCategories
                                        .AnyAsync(bpc => bpc.BlogCategoryId == categoryId);

            if (isInUse)
            {
                // Category is linked, prevent deletion.
                Console.WriteLine($"Attempted to delete category ID {categoryId} which is currently in use.");
                // Returning false will trigger the "Deletion Failed" notification in ManageBlogCategories.razor
                return false;
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

        // Optional: Implement SlugExistsAsync if needed for categories
        // public async Task<bool> SlugExistsAsync(int currentCategoryId, string slug) { ... }

        // Optional: Slug generation helper (could be moved to a shared utility class)
        // private string GenerateSlug(string? phrase) { ... logic ... }
    }
}