// File Path: Data/Blogs/Repository/BlogPostRepository.cs
using LehmanCustomConstruction.Data.Blogs.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        // --- GetAllAsync ---
        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Include Categories/Author etc. here if needed for the list view
            return await context.BlogPosts
                                .OrderByDescending(p => p.PublishDate) // Example ordering
                                .ToListAsync();
        }
        // ---------------

        // --- GetByIdAsync ---
        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BlogPosts
                                 .Include(b => b.BlogPostCategories) // Load the join table entries
                                                                     // Optional: .ThenInclude(bc => bc.BlogCategory) // Load actual category details
                                 .FirstOrDefaultAsync(b => b.ID == id);
        }
        // ----------------

        // --- GetBySlugAsync ---
        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Include Categories/Author etc. here if needed for the single post view
            return await context.BlogPosts
                                 .Include(b => b.BlogPostCategories) // Example include
                                 .ThenInclude(bc => bc.BlogCategory)  // Example include
                                 .FirstOrDefaultAsync(x => x.Slug == slug);
        }
        // ------------------

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