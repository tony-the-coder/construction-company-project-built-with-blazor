// File: Data/Repositories/PageContentRepository.cs
using LehmanCustomConstruction.Data.Common;
using LehmanCustomConstruction.Data.Interfaces;
using Microsoft.EntityFrameworkCore; // Required for DbContext and async methods

namespace LehmanCustomConstruction.Data.Repositories // Adjust namespace if needed
{
    public class PageContentRepository : IPageContentRepository
    {
        private readonly ApplicationDbContext _context;

        // Inject the DbContext
        public PageContentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetContentAsync(string pageKey)
        {
            var content = await _context.PageContents
                                        .AsNoTracking() // Good practice for read-only data
                                        .FirstOrDefaultAsync(p => p.PageKey == pageKey);
            return content?.HtmlContent;
        }

        public async Task<PageContent?> GetPageContentAsync(string pageKey)
        {
            return await _context.PageContents
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(p => p.PageKey == pageKey);
        }

        public async Task SaveContentAsync(string pageKey, string htmlContent)
        {
            var existingContent = await _context.PageContents.FindAsync(pageKey);

            if (existingContent != null)
            {
                // Update existing content
                existingContent.HtmlContent = htmlContent;
                existingContent.DateModified = DateTime.UtcNow;
                _context.PageContents.Update(existingContent);
            }
            else
            {
                // Create new content
                var newContent = new PageContent
                {
                    PageKey = pageKey,
                    HtmlContent = htmlContent,
                    DateModified = DateTime.UtcNow
                };
                await _context.PageContents.AddAsync(newContent);
            }

            await _context.SaveChangesAsync(); // Save changes to the database
        }
    }
}