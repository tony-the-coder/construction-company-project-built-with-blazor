// File: Data/Repositories/PageContentRepository.cs
using LehmanCustomConstruction.Data.Common;
using LehmanCustomConstruction.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System; // Required for DateTime
using System.Threading.Tasks; // Required for Task

namespace LehmanCustomConstruction.Data.Repositories
{
    public class PageContentRepository : IPageContentRepository
    {
        // --- Inject the FACTORY ---
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PageContentRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        // --- End Factory Injection ---

        public async Task<string?> GetContentAsync(string pageKey)
        {
            // Create context instance within method scope
            using var context = _contextFactory.CreateDbContext();
            var content = await context.PageContents
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(p => p.PageKey == pageKey);
            return content?.HtmlContent;
        }

        public async Task<PageContent?> GetPageContentAsync(string pageKey)
        {
            // Create context instance within method scope
            using var context = _contextFactory.CreateDbContext();
            return await context.PageContents
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.PageKey == pageKey);
        }

        public async Task SaveContentAsync(string pageKey, string htmlContent)
        {
            // Create context instance within method scope
            using var context = _contextFactory.CreateDbContext();
            var existingContent = await context.PageContents.FindAsync(pageKey);

            if (existingContent != null)
            {
                existingContent.HtmlContent = htmlContent;
                existingContent.DateModified = DateTime.UtcNow;
                context.PageContents.Update(existingContent);
            }
            else
            {
                var newContent = new PageContent
                {
                    PageKey = pageKey,
                    HtmlContent = htmlContent,
                    DateModified = DateTime.UtcNow
                };
                await context.PageContents.AddAsync(newContent);
            }

            await context.SaveChangesAsync();
        }
    }
}