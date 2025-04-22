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
using System.Security.Claims; // Needed for ClaimTypes and FindFirstValue
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http; // Needed for HttpContext in minimal API

var builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => { options.DetailedErrors = true; });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>(); // Standard provider
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor(); // Keep this, useful elsewhere too
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Repositories
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();
// Messaging Repositories (Add these if you created them)
// builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
// builder.Services.AddScoped<IMessageRepository, MessageRepository>();

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
    .AddRoles<IdentityRole>() // <<< ADD THIS if you use Roles (like "Admin")
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Radzen Services
builder.Services.AddRadzenComponents();

// --- END Service Registration ---

var app = builder.Build();


// File: Program.cs (Place this section after "var app = builder.Build();")

// === START: Seed Roles and Admin User (Run ONCE on startup) ===
using (var scope = app.Services.CreateScope()) // Create a service scope to resolve services
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = services.GetRequiredService<IConfiguration>(); // Get configuration

    logger.LogInformation("Attempting to seed roles and admin user...");
    try
    {
        // 1. Seed Roles
        string[] roleNames = { "Admin", "Marketing", "OfficeAdmin", "Accounting" }; // Add/remove roles as needed
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Created Role: {RoleName}", roleName);
                }
                else
                {
                    // Log errors if role creation fails
                    logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // 2. Seed Initial Admin User
        // IMPORTANT: Configure these in appsettings.Development.json or User Secrets!
        string adminEmail = configuration["SeedAdmin:Email"] ?? "admin@lehman.test"; // Default if not configured
        string adminPassword = configuration["SeedAdmin:Password"] ?? "TempP@ss123!"; // Default if not configured

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail, // Use email as username by default
                Email = adminEmail,
                EmailConfirmed = true // IMPORTANT: Confirm email so they can log in immediately
                // Add any other required user properties here if necessary
            };
            var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createUserResult.Succeeded)
            {
                logger.LogInformation("Created Seed Admin User: {AdminEmail}", adminEmail);

                // 3. Assign the 'Admin' role to the new Admin User
                if (await roleManager.RoleExistsAsync("Admin")) // Ensure role exists before assigning
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Assigned 'Admin' role to {AdminEmail}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to assign 'Admin' role to {AdminEmail}: {Errors}", adminEmail, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Cannot assign 'Admin' role: Role does not exist.");
                }
            }
            else
            {
                logger.LogError("Failed to create seed admin user {AdminEmail}: {Errors}", adminEmail, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user {AdminEmail} already exists. Ensuring Admin role assignment...", adminEmail);
            // Optional: Ensure existing admin user has the Admin role if they somehow lost it
            if (await roleManager.RoleExistsAsync("Admin") && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (addToRoleResult.Succeeded) { logger.LogInformation("Ensured 'Admin' role is assigned to {AdminEmail}", adminEmail); }
                else { logger.LogError("Failed to assign 'Admin' role to existing user {AdminEmail}: {Errors}", adminEmail, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))); }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during role/admin seeding.");
        // Consider whether the app should fail to start if seeding fails critical roles/users
        // throw;
    }
}
// === END: Seed Roles and Admin User ===



// --- Middleware ---
if (app.Environment.IsDevelopment()) { app.UseMigrationsEndPoint(); app.UseDeveloperExceptionPage(); }
else { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization(); // Must be after UseAuthentication
app.UseAntiforgery();

// --- Endpoint Mapping ---
// Use Global Interactive Mode based on previous working template/Radzen needs
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// File Download Minimal API Endpoint (Re-enabled Auth)
app.MapGet("/download/{id:int}", async (
    int id,
    HttpContext httpContext, // <<< Add HttpContext back
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    ILogger<Program> logger) =>
{
    // --- RE-ADD AUTH CHECKS ---
    if (!(httpContext.User.Identity?.IsAuthenticated ?? false)) { return Results.Unauthorized(); }
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    var isAdmin = httpContext.User.IsInRole("Admin"); // Check role
    if (userId == null) { logger.LogError("Auth user missing NameIdentifier claim."); return Results.Forbid(); }
    // --- END AUTH CHECKS ---

    var document = await dbContext.CustomerDocuments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    if (document == null) { return Results.NotFound($"Document ID {id} not found or deleted."); }

    // --- RE-ADD PERMISSION CHECK ---
    if (document.UploadedById != userId && document.TargetUserId != userId && !isAdmin)
    {
        logger.LogWarning("Forbidden download: User {UserId}, Doc {DocumentId}, Target {TargetUserId}, Uploader {UploaderId}", userId, id, document.TargetUserId, document.UploadedById);
        return Results.Forbid();
    }
    // --- END PERMISSION CHECK ---

    var basePath = configuration["FileUploadSettings:BasePath"];
    if (string.IsNullOrWhiteSpace(basePath)) { logger.LogError("BasePath not configured."); return Results.Problem("Server config error."); }
    string userDirectory = Path.Combine(basePath, document.TargetUserId ?? "_unknown_user"); string filePath = Path.Combine(userDirectory, document.StoredFileName);
    var fullBasePath = Path.GetFullPath(basePath); var fullFilePath = Path.GetFullPath(filePath);
    if (!fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase)) { logger.LogError("Path Traversal: Doc {DocumentId}, Path {FilePath}", id, filePath); return Results.BadRequest("Invalid path."); }
    if (document.StoredFileName.Contains("..") || document.StoredFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) { logger.LogError("Invalid Filename: Doc {DocumentId}, FileName {FileName}", id, document.StoredFileName); return Results.BadRequest("Invalid filename."); }
    if (!File.Exists(filePath)) { logger.LogWarning("File not found: Doc {DocumentId}, Path: {FilePath}", id, filePath); return Results.NotFound($"File not found."); }

    logger.LogInformation("Serving file: Doc {DocumentId}, Path {FilePath} to User {UserId}", id, filePath, userId);
    return Results.File(filePath, document.ContentType ?? "application/octet-stream", document.OriginalFileName);

})
.RequireAuthorization(); // <<< ADD .RequireAuthorization() BACK
// --- End File Download Endpoint ---

// --- END Middleware ---
app.Run();