using Azure.Messaging.WebPubSub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api;

public class WebPubSub
{
    public WebPubSub(ILogger<WebPubSub> logger, IConfiguration configuration)
    {
        var connectionString = configuration["WebPubSubConnectionString"];
        var hubName = configuration["WebPubSubHubName"] ?? "notifications";
        Client = new WebPubSubServiceClient(connectionString, hubName);
    }

    public WebPubSubServiceClient Client { get; private init; }
}