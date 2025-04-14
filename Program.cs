// File: Program.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LehmanCustomConstruction.Components;
using LehmanCustomConstruction.Components.Account;
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Blogs.Interfaces;       // Ensure Blog interfaces namespace is correct
using LehmanCustomConstruction.Data.Blogs.Repository;   // Ensure Blog repository namespace is correct
using LehmanCustomConstruction.Data.Interfaces;           // Namespace for IPageContentRepository & IContactInquiryRepository
using LehmanCustomConstruction.Data.Repositories;         // Namespace for PageContentRepository & ContactInquiryRepository
// using LehmanCustomConstruction.Services;                  // Add when IntuitService is needed
using Radzen;                                           // Namespace for AddRadzenComponents

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- Repositories ---
// Register the REAL repositories. Assumes the interfaces/classes exist in the specified namespaces.
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();
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
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- Identity Core Configuration ---
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
// builder.Services.AddScoped<IntuitService>(); 


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

// --- Endpoint Mapping ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Add custom endpoints (like file upload controllers) here later if needed
// app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// --- NO PLACEHOLDER CODE BELOW THIS LINE ---