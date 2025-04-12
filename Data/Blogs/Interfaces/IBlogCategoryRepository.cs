// In Data/Blogs/Interfaces/IBlogCategoryRepository.cs
using LehmanCustomConstruction.Data.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Interfaces
{
    public interface IBlogCategoryRepository
    {
        Task<IEnumerable<BlogCategory>> GetAllAsync();
        // Add other methods later if needed (GetById, Add, Update, Delete)
        // For the dropdown, GetAllAsync is the primary requirement.
    }
}