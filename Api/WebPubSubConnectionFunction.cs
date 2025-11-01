using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StacyClouds.SwaAuth.Api;

namespace Api;

public class WebPubSubConnectionFunction(
    ILogger<WebPubSubConnectionFunction> logger, 
    WebPubSub webPubSub)
{
    [Function("negotiate")]
    public async Task<IActionResult> GetConnection(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "webpubsub/negotiate")]
            HttpRequest req)
    {
        try
        {
            var authorised = StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user);
            if (!authorised)
            {
                return new UnauthorizedResult();
            }
            
            var webPubSubServiceClient = webPubSub.Client;

            // Generate connection URL - this is what the client will use to connect directly to Web PubSub
            var connectionUri = await webPubSubServiceClient.GetClientAccessUriAsync(
                userId: user!.UserId,
                roles: [],
                expiresAfter: TimeSpan.FromHours(1)
            );

            var response = new ConnectionResponse
            {
                Url = connectionUri.ToString()
            };

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating Web PubSub connection");
            return new StatusCodeResult(500);
        }
    }
}

public class ConnectionResponse
{
    public string Url { get; set; } = string.Empty;
}