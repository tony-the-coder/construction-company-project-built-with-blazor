using LehmanCustomConstruction.Data.Blogs;


namespace LehmanCustomConstruction.Data.Blogs.Interfaces
{
    
        public interface IBlogPostRepository
        {
        Task<BlogPost> AddAsync(BlogPost entity);
        Task<BlogPost> UpdateAsync(BlogPost entity);
        Task<bool> DeleteAsync(int id);
        Task<BlogPost?> GetByIdAsync(int id);
        Task<BlogPost?> GetBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetAllAsync();
       
    }
    }

