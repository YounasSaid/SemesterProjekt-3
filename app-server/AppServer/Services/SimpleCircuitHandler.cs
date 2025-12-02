using Microsoft.AspNetCore.Components.Server.Circuits;

namespace AppServer.Services;

/// <summary>
/// Circuit handler that clears userId when circuit disconnects
/// </summary>
public class SimpleCircuitHandler : CircuitHandler
{
    private readonly SimpleAuthService _authService;

    public SimpleCircuitHandler(SimpleAuthService authService)
    {
        _authService = authService;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine($"🟢 SimpleCircuitHandler: Circuit connected = {circuit.Id}");
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine($"🔴 SimpleCircuitHandler: Circuit disconnected = {circuit.Id}");
        _authService.ClearUserId();
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }
}