using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Shared.Models;
using Websocket.Client;

namespace Client.Services;

public class WebPubSubService(HttpClient httpClient) : IDisposable
{
    private WebsocketClient? _webSocket;

    public event Action<Notification>? OnMessageReceived;
    public event Action<bool>? OnConnectionStateChanged;

    public bool IsConnected => _webSocket?.NativeClient?.State == WebSocketState.Open;
    public bool IsConnecting { get; private set; }

    public async Task<bool> ConnectAsync()
    {
        if (IsConnected)
        {
            return true;
        }
        
        IsConnecting = true;
        
        try
        {
            var connectionInfo = await httpClient.GetFromJsonAsync<ConnectionResponse>(
                $"api/webpubsub/negotiate");
            if (connectionInfo is null)
            {
                Console.WriteLine("Unable to retrieve connection string");
                return false;
            }

            _webSocket = new WebsocketClient(new Uri(connectionInfo.Url!));
            _webSocket.ReconnectTimeout = TimeSpan.FromHours(10);
            _webSocket.MessageReceived.Subscribe(DeserializeMessage);
            _webSocket.ReconnectionHappened.Subscribe(_ => OnConnectionStateChanged?.Invoke(IsConnected));
            _webSocket.DisconnectionHappened.Subscribe(_ => OnConnectionStateChanged?.Invoke(false));
            await _webSocket.Start();
            OnConnectionStateChanged?.Invoke(true);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private void DeserializeMessage(ResponseMessage msg)
    {
        var messageJson = msg.Text;
        if (string.IsNullOrWhiteSpace(messageJson))
        {
            return;
        }

        var message = ParseNotification(messageJson);
        if (message is null)
        {
            return;
        }
        
        OnMessageReceived?.Invoke(message);
    }

    private static Notification? ParseNotification(string messageJson)
    {
        try
        {
            var root = JsonNode.Parse(messageJson);
            if (root is not JsonObject messageObject)
            {
                return null;
            }

            if (messageObject["type"]?.GetValue<string>() == "message"
                && messageObject["from"]?.GetValue<string>() == "server")
            {
                var data = messageObject["data"];
                if (data is JsonValue stringValue)
                {
                    var nestedJson = stringValue.GetValue<string>();
                    return JsonSerializer.Deserialize<Notification>(nestedJson, JsonSerializerOptions.Web);
                }

                return data?.Deserialize<Notification>(JsonSerializerOptions.Web);
            }

            return messageObject.Deserialize<Notification>(JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
        httpClient.Dispose();
    }

}
