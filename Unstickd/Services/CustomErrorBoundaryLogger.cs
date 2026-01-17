using Microsoft.AspNetCore.Components.Web;
using Serilog;

namespace Unstickd.Services;

// This intercepts errors caught by <ErrorBoundary> and logs them properly
public class CustomErrorBoundaryLogger : IErrorBoundaryLogger
{
    public ValueTask LogErrorAsync(Exception exception)
    {
        // Log with Serilog with high distinctiveness
        Log.Error(exception, "🛑 Blazor UI Error Boundary caught an exception");
        return ValueTask.CompletedTask;
    }
}
