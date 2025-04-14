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
using LehmanCustomConstruction.Data.Common; // << ADDED: Namespace for EmailSettings
using LehmanCustomConstruction.Services.Interfaces; // << ADDED: Namespace for IEmailService
using LehmanCustomConstruction.Services; // << ADDED: Namespace for EmailService
// using LehmanCustomConstruction.Services; // Add when IntuitService is needed (If in same namespace)
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Core Blazor & Identity Services ---
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- ADDED: HttpContextAccessor (Needed by EmailService to get IOptions dynamically if required elsewhere, good practice) ---
builder.Services.AddHttpContextAccessor();

// --- Bind EmailSettings Configuration ---
// Reads "EmailSettings" section from appsettings.json/UserSecrets/EnvironmentVariables
// and makes IOptions<EmailSettings> available for injection
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// --- Repositories ---
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();
// Add Portfolio repositories later

// --- Custom Services ---
builder.Services.AddScoped<IEmailService, EmailService>(); // << ADDED: Register Email Service
// builder.Services.AddScoped<IntuitService>(); // Example registration for Intuit service (later)

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

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>(); // Using default NoOp sender for Identity emails unless configured otherwise

// --- Radzen Services ---
builder.Services.AddRadzenComponents(); // Includes DialogService, NotificationService, etc.

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

// Add Authentication/Authorization middleware - ORDER MATTERS HERE
// Typically UseAuthentication comes before UseAuthorization
app.UseAuthentication(); // << Ensure this is added
app.UseAuthorization();  // << Ensure this is added

// --- Endpoint Mapping ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Add custom endpoints here later if needed
// app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// --- No placeholder code below this line ---