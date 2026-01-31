using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoryFort.Services;

public record ToastItem(string Id, string Title, string Message, string Variant = "primary", int DurationMs = 4000);

public class ToastService
{
    public event Action<ToastItem>? OnToast;

    public void Show(string title, string message, string variant = "primary", int durationMs = 4000)
    {
        var id = Guid.NewGuid().ToString("N");
        OnToast?.Invoke(new ToastItem(id, title, message, variant, durationMs));
    }
}
