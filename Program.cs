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
using LehmanCustomConstruction.Data.Common; // Namespace for EmailSettings
using LehmanCustomConstruction.Services.Interfaces; // Namespace for IEmailService
using LehmanCustomConstruction.Services; // Namespace for EmailService
// using LehmanCustomConstruction.Services; // Add when IntuitService is needed
using Radzen;

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
builder.Services.AddHttpContextAccessor(); // Needed by some services, like StatusMessage cookie handling

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
//     client.BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com/");
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
// Ensure these are active and in the correct order
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

// Add custom API controller endpoints here later if needed
// app.MapControllers();

app.Run();