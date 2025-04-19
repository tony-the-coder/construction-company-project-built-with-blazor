// File Path: Data/Blogs/Interfaces/IBlogPostRepository.cs
using LehmanCustomConstruction.Data.Blogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Blogs.Interfaces
{
    /// <summary>
    /// Interface defining data operations for BlogPost entities.
    /// </summary>
    public interface IBlogPostRepository
    {
        /// <summary>
        /// Adds a new blog post to the database.
        /// </summary>
        /// <param name="entity">The BlogPost object to add.</param>
        /// <returns>The added BlogPost object (with potential DB-generated ID).</returns>
        Task<BlogPost> AddAsync(BlogPost entity);

        /// <summary>
        /// Updates an existing blog post and its category associations.
        /// </summary>
        /// <param name="entity">The BlogPost object with updated scalar properties.</param>
        /// <param name="selectedCategoryIds">The list of Category IDs this post should be associated with.</param>
        /// <returns>The updated BlogPost object.</returns>
        Task<BlogPost> UpdateAsync(BlogPost entity, IEnumerable<int> selectedCategoryIds);

        /// <summary>
        /// Deletes a blog post by its ID.
        /// </summary>
        /// <param name="id">The ID of the post to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets a blog post by its ID (intended for admin use, may include drafts).
        /// Includes associated BlogPostCategories.
        /// </summary>
        /// <param name="id">The ID of the post to retrieve.</param>
        /// <returns>The found BlogPost or null.</returns>
        Task<BlogPost?> GetByIdAsync(int id);

        /// <summary>
        /// Gets a blog post by its slug (may include drafts).
        /// Includes associated BlogPostCategories and BlogCategory details.
        /// </summary>
        /// <param name="slug">The slug of the post to retrieve.</param>
        /// <returns>The found BlogPost or null.</returns>
        Task<BlogPost?> GetBySlugAsync(string slug);

        /// <summary>
        /// Gets all blog posts, optionally filtering for published posts only.
        /// Includes associated BlogPostCategories and BlogCategory details. Ordered by PublishDate descending.
        /// </summary>
        /// <param name="publishedOnly">If true, returns only posts where IsPublished is true.</param>
        /// <returns>An enumerable collection of BlogPost objects.</returns>
        Task<IEnumerable<BlogPost>> GetAllAsync(bool publishedOnly = false);

        /// <summary>
        /// Checks if a given slug already exists for a different blog post.
        /// </summary>
        /// <param name="currentPostId">The ID of the post currently being edited (0 if new).</param>
        /// <param name="slug">The slug to check.</param>
        /// <returns>True if the slug exists for another post, false otherwise.</returns>
        Task<bool> SlugExistsAsync(int currentPostId, string slug);

        /// <summary>
        /// Gets a single *published* blog post by its slug (for public viewing).
        /// Includes associated BlogPostCategories and BlogCategory details.
        /// </summary>
        /// <param name="slug">The slug of the published post to retrieve.</param>
        /// <returns>The found published BlogPost or null.</returns>
        Task<BlogPost?> GetPublishedPostBySlugAsync(string slug);

        /// <summary>
        /// Gets all *published* blog posts associated with a specific category slug.
        /// Includes associated BlogPostCategories and BlogCategory details. Ordered by PublishDate descending.
        /// </summary>
        /// <param name="categorySlug">The slug of the category to filter by.</param>
        /// <returns>An enumerable collection of matching published BlogPost objects.</returns>
        Task<IEnumerable<BlogPost>> GetPublishedPostsByCategorySlugAsync(string categorySlug);
    }
}