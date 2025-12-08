namespace AppServer.Services;

// Helper service til at få Circuit ID fra HttpContext (Scoped injection)
public class CircuitAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CircuitAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCircuitId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            Console.WriteLine("CircuitAccessor: No HttpContext available");
            return null;
        }

        if (httpContext.Items.TryGetValue("__CircuitId__", out var circuitId) && circuitId is string id)
        {
            return id;
        }

        Console.WriteLine("CircuitAccessor: No Circuit ID in HttpContext.Items");
        return null;
    }
}
