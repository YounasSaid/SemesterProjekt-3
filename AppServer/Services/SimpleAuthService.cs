using System.Collections.Concurrent;

namespace AppServer.Services;

/// <summary>
/// Singleton service til simpel session tracking af indlogget bruger.
/// Gemmer den aktuelle userId i memory (én bruger = én aktiv session ad gangen).
/// </summary>
public class SimpleAuthService
{
    // Gemmer bare den aktuelle userId - ingen circuit tracking nødvendig
    private string? _currentUserId;
    private readonly object _lock = new object();

    public void SetUserId(string userId)
    {
        lock (_lock)
        {
            _currentUserId = userId;
            Console.WriteLine($"SimpleAuthService: Stored userId={userId}");
        }
    }

    public string? GetUserId()
    {
        lock (_lock)
        {
            Console.WriteLine($"SimpleAuthService: GetUserId() = {_currentUserId ?? "NULL"}");
            return _currentUserId;
        }
    }

    public void ClearUserId()
    {
        lock (_lock)
        {
            Console.WriteLine($"SimpleAuthService: Cleared userId");
            _currentUserId = null;
        }
    }
}
