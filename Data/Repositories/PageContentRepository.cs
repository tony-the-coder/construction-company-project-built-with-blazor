// File: Data/Repositories/PageContentRepository.cs
using LehmanCustomConstruction.Data.Common; // Your models namespace
using LehmanCustomConstruction.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider, CreateScope, GetRequiredService
using System;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Repositories
{
    public class PageContentRepository : IPageContentRepository
    {
        // --- Inject IServiceProvider ---
        private readonly IServiceProvider _serviceProvider;

        public PageContentRepository(IServiceProvider serviceProvider) // Updated constructor
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        // --- End IServiceProvider Injection ---

        public async Task<string?> GetContentAsync(string pageKey)
        {
            // Create a scope for this operation
            using var scope = _serviceProvider.CreateScope();
            // Resolve the DbContext from the scope
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var content = await context.PageContents
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(p => p.PageKey == pageKey);
            return content?.HtmlContent;
            // Context is disposed automatically when 'scope' is disposed
        }

        public async Task<PageContent?> GetPageContentAsync(string pageKey)
        {
            // Create a scope for this operation
            using var scope = _serviceProvider.CreateScope();
            // Resolve the DbContext from the scope
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.PageContents
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.PageKey == pageKey);
            // Context is disposed automatically when 'scope' is disposed
        }

        public async Task SaveContentAsync(string pageKey, string htmlContent)
        {
            // Create a scope for this operation
            using var scope = _serviceProvider.CreateScope();
            // Resolve the DbContext from the scope
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingContent = await context.PageContents.FirstOrDefaultAsync(p => p.PageKey == pageKey); // Use FirstOrDefaultAsync for potentially null

            if (existingContent != null)
            {
                existingContent.HtmlContent = htmlContent;
                existingContent.DateModified = DateTime.UtcNow;
                // No need for context.PageContents.Update(existingContent); if the entity is tracked
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
            // Context is disposed automatically when 'scope' is disposed
        }
    }
}