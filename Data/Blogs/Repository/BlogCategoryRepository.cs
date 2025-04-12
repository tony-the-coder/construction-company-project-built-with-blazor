using LehmanCustomConstruction.Data; // Needed for ApplicationDbContext
using LehmanCustomConstruction.Data.Blogs; // Needed for BlogCategory
using LehmanCustomConstruction.Data.Blogs.Interfaces; // Needed for IBlogCategoryRepository
using Microsoft.EntityFrameworkCore; // Needed for EF Core methods like ToListAsync, OrderBy
using System.Collections.Generic;
using System.Linq; // Needed for OrderBy
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Repository
{
    public class BlogCategoryRepository : IBlogCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Inject the ApplicationDbContext
        public BlogCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Implement the GetAllAsync method defined in the interface
        public async Task<IEnumerable<BlogCategory>> GetAllAsync()
        {
            // Fetch all categories from the BlogCategories table
            // Order them by Name alphabetically for a user-friendly dropdown
            return await _context.BlogCategories
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();

            // Note: We don't typically need to .Include(c => c.BlogPostCategories) here,
            // as we usually just need the category names and IDs for the dropdown.
        }

        // Add other methods like AddAsync, GetByIdAsync, UpdateAsync, DeleteAsync here
        // later if you build a separate admin section to manage categories themselves.
    }
}