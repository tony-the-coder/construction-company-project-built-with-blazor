using LehmanCustomConstruction.Data.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Interfaces
{
    public interface IBlogCategoryRepository
    {
        Task<IEnumerable<BlogCategory>> GetAllAsync();
        Task<BlogCategory?> GetByIdAsync(int categoryId); 
        Task<BlogCategory> AddAsync(BlogCategory category); 
        Task<BlogCategory> UpdateAsync(BlogCategory category); 
        Task<bool> DeleteAsync(int categoryId);
        Task<BlogCategory?> GetBySlugAsync(string slug);

        // Task<bool> SlugExistsAsync(int currentCategoryId, string slug);
    }
}