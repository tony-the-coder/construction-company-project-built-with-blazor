using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LehmanCustomConstruction.Components;
using LehmanCustomConstruction.Components.Account;
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Blogs.Interfaces;       // Ensure Blog interfaces namespace is correct
using LehmanCustomConstruction.Data.Blogs.Repository;   // Ensure Blog repository namespace is correct
using LehmanCustomConstruction.Data.Interfaces;           // << ADDED: Namespace for IPageContentRepository
using LehmanCustomConstruction.Data.Repositories;         // << ADDED: Namespace for PageContentRepository
// using LehmanCustomConstruction.Services;                  // Add when IntuitService is needed
using Radzen;                                           // Namespace for AddRadzenComponents
// using Radzen.Blazor; // Often not needed directly here, covered by AddRadzenComponents

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- Repositories ---
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>(); // << ADDED THIS LINE
// Add Portfolio repositories later

// --- Authentication & Identity ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// --- DbContext Configuration ---
// Using AddDbContextFactory is often preferred for Blazor Server to manage DbContext lifetime correctly
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// AddDatabaseDeveloperPageExceptionFilter is good for development
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- Identity Core Configuration ---
// This setup requires AddEntityFrameworkStores, which uses the underlying DbContext configuration
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// --- Radzen Services ---
builder.Services.AddRadzenComponents(); // Includes DialogService, NotificationService, etc.

// --- HttpClient for Intuit API (Uncomment and configure when needed) ---
// builder.Services.AddHttpClient("IntuitAPI", client =>
// {
//     client.BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com/");
// });

// --- Custom Services (Uncomment and configure when needed) ---
// builder.Services.AddScoped<IntuitService>(); // Example registration for your service


// ======================================================================
var app = builder.Build();
// ======================================================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Serve static files from wwwroot
app.UseAntiforgery();

// --- Endpoint Mapping ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // Or other render modes

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Add custom endpoints (like file upload controllers) here later if needed
// Example: app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// --- ADD TEMPORARY PLACEHOLDER INTERFACE/CLASS DEFINITIONS HERE ---
// --- (REMOVE THESE ONCE MOVED TO SEPARATE FILES) ---
// --- NECESSARY IF THE REAL FILES DON'T EXIST YET FOR COMPILATION ---

// public interface IPageContentRepository { Task<string?> GetContentAsync(string pageKey); Task SaveContentAsync(string pageKey, string htmlContent); }
// public class PageContentRepository : IPageContentRepository { private readonly ApplicationDbContext _context; public PageContentRepository(ApplicationDbContext context) { _context = context; } public async Task<string?> GetContentAsync(string pageKey) { var content = await _context.PageContents.AsNoTracking().FirstOrDefaultAsync(p => p.PageKey == pageKey); return content?.HtmlContent; } public Task SaveContentAsync(string pageKey, string htmlContent) { throw new NotImplementedException(); /* Implement real save later */ } public Task<LehmanCustomConstruction.Data.Common.PageContent?> GetPageContentAsync(string pageKey) => throw new NotImplementedException(); }
// public interface IBlogPostRepository { Task<IEnumerable<BlogPost>> GetAllAsync(bool publishedOnly = false); Task<BlogPost?> GetByIdAsync(int id); Task<BlogPost?> GetPublishedPostBySlugAsync(string slug); Task<IEnumerable<BlogPost>> GetPublishedPostsByCategorySlugAsync(string categorySlug); Task<bool> SlugExistsAsync(int postIdToExclude, string slug); Task<BlogPost> AddAsync(BlogPost blogPost); Task<BlogPost> UpdateAsync(BlogPost blogPost, IEnumerable<int> selectedCategoryIds); Task<bool> DeleteAsync(int id); }
// public class BlogPostRepository : IBlogPostRepository { public Task<IEnumerable<BlogPost>> GetAllAsync(bool publishedOnly = false) => throw new NotImplementedException(); public Task<BlogPost?> GetByIdAsync(int id) => throw new NotImplementedException(); public Task<BlogPost?> GetPublishedPostBySlugAsync(string slug) => throw new NotImplementedException(); public Task<IEnumerable<BlogPost>> GetPublishedPostsByCategorySlugAsync(string categorySlug) => throw new NotImplementedException(); public Task<bool> SlugExistsAsync(int postIdToExclude, string slug) => throw new NotImplementedException(); public Task<BlogPost> AddAsync(BlogPost blogPost) => throw new NotImplementedException(); public Task<BlogPost> UpdateAsync(BlogPost blogPost, IEnumerable<int> selectedCategoryIds) => throw new NotImplementedException(); public Task<bool> DeleteAsync(int id) => throw new NotImplementedException(); }
// public interface IBlogCategoryRepository { Task<IEnumerable<BlogCategory>> GetAllAsync(); Task<BlogCategory?> GetByIdAsync(int id); Task<BlogCategory?> GetBySlugAsync(string slug); Task<BlogCategory> AddAsync(BlogCategory category); Task<BlogCategory> UpdateAsync(BlogCategory category); Task<bool> DeleteAsync(int id); }
// public class BlogCategoryRepository : IBlogCategoryRepository { public Task<IEnumerable<BlogCategory>> GetAllAsync() => throw new NotImplementedException(); public Task<BlogCategory?> GetByIdAsync(int id) => throw new NotImplementedException(); public Task<BlogCategory?> GetBySlugAsync(string slug) => throw new NotImplementedException(); public Task<BlogCategory> AddAsync(BlogCategory category) => throw new NotImplementedException(); public Task<BlogCategory> UpdateAsync(BlogCategory category) => throw new NotImplementedException(); public Task<bool> DeleteAsync(int id) => throw new NotImplementedException(); }
// --- END TEMPORARY PLACEHOLDERS ---