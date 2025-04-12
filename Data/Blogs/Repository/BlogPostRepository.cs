using LehmanCustomConstruction.Data; // Ensure this is present for ApplicationDbContext
using LehmanCustomConstruction.Data.Blogs; // Ensure this is present for BlogPost
using LehmanCustomConstruction.Data.Blogs.Interfaces;
using Microsoft.EntityFrameworkCore; // Required for .Include() and other EF methods
using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for OrderBy, Where etc. (though not used in GetByIdAsync change)
using System.Threading.Tasks; // Required for async Task

namespace LehmanCustomConstruction.Data.Blogs.Repository
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogPostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BlogPost> AddAsync(BlogPost entity)
        {
            // EF Core automatically handles adding related BlogPostCategory entities
            // if they are present in the entity.BlogPostCategories collection.
            await _context.BlogPosts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var obj = await _context.BlogPosts.FirstOrDefaultAsync(x => x.ID == id);
            if (obj != null)
            {
                _context.BlogPosts.Remove(obj); // Deletion cascades to BlogPostCategories if configured
                return (await _context.SaveChangesAsync()) > 0;
            }
            return false;
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            // Consider adding .Include(b => b.BlogPostCategories).ThenInclude(bc => bc.BlogCategory)
            // here if you need to display categories on a blog post list page.
            return await _context.BlogPosts.ToListAsync();
        }

        // --- MODIFIED GetByIdAsync ---
        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            // Changed from FindAsync to FirstOrDefaultAsync to allow Include.
            // Added .Include() to eagerly load the BlogPostCategories navigation property.
            return await _context.BlogPosts
                                 .Include(b => b.BlogPostCategories) // Load the join table entries
                                                                     // Optional: If you need the actual Category Name here too:
                                                                     // .ThenInclude(bc => bc.BlogCategory)
                                 .FirstOrDefaultAsync(b => b.ID == id); // Find by primary key
        }
        // --- END MODIFICATION ---

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            // Consider adding .Include() here if you need categories on the single post display page.
            return await _context.BlogPosts
                                 // .Include(b => b.BlogPostCategories)
                                 // .ThenInclude(bc => bc.BlogCategory)
                                 .FirstOrDefaultAsync(x => x.Slug == slug);
        }

        public async Task<BlogPost> UpdateAsync(BlogPost entity)
        {
            // EF Core's change tracker handles updates to the main entity
            // AND changes made to the BlogPostCategories collection (adds/removes)
            // because the entity was loaded with .Include() in GetByIdAsync.
            _context.Entry(entity).State = EntityState.Modified; // Marks the main entity as modified

            // You could explicitly mark BlogPostCategories as modified too, but often not needed
            // if the collection itself was modified (items added/removed).
            // Let EF Core handle the tracked changes.

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency issues - e.g., reload or notify user
                Console.WriteLine($"Concurrency error updating blog post: {ex.Message}");
                throw; // Rethrow for now, or handle more gracefully
            }
            return entity;
        }
    }
}