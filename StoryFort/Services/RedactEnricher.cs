using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;

namespace StoryFort.Services;

public class RedactEnricher : ILogEventEnricher
{
    private static readonly HashSet<string> SensitiveKeys = new()
    {
        "CohereApiKey",
        "ProtectedCohereApiKey",
        "Story.Content",
        "Content",
        "ProtectedKey"
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Replace any sensitive property values with [REDACTED]
        foreach (var key in SensitiveKeys)
        {
            if (logEvent.Properties.ContainsKey(key))
            {
                logEvent.RemovePropertyIfPresent(key);
                logEvent.AddOrUpdateProperty(new LogEventProperty(key, new ScalarValue("[REDACTED]")));
            }
        }
    }
}
