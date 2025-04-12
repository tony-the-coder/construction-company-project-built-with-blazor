using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LehmanCustomConstruction.Components;
using LehmanCustomConstruction.Components.Account;
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Blogs.Interfaces;
using LehmanCustomConstruction.Data.Blogs.Repository;
using Radzen;
using Radzen.Blazor;
using System; // Required for Action<>

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// --- START: Modified DbContext Registration ---

// Define the configuration action once to avoid potential issues
Action<DbContextOptionsBuilder> dbContextOptionsAction = options =>
    options.UseSqlServer(connectionString);

// Register the DbContext for Identity (scoped lifetime) using the action
//builder.Services.AddDbContext<ApplicationDbContext>(dbContextOptionsAction);

// Register the DbContext Factory (singleton lifetime) using the same action
builder.Services.AddDbContextFactory<ApplicationDbContext>(dbContextOptionsAction);

// --- END: Modified DbContext Registration ---


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// This depends on the scoped DbContext registered by AddDbContext
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add Radzen Blazor component services
builder.Services.AddRadzenComponents(); // Make sure this is present

// Add HttpClient for Intuit API (Uncomment and configure when needed)
//builder.Services.AddHttpClient("IntuitAPI", client =>
//{
//    client.BaseAddress = new Uri("https://sandbox-quickbooks.api.intuit.com/");
//    // You can add default headers or other configurations here if needed.
//});

// Register IntuitService (Uncomment and configure when needed)
//builder.Services.AddScoped<LehmanCustomConstruction.Services.IntuitService>(); // Add this line


var app = builder.Build(); // This line should no longer throw the service validation error

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Ensure static files middleware is present
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run(); 