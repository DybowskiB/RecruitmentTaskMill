using Backend.Interfaces;

namespace Backend.Services;

public sealed class ClientIdProvider : IClientIdProvider
{
    private const string CookieName = "client-id";

    public string GetOrCreateClientId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(CookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var clientId = Guid.NewGuid().ToString("N");

        httpContext.Response.Cookies.Append(CookieName, clientId, new CookieOptions
        {
            HttpOnly = false,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

        return clientId;
    }
}
