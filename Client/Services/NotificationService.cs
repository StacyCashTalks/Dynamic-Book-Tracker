using Microsoft.JSInterop;
using System.Text.Json;
using Shared.Models;

namespace Client.Services;

public class NotificationService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private DotNetObjectReference<NotificationService>? _dotNetReference;

    public event Action<Notification>? OnNotificationReceived;

    public NotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync(string connectionUrl, string userId)
    {
        _dotNetReference = DotNetObjectReference.Create(this);
        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/notifications.js");
        await _module.InvokeVoidAsync("initialize", connectionUrl, userId, _dotNetReference);
    }

    [JSInvokable]
    public void ReceiveNotification(string notificationJson)
    {
        try
        {
            var notification = JsonSerializer.Deserialize<Notification>(notificationJson);
            if (notification != null)
            {
                OnNotificationReceived?.Invoke(notification);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing notification: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("disconnect");
            await _module.DisposeAsync();
        }

        _dotNetReference?.Dispose();
    }
}
