// File: Program.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LehmanCustomConstruction.Components;
using LehmanCustomConstruction.Components.Account;
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Blogs.Interfaces;
using LehmanCustomConstruction.Data.Blogs.Repository;
using LehmanCustomConstruction.Data.Interfaces;
using LehmanCustomConstruction.Data.Repositories;
using LehmanCustomConstruction.Data.Common;
using LehmanCustomConstruction.Services.Interfaces;
using LehmanCustomConstruction.Services;
using Radzen;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---
// NO AddControllers() here
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => { options.DetailedErrors = true; });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>(); // Standard provider
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Repositories
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();

// Custom Services
builder.Services.AddScoped<IEmailService, EmailService>();

// Authentication & Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity Core Configuration
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Radzen Services
builder.Services.AddRadzenComponents();

// --- END Service Registration ---

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment()) { app.UseMigrationsEndPoint(); app.UseDeveloperExceptionPage(); }
else { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// --- Endpoint Mapping ---
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();
// NO MapControllers() here

// File Download Minimal API Endpoint (Auth Disabled for Demo)
app.MapGet("/download/{id:int}", async (int id, ApplicationDbContext dbContext, IConfiguration configuration, ILogger<Program> logger) =>
{
    logger.LogWarning("DEMO MODE: Download endpoint accessed anonymously for document ID {DocumentId}. RE-ENABLE AUTH!", id);
    var document = await dbContext.CustomerDocuments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    if (document == null) { return Results.NotFound($"Document with ID {id} not found or marked as deleted."); }
    var basePath = configuration["FileUploadSettings:BasePath"];
    if (string.IsNullOrWhiteSpace(basePath)) { logger.LogError("BasePath not configured."); return Results.Problem("Server config error."); }
    string userDirectory = Path.Combine(basePath, document.TargetUserId ?? "_unknown_user"); string filePath = Path.Combine(userDirectory, document.StoredFileName);
    var fullBasePath = Path.GetFullPath(basePath); var fullFilePath = Path.GetFullPath(filePath);
    if (!fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase)) { logger.LogError("Path Traversal: Doc {DocumentId}, Path {FilePath}", id, filePath); return Results.BadRequest("Invalid path."); }
    if (document.StoredFileName.Contains("..") || document.StoredFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) { logger.LogError("Invalid Filename: Doc {DocumentId}, FileName {FileName}", id, document.StoredFileName); return Results.BadRequest("Invalid filename."); }
    if (!File.Exists(filePath)) { logger.LogWarning("File not found: Doc {DocumentId}, Path: {FilePath}", id, filePath); return Results.NotFound($"File not found."); }
    logger.LogInformation("DEMO MODE: Serving file: Doc {DocumentId}, Path: {FilePath}", id, filePath);
    return Results.File(filePath, document.ContentType ?? "application/octet-stream", document.OriginalFileName);
}); // No .RequireAuthorization()

// --- END Middleware ---
app.Run();