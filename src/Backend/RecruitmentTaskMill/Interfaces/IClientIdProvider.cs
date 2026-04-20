namespace Backend.Interfaces;

public interface IClientIdProvider
{
    string GetOrCreateClientId(HttpContext httpContext);
}
