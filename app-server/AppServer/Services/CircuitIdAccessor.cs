using Microsoft.AspNetCore.Components.Server.Circuits;

namespace AppServer.Services;

/// <summary>
/// Provides access to the current Circuit ID
/// </summary>
public class CircuitIdAccessor
{
    private static readonly AsyncLocal<string?> _circuitId = new();

    public string? CircuitId
    {
        get => _circuitId.Value;
        set => _circuitId.Value = value;
    }
}
