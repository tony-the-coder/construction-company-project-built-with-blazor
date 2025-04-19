// File Path: Data/Blogs/Repository/BlogPostRepository.cs
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
    public class BlogPostRepository : IBlogPostRepository
    {
        // --- Inject IServiceProvider ---
        private readonly IServiceProvider _serviceProvider;

        public BlogPostRepository(IServiceProvider serviceProvider) // Updated constructor
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        // --- End IServiceProvider Injection ---

        // --- AddAsync ---
        public async Task<BlogPost> AddAsync(BlogPost entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            entity.BlogPostCategories ??= new List<BlogPostCategory>(); // Ensure collection exists
            await context.BlogPosts.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        // --------------

        // --- DeleteAsync ---
        public async Task<bool> DeleteAsync(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var obj = await context.BlogPosts.FirstOrDefaultAsync(x => x.ID == id);
            if (obj != null)
            {
                context.BlogPosts.Remove(obj); // Deletion cascades should handle BlogPostCategories
                return (await context.SaveChangesAsync()) > 0;
            }
            return false;
        }
        // ---------------

        // --- GetAllAsync ---
        public async Task<IEnumerable<BlogPost>> GetAllAsync(bool publishedOnly = false)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var query = context.BlogPosts
                               .Include(p => p.BlogPostCategories)
                               .ThenInclude(bpc => bpc.BlogCategory)
                               .AsQueryable();

            if (publishedOnly)
            {
                query = query.Where(p => p.IsPublished);
            }

            return await query.OrderByDescending(p => p.PublishDate)
                              .ToListAsync();
        }
        // --- END MODIFICATION ---

        // --- GetByIdAsync (For Admin Editing - includes drafts) ---
        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogPosts
                                 .Include(b => b.BlogPostCategories)
                                 .ThenInclude(bpc => bpc.BlogCategory)
                                 .FirstOrDefaultAsync(b => b.ID == id);
        }
        // -----------------------------------------------------------

        // --- GetBySlugAsync (Generic - might include drafts) ---
        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogPosts
                                 .Include(b => b.BlogPostCategories)
                                 .ThenInclude(bc => bc.BlogCategory)
                                 .FirstOrDefaultAsync(x => x.Slug == slug);
        }
        // -------------------------------------------------------

        // --- GetPublishedPostBySlugAsync (For Public View) ---
        public async Task<BlogPost?> GetPublishedPostBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogPosts
                                  .Where(p => p.Slug == slug && p.IsPublished)
                                  .Include(p => p.BlogPostCategories)
                                  .ThenInclude(bpc => bpc.BlogCategory)
                                  .FirstOrDefaultAsync();
        }
        // --- END ADDED METHOD ---

        // --- GetPublishedPostsByCategorySlugAsync ---
        public async Task<IEnumerable<BlogPost>> GetPublishedPostsByCategorySlugAsync(string categorySlug)
        {
            if (string.IsNullOrWhiteSpace(categorySlug))
            {
                return Enumerable.Empty<BlogPost>();
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogPosts
                                 .Where(p => p.IsPublished && p.BlogPostCategories.Any(bpc => bpc.BlogCategory.Slug == categorySlug))
                                 .Include(p => p.BlogPostCategories)
                                 .ThenInclude(bpc => bpc.BlogCategory)
                                 .OrderByDescending(p => p.PublishDate)
                                 .ToListAsync();
        }
        // --- END ADDED MISSING IMPLEMENTATION ---

        // --- SlugExistsAsync ---
        public async Task<bool> SlugExistsAsync(int currentPostId, string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.BlogPosts
                                .Where(p => p.Slug == slug && p.ID != currentPostId)
                                .AnyAsync();
        }
        // ---------------------

        // --- UpdateAsync ---
        public async Task<BlogPost> UpdateAsync(BlogPost entity, IEnumerable<int> selectedCategoryIds)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            selectedCategoryIds ??= Enumerable.Empty<int>();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // IMPORTANT: Fetch the entity from the context *within this scope* before updating
            // to ensure it's tracked by this context instance. Or Attach if you know it's detached.
            // Let's assume the passed 'entity' might be detached or from another context.
            // A safer approach is to fetch it first.

            var postToUpdate = await context.BlogPosts
                                            .Include(p => p.BlogPostCategories) // Include existing categories
                                            .FirstOrDefaultAsync(p => p.ID == entity.ID);

            if (postToUpdate == null)
            {
                throw new InvalidOperationException($"Blog post with ID {entity.ID} not found.");
            }

            // Update scalar properties (Title, Content, Slug, etc.)
            // Use the values from the passed-in 'entity' object
            context.Entry(postToUpdate).CurrentValues.SetValues(entity); // Copies scalar properties

            // Now handle categories based on the fetched 'postToUpdate'
            var existingCategoryIds = postToUpdate.BlogPostCategories.Select(bc => bc.BlogCategoryId).ToList();

            // 3. Determine which categories to add.
            var categoryIdsToAdd = selectedCategoryIds.Except(existingCategoryIds).ToList();
            foreach (var categoryId in categoryIdsToAdd)
            {
                postToUpdate.BlogPostCategories.Add(new BlogPostCategory { BlogCategoryId = categoryId });
                // No need to set BlogPostId, EF Core handles it based on navigation property
            }

            // 4. Determine which categories to remove.
            var categoryIdsToRemove = existingCategoryIds.Except(selectedCategoryIds).ToList();
            foreach (var categoryIdToRemove in categoryIdsToRemove)
            {
                var blogPostCategoryToRemove = postToUpdate.BlogPostCategories
                                                            .FirstOrDefault(bpc => bpc.BlogCategoryId == categoryIdToRemove);
                if (blogPostCategoryToRemove != null)
                {
                    postToUpdate.BlogPostCategories.Remove(blogPostCategoryToRemove); // Remove from navigation property
                    // context.BlogPostCategories.Remove(blogPostCategoryToRemove); // Alternative if removing directly
                }
            }

            // 5. Save changes.
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error updating blog post: {ex.Message}");
                // Handle concurrency if needed
                throw;
            }

            // Return the updated tracked entity (or the original reference, depending on needs)
            return postToUpdate;
        }
        // -----------------
    }
}