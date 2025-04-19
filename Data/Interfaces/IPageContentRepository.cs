// File: Data/Interfaces/IPageContentRepository.cs
using LehmanCustomConstruction.Data.Common; // Use the correct namespace for PageContent

namespace LehmanCustomConstruction.Data.Interfaces // Adjust namespace if needed
{
    public interface IPageContentRepository
    {
        Task<string?> GetContentAsync(string pageKey);
        Task<PageContent?> GetPageContentAsync(string pageKey); // Optional: Get the whole object
        Task SaveContentAsync(string pageKey, string htmlContent);
        // Add other methods if needed (e.g., GetAllKeysAsync)
    }
}