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
// using LehmanCustomConstruction.Services; // Add when IntuitService is needed
using Radzen;
using System.Security.Claims; // Added for ClaimTypes
using Microsoft.Extensions.Configuration; // Added for IConfiguration access in endpoint
using Microsoft.Extensions.Logging; // Added for ILogger access in endpoint
using System.IO; // Added for Path operations

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Core Blazor & Identity Services ---
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>(); // Keep for non-interactive scenarios if needed elsewhere
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- HttpContextAccessor ---
builder.Services.AddHttpContextAccessor(); // Needed by some services, like StatusMessage cookie handling AND our download endpoint

// --- Bind EmailSettings Configuration ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// --- Repositories ---
// Assuming these have been updated to use IServiceProvider injection pattern
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();
// Add Portfolio repositories later

// --- Custom Services ---
builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<IntuitService>();

// --- Authentication & Identity ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// --- DbContext Configuration ---
// Register the Scoped DbContext required by default Identity stores
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- Identity Core Configuration ---
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Use the NoOp sender for now unless a real one is configured
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// --- Radzen Services ---
builder.Services.AddRadzenComponents();

// --- HttpClient for Intuit API (Uncomment and configure when needed) ---
// builder.Services.AddHttpClient("IntuitAPI", client =>
// {
//    client.BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com/");
// });


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
app.UseStaticFiles();
app.UseAntiforgery();

// --- Authentication & Authorization Middleware ---
// Ensure these are active and in the correct order (before endpoint mapping)
app.UseAuthentication();
app.UseAuthorization();
// --- End Auth ---

// --- Endpoint Mapping ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Map Identity Endpoints ---
// Ensure this is active to handle the POST from Login.razor
app.MapAdditionalIdentityEndpoints();
// --- End Identity Endpoints ---

// --- Add the Download Minimal API Endpoint ---
// **********************************************
// ** START: File Download Endpoint Definition **
// **********************************************
app.MapGet("/download/{id:int}", async (
    int id,
    HttpContext httpContext, // Access HttpContext directly
    ApplicationDbContext dbContext,
    IConfiguration configuration, // Inject IConfiguration
    ILogger<Program> logger) => // Inject logger (using Program as category)
{
    // 1. Check Authentication
    if (!(httpContext.User.Identity?.IsAuthenticated ?? false))
    {
        logger.LogWarning("Download attempt for ID {DocumentId} by unauthenticated user.", id);
        return Results.Unauthorized();
    }

    // 2. Get User ID
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
    {
        logger.LogError("Download failed for ID {DocumentId}: Could not determine user identity for authenticated user.", id);
        return Results.Forbid(); // Or BadRequest
    }

    // 3. Retrieve Document Metadata from DB
    var document = await dbContext.CustomerDocuments.FindAsync(id);
    if (document == null)
    {
        logger.LogWarning("Download attempt failed: Document with ID {DocumentId} not found.", id);
        return Results.NotFound($"Document with ID {id} not found.");
    }

    // 4. IMPORTANT: Authorization Check - Does this user OWN the document?
    //    (Adjust this logic based on your requirements - e.g., allow admins, check TargetUserId?)
    //    Current logic: User must be the one who uploaded OR the target user.
    if (document.UploadedById != userId && document.TargetUserId != userId /*&& !httpContext.User.IsInRole("Admin")*/ )
    {
        logger.LogWarning("Forbidden download attempt for ID {DocumentId} by user {UserId}. Document owner: {OwnerId}, Target: {TargetId}",
           id, userId, document.UploadedById, document.TargetUserId);
        return Results.Forbid(); // User does not have permission
    }

    // 5. Construct File Path
    var basePath = configuration["FileUploadSettings:BasePath"]; // Read from config
    if (string.IsNullOrWhiteSpace(basePath))
    {
        logger.LogError("Download failed for ID {DocumentId}: FileUploadSettings:BasePath is not configured.", id);
        return Results.Problem("Server configuration error: Upload path not set.");
    }
    // Use UploadedById for the subfolder, matching the upload logic
    var filePath = Path.Combine(basePath, document.UploadedById, document.StoredFileName);

    // Basic check to prevent path traversal using StoredFileName (should be GUID anyway)
    if (document.StoredFileName.Contains("..") || document.StoredFileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
    {
        logger.LogError("Download failed for ID {DocumentId}: Invalid characters in stored filename '{StoredFileName}'.", id, document.StoredFileName);
        return Results.BadRequest("Invalid filename.");
    }

    // 6. Check if File Exists
    if (!File.Exists(filePath))
    {
        logger.LogError("Download failed for ID {DocumentId}: File not found at physical path {FilePath}. StoredFileName: {StoredFileName}",
            id, filePath, document.StoredFileName);
        return Results.NotFound($"File associated with document ID {id} not found on server.");
    }

    // 7. Return the File
    logger.LogInformation("Serving file for Document ID {DocumentId} to user {UserId}. Path: {FilePath}", id, userId, filePath);
    // Returns the file stream. Browser will handle the download dialog.
    // Provide the original filename so the user sees the correct name when saving.
    return Results.File(filePath, document.ContentType ?? "application/octet-stream", document.OriginalFileName);

}).RequireAuthorization(); // Ensures only logged-in users can even hit the endpoint
// ********************************************
// ** END: File Download Endpoint Definition **
// ********************************************


// Add custom API controller endpoints here later if needed
// app.MapControllers();

app.Run();