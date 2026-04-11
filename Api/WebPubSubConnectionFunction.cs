using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;
using StacyClouds.SwaAuth.Api;

namespace Api;

public class WebPubSubConnectionFunction
{
    private readonly ILogger<WebPubSubConnectionFunction> _logger;
    private readonly WebPubSub _webPubSub;
    private readonly Container _watchListContainer;

    public WebPubSubConnectionFunction(
        ILogger<WebPubSubConnectionFunction> logger,
        WebPubSub webPubSub,
        IConfiguration configuration)
    {
        _logger = logger;
        _webPubSub = webPubSub;

        var cosmosClient = new CosmosClient(configuration["CosmosDbConnectionString"]);
        var databaseName = configuration["CosmosDbDatabaseName"] ?? "BookTracker";
        var containerName = configuration["CosmosDbWatchListContainerName"] ?? "WatchList";
        var database = cosmosClient.GetDatabase(databaseName);
        _watchListContainer = database.GetContainer(containerName);
    }
    
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

            var webPubSubServiceClient = _webPubSub.Client;

            // Get all books the user is watching
            var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", user!.UserId);

            var iterator = _watchListContainer.GetItemQueryIterator<WatchList>(query);
            var watchList = new List<WatchList>();

            while (iterator.HasMoreResults)
            {
                var watchListResponse = await iterator.ReadNextAsync();
                watchList.AddRange(watchListResponse);
            }

            _logger.LogInformation($"User {user.UserId} is watching {watchList.Count} books");

            // Generate connection URL - this is what the client will use to connect directly to Web PubSub
            var connectionUri = await webPubSubServiceClient.GetClientAccessUriAsync(
                userId: user.UserId,
                roles: [],
                groups: watchList.Select(wl => wl.BookId),
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
            _logger.LogError(ex, "Error generating Web PubSub connection");
            return new StatusCodeResult(500);
        }
    }
}