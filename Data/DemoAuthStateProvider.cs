// File: DemoAuthStateProvider.cs (Choose a suitable location)
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

// WARNING: DEMO ONLY - Bypasses real authentication
public class DemoAuthStateProvider : AuthenticationStateProvider
{
    private readonly Task<AuthenticationState> _authenticationStateTask;
    private static readonly string _demoUserId = "683a96be-9b4a-4af7-a863-b98a88815cae"; // test4@test.com ID
    private static readonly string _demoUserName = "test4@test.com"; // test4@test.com Email/Username
    private static readonly string _demoRole = "Admin"; // Role to assign for demo

    public DemoAuthStateProvider()
    {
        // Create the claims for the simulated user
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _demoUserId), // Essential for getting User ID
            new Claim(ClaimTypes.Name, _demoUserName),         // Used for display name
            new Claim(ClaimTypes.Email, _demoUserName),        // Optional email claim
            new Claim(ClaimTypes.Role, _demoRole)              // Essential for role checks
        }, "DemoAuth"); // Use a custom authentication type name

        var user = new ClaimsPrincipal(identity);
        _authenticationStateTask = Task.FromResult(new AuthenticationState(user));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Always return the hardcoded authenticated user state
        return _authenticationStateTask;
    }
}