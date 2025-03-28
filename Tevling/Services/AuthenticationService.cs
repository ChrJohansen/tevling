using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

namespace Tevling.Services;

public class AuthenticationService(
    ILogger<AuthenticationService> logger,
    AuthenticationStateProvider authenticationStateProvider,
    IAthleteService athleteService,
    IHttpContextAccessor httpContextAccessor,
    IStravaClient stravaClient)
    : IAuthenticationService
{
    public async Task LoginAsync(Athlete athlete, CancellationToken ct = default)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, athlete.Name),
            new Claim(ClaimTypes.NameIdentifier, athlete.Id.ToString()),
        };

        ClaimsIdentity claimsIdentity = new(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        AuthenticationProperties authProperties = new()
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
            IsPersistent = true,
            RedirectUri = "/",
        };

        HttpContext httpContext = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("No active HttpContext");

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        logger.LogInformation("User ID {AthleteId} logged in at {DateTime}", athlete.Id, DateTime.Now);
    }

    public async Task<Athlete> GetCurrentAthleteAsync(CancellationToken ct = default)
    {
        AuthenticationState authenticationState = await authenticationStateProvider
            .GetAuthenticationStateAsync();

        Athlete? athlete = null;

        string? athleteIdStr = authenticationState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(athleteIdStr, out int athleteId))
        {
            athlete = await athleteService.GetAthleteByIdAsync(athleteId, ct);
        }

        if (athlete is null)
        {
            throw new Exception("Logged in athlete not found");
        }

        return athlete;
    }

    public async Task LogoutAsync(bool deauthorizeApp = false, CancellationToken ct = default)
    {
        HttpContext httpContext = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("No active HttpContext");

        string? athleteIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _ = int.TryParse(athleteIdStr, out int athleteId);

        if (deauthorizeApp)
        {
            string accessToken = await athleteService.GetAccessTokenAsync(athleteId, ct);
            await stravaClient.DeauthorizeAppAsync(accessToken, ct);
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        logger.LogInformation("User ID {AthleteId} logged out at {DateTime}", athleteId, DateTime.Now);
    }
}
