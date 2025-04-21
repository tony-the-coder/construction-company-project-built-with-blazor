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
using LehmanCustomConstruction.Data.Common; // Namespace for EmailSettings AND CustomerDocument
using LehmanCustomConstruction.Services.Interfaces; // Namespace for IEmailService
using LehmanCustomConstruction.Services; // Namespace for EmailService
using Radzen;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
// No DemoAuthStateProvider needed

var builder = WebApplication.CreateBuilder(args);

// --- START: Service Registration ---

// <<< ADD API Controller Services >>>
builder.Services.AddControllers();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true; // Enable detailed errors for circuits
    });

// --- Core Blazor & Identity Services ---
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
// --- Use Standard AuthenticationStateProvider ---
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- Antiforgery Service ---
// Note: API Controllers might need specific antiforgery token handling if not using Blazor forms
builder.Services.AddAntiforgery();

// --- HttpContextAccessor ---
builder.Services.AddHttpContextAccessor(); // Useful for API controllers too

// --- Bind EmailSettings Configuration ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// --- Repositories ---
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();

// --- Custom Services ---
builder.Services.AddScoped<IEmailService, EmailService>();

// --- Authentication & Identity ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// --- DbContext Configuration ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- Identity Core Configuration ---
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// --- Radzen Services ---
builder.Services.AddRadzenComponents(); // Includes DialogService, NotificationService etc.

// --- HttpClient (Example) ---
// builder.Services.AddHttpClient(...);

// --- END: Service Registration ---
// ======================================================================
var app = builder.Build();
// ======================================================================
// --- START: Middleware Pipeline Configuration ---

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Keep Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Keep Antiforgery middleware
app.UseAntiforgery();

// --- Endpoint Mapping ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// <<< MAP API CONTROLLERS >>>
// This line enables routing to your UploadController actions
app.MapControllers();

// --- File Download Minimal API Endpoint (Auth Disabled for Demo) ---
app.MapGet("/download/{id:int}", async (
    int id,
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    ILogger<Program> logger) =>
{
    // ... (Download logic with auth checks removed, as provided before) ...
    logger.LogWarning("DEMO MODE: Download endpoint accessed anonymously for document ID {DocumentId}. RE-ENABLE AUTH!", id);
    var document = await dbContext.CustomerDocuments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    if (document == null) { return Results.NotFound($"Document with ID {id} not found or marked as deleted."); }
    var basePath = configuration["FileUploadSettings:BasePath"];
    if (string.IsNullOrWhiteSpace(basePath)) { logger.LogError("FileUploadSettings:BasePath not configured."); return Results.Problem("Server configuration error: Upload path not set."); }
    string userDirectory = Path.Combine(basePath, document.TargetUserId ?? "_unknown_user");
    string filePath = Path.Combine(userDirectory, document.StoredFileName);
    var fullBasePath = Path.GetFullPath(basePath);
    var fullFilePath = Path.GetFullPath(filePath);
    if (!fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase)) { logger.LogError("Potential Path Traversal Attack: Document {DocumentId}, Path {FilePath}", id, filePath); return Results.BadRequest("Invalid file path."); }
    if (document.StoredFileName.Contains("..") || document.StoredFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) { logger.LogError("Invalid StoredFileName detected: Document {DocumentId}, FileName {FileName}", id, document.StoredFileName); return Results.BadRequest("Invalid stored filename."); }
    if (!File.Exists(filePath)) { logger.LogWarning("File not found on server for Document ID {DocumentId}, Path: {FilePath}", id, filePath); return Results.NotFound($"File associated with document ID {id} not found on server."); }
    logger.LogInformation("DEMO MODE: Serving file for Document ID {DocumentId}. Path: {FilePath}", id, filePath);
    return Results.File(filePath, document.ContentType ?? "application/octet-stream", document.OriginalFileName);
}); // No .RequireAuthorization()
// --- End File Download Endpoint ---

// --- END: Middleware Pipeline Configuration ---

app.Run();

// Public record for Minimal API DTO (if needed elsewhere, otherwise can be local)
// Remove if not used by any Minimal API:
// public record LoginRequestDto(string Email, string Password, bool RememberMe);