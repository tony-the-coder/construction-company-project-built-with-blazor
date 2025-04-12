// File Path: Data/Blogs/Interfaces/IBlogPostRepository.cs
using LehmanCustomConstruction.Data.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Interfaces
{
    public interface IBlogPostRepository
    {
        Task<BlogPost> AddAsync(BlogPost entity);
        Task<BlogPost> UpdateAsync(BlogPost entity, IEnumerable<int> selectedCategoryIds);
        Task<bool> DeleteAsync(int id);
        Task<BlogPost?> GetByIdAsync(int id);
        Task<BlogPost?> GetBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetAllAsync();
        Task<bool> SlugExistsAsync(int currentPostId, string slug); // <-- ADDED
    }
}