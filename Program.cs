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
// using Microsoft.AspNetCore.Components.Web; // No longer strictly needed here

var builder = WebApplication.CreateBuilder(args);

// --- START: Service Registration ---

builder.Services.AddRazorComponents()
    // <<< FIX: Enable DetailedErrors for Blazor Server circuits >>>
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true; // <<< ADD THIS LINE
    });

// --- Core Blazor & Identity Services ---
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- Antiforgery Service ---
builder.Services.AddAntiforgery();

// --- HttpContextAccessor ---
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddRadzenComponents();

// --- HttpClient (Example) ---
// builder.Services.AddHttpClient(...);

// --- END: Service Registration ---
// ======================================================================
var app = builder.Build();
// ======================================================================
// --- START: Middleware Pipeline Configuration ---

// Detailed Errors are also often enabled by default in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage(); // Shows detailed error page for non-Blazor requests
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization MUST come before Antiforgery and Endpoints
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery SHOULD come after Auth but before Endpoint execution
app.UseAntiforgery();

// --- Endpoint Mapping ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // Keep this simple as before

// --- Map Identity Endpoints ---
// Ensure this is active to handle the POST from Login.razor and others
app.MapAdditionalIdentityEndpoints();

// --- File Download Minimal API Endpoint ---
app.MapGet("/download/{id:int}", async (
    int id,
    HttpContext httpContext,
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    ILogger<Program> logger) =>
{
    // ... (Your existing download logic remains unchanged) ...
    if (!(httpContext.User.Identity?.IsAuthenticated ?? false)) { /*...*/ return Results.Unauthorized(); }
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) { /*...*/ return Results.Forbid(); }
    var document = await dbContext.CustomerDocuments.FindAsync(id);
    if (document == null) { /*...*/ return Results.NotFound($"Document with ID {id} not found."); }
    if (document.UploadedById != userId && document.TargetUserId != userId /*&& !httpContext.User.IsInRole("Admin")*/ ) { /*...*/ return Results.Forbid(); }
    var basePath = configuration["FileUploadSettings:BasePath"];
    if (string.IsNullOrWhiteSpace(basePath)) { /*...*/ return Results.Problem("Server configuration error: Upload path not set."); }
    var filePath = Path.Combine(basePath, document.UploadedById, document.StoredFileName);
    if (document.StoredFileName.Contains("..") || document.StoredFileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) { /*...*/ return Results.BadRequest("Invalid filename."); }
    if (!File.Exists(filePath)) { /*...*/ return Results.NotFound($"File associated with document ID {id} not found on server."); }
    logger.LogInformation("Serving file for Document ID {DocumentId} to user {UserId}. Path: {FilePath}", id, userId, filePath);
    return Results.File(filePath, document.ContentType ?? "application/octet-stream", document.OriginalFileName);

}).RequireAuthorization();
// --- End File Download Endpoint ---

// --- END: Middleware Pipeline Configuration ---

app.Run();