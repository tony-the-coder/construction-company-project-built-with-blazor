using LehmanCustomConstruction.Data.Blogs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LehmanCustomConstruction.Data.Blogs.Repository
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor to inject the ApplicationDbContext
        public BlogPostRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<BlogPost> AddAsync(BlogPost entity)
        {
            await _context.BlogPosts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;

        }

        //I learned that this should be public async Task<BlogPost> DeleteAsync(int id)
        //so I need to check since I learned it should be bool
        //Regardless, I need to review this again, unfortunatley
        public async Task<bool> DeleteAsync(int id)
        {
            var obj = await _context.BlogPosts.FirstOrDefaultAsync(x => x.ID == id);
            if (obj != null)
            {
                _context.BlogPosts.Remove(obj);
                return (await _context.SaveChangesAsync()) > 0;
            }
            return false;
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            return await _context.BlogPosts.ToListAsync();
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _context.BlogPosts.FindAsync(id);
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            return await _context.BlogPosts
                .FirstOrDefaultAsync(x => x.Slug == slug);
        }

        public async Task<BlogPost> UpdateAsync(BlogPost entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

      
    }
}
