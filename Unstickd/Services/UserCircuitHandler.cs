using Microsoft.AspNetCore.Components.Server.Circuits;
using Serilog;

namespace Unstickd.Services;

public class UserCircuitHandler : CircuitHandler
{
    private string _circuitId = string.Empty;

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitId = circuit.Id;
        Log.Information("🟢 Circuit Opened: {CircuitId}", _circuitId);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Log.Information("🔴 Circuit Closed: {CircuitId}", circuit.Id);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Log.Information("⬆️ Connection UP: {CircuitId}", circuit.Id);
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Log.Information("⬇️ Connection DOWN: {CircuitId}", circuit.Id);
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }
}
