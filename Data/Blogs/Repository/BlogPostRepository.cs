// File Path: Data/Blogs/Repository/BlogPostRepository.cs
using LehmanCustomConstruction.Data; // Required for ApplicationDbContext
using LehmanCustomConstruction.Data.Blogs; // Required for BlogPost, BlogCategory etc.
using LehmanCustomConstruction.Data.Blogs.Interfaces; // Required for IBlogPostRepository
using Microsoft.EntityFrameworkCore; // Required for EF Core methods, DbContextFactory
using System; // Required for ArgumentNullException
using System.Collections.Generic; // Required for IEnumerable, List
using System.Linq; // Required for LINQ methods like Where, OrderByDescending etc.
using System.Threading.Tasks; // Required for async Task

namespace LehmanCustomConstruction.Data.Blogs.Repository
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BlogPostRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // --- AddAsync ---
        public async Task<BlogPost> AddAsync(BlogPost entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await using var context = await _contextFactory.CreateDbContextAsync();
            entity.BlogPostCategories ??= new List<BlogPostCategory>(); // Ensure collection exists
            await context.BlogPosts.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        // --------------

        // --- DeleteAsync ---
        public async Task<bool> DeleteAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var obj = await context.BlogPosts.FirstOrDefaultAsync(x => x.ID == id);
            if (obj != null)
            {
                context.BlogPosts.Remove(obj); // Deletion cascades should handle BlogPostCategories
                return (await context.SaveChangesAsync()) > 0;
            }
            return false;
        }
        // ---------------

        // --- MODIFIED GetAllAsync ---
        public async Task<IEnumerable<BlogPost>> GetAllAsync(bool publishedOnly = false) // Added parameter
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.BlogPosts
                               .Include(p => p.BlogPostCategories) // Include categories
                               .ThenInclude(bpc => bpc.BlogCategory) // Include category details
                               .AsQueryable();

            if (publishedOnly)
            {
                query = query.Where(p => p.IsPublished); // Apply filter if requested
            }

            // Always order, most recent first
            return await query.OrderByDescending(p => p.PublishDate)
                              .ToListAsync();
        }
        // --- END MODIFICATION ---

        // --- GetByIdAsync (For Admin Editing - includes drafts) ---
        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogPosts
                                 .Include(b => b.BlogPostCategories) // Load the join table entries
                                 .ThenInclude(bpc => bpc.BlogCategory) // Load actual category details
                                 .FirstOrDefaultAsync(b => b.ID == id);
        }
        // -----------------------------------------------------------

        // --- GetBySlugAsync (Generic - might include drafts) ---
        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogPosts
                                 .Include(b => b.BlogPostCategories) // Example include
                                 .ThenInclude(bc => bc.BlogCategory)  // Example include
                                 .FirstOrDefaultAsync(x => x.Slug == slug);
        }
        // -------------------------------------------------------

        // --- ADDED GetPublishedPostBySlugAsync (For Public View) ---
        public async Task<BlogPost?> GetPublishedPostBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Include categories and ensure it's published
            return await context.BlogPosts
                                  .Where(p => p.Slug == slug && p.IsPublished) // Filter by slug AND published status
                                  .Include(p => p.BlogPostCategories)
                                  .ThenInclude(bpc => bpc.BlogCategory)
                                  .FirstOrDefaultAsync();
        }
        // --- END ADDED METHOD ---

        // <<< --- START: ADDED MISSING IMPLEMENTATION --- >>>
        // --- GetPublishedPostsByCategorySlugAsync ---
        public async Task<IEnumerable<BlogPost>> GetPublishedPostsByCategorySlugAsync(string categorySlug)
        {
            if (string.IsNullOrWhiteSpace(categorySlug))
            {
                return Enumerable.Empty<BlogPost>(); // Return empty if no slug provided
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.BlogPosts
                                 .Where(p => p.IsPublished && p.BlogPostCategories.Any(bpc => bpc.BlogCategory.Slug == categorySlug)) // Filter by published AND category slug
                                 .Include(p => p.BlogPostCategories) // Include needed data for cards
                                 .ThenInclude(bpc => bpc.BlogCategory)
                                 .OrderByDescending(p => p.PublishDate) // Order results
                                 .ToListAsync();
        }
        // --- END ADDED MISSING IMPLEMENTATION ---


        // --- SlugExistsAsync ---
        public async Task<bool> SlugExistsAsync(int currentPostId, string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;

            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogPosts
                                .Where(p => p.Slug == slug && p.ID != currentPostId)
                                // Future: .Where(p => !p.IsDeleted)
                                .AnyAsync();
        }
        // ---------------------

        // --- UpdateAsync ---
        public async Task<BlogPost> UpdateAsync(BlogPost entity, IEnumerable<int> selectedCategoryIds)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            selectedCategoryIds ??= Enumerable.Empty<int>(); // Ensure not null

            await using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Attach the main entity and mark it as modified.
            var attachedEntity = context.BlogPosts.Attach(entity);
            attachedEntity.State = EntityState.Modified;

            // 2. Get the current category IDs associated with this post *from the database*.
            var existingCategoryIds = await context.BlogPostCategories
                .Where(bc => bc.BlogPostId == entity.ID)
                .Select(bc => bc.BlogCategoryId)
                .ToListAsync();

            // 3. Determine which categories to add.
            var categoryIdsToAdd = selectedCategoryIds.Except(existingCategoryIds).ToList();
            foreach (var categoryId in categoryIdsToAdd)
            {
                context.BlogPostCategories.Add(new BlogPostCategory { BlogPostId = entity.ID, BlogCategoryId = categoryId });
            }

            // 4. Determine which categories to remove.
            var categoryIdsToRemove = existingCategoryIds.Except(selectedCategoryIds).ToList();
            if (categoryIdsToRemove.Any())
            {
                var categoriesToRemove = await context.BlogPostCategories
                    .Where(bc => bc.BlogPostId == entity.ID && categoryIdsToRemove.Contains(bc.BlogCategoryId))
                    .ToListAsync();
                context.BlogPostCategories.RemoveRange(categoriesToRemove);
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

            return entity; // Return the original entity reference
        }
        // -----------------
    }
}