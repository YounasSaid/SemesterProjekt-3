using System.Collections.Concurrent;

namespace AppServer.Services;

/// <summary>
/// Singleton service that stores the currently logged-in userId
/// SIMPLIFICATION: One user = one active session at a time
/// </summary>
public class SimpleAuthService
{
    // Just store the current userId - no circuit tracking needed!
    private string? _currentUserId;
    private readonly object _lock = new object();

    public void SetUserId(string userId)
    {
        lock (_lock)
        {
            _currentUserId = userId;
            Console.WriteLine($"✅ SimpleAuthService: Stored userId={userId}");
        }
    }

    public string? GetUserId()
    {
        lock (_lock)
        {
            Console.WriteLine($"🔍 SimpleAuthService: GetUserId() = {_currentUserId ?? "NULL"}");
            return _currentUserId;
        }
    }

    public void ClearUserId()
    {
        lock (_lock)
        {
            Console.WriteLine($"🗑️ SimpleAuthService: Cleared userId");
            _currentUserId = null;
        }
    }
}
