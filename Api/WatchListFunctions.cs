using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;
using StacyClouds.SwaAuth.Api;

namespace Api;

public class WatchListFunctions
{
    private readonly ILogger<WatchListFunctions> _logger;
    private readonly Container _watchListContainer;
    private readonly WebPubSub _webPubSub;

    public WatchListFunctions(ILogger<WatchListFunctions> logger, IConfiguration configuration, WebPubSub webPubSub)
    {
        _logger = logger;
        _webPubSub = webPubSub;
        var connectionString = configuration["CosmosDbConnectionString"];
        _logger.LogCritical(connectionString);

        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };
        
        var cosmosClient = new CosmosClient(connectionString, options);
        var databaseName = configuration["CosmosDbDatabaseName"];
        var database = cosmosClient.GetDatabase(databaseName);

        _watchListContainer = database.GetContainer(configuration["CosmosDbWatchListContainerName"]);
    }

    [Function("AddToWatchList")]
    public async Task<IActionResult> AddToWatchList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "watchlist/{bookId}")] HttpRequest req,
        string bookId)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        // Check if already watching
        var query = new QueryDefinition("SELECT * FROM c WHERE c.BookId = @bookId AND c.UserId = @userId")
            .WithParameter("@bookId", bookId)
            .WithParameter("@userId", user!.UserId);

        var iterator = _watchListContainer.GetItemQueryIterator<WatchList>(query);
        var existing = new List<WatchList>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            existing.AddRange(response);
        }

        if (existing.Any())
        {
            return new OkObjectResult(existing.First());
        }

        var watchList = new WatchList
        {
            Id = Guid.NewGuid().ToString(),
            BookId = bookId,
            UserId = user!.UserId!
        };

        var createResponse = await _watchListContainer.CreateItemAsync(watchList, new PartitionKey(watchList.UserId));

        // Add user to the book's Web PubSub group
        try
        {
            await _webPubSub.Client.AddUserToGroupAsync(bookId, user.UserId);
            _logger.LogInformation($"Added user {user.UserId} to group {bookId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to add user {user.UserId} to group {bookId}");
        }

        return new CreatedResult($"/api/watchlist/{bookId}", createResponse.Resource);
    }

    [Function("RemoveFromWatchList")]
    public async Task<IActionResult> RemoveFromWatchList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "watchlist/{bookId}")] HttpRequest req,
        string bookId)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        var query = new QueryDefinition("SELECT * FROM c WHERE c.BookId = @bookId AND c.UserId = @userId")
            .WithParameter("@bookId", bookId)
            .WithParameter("@userId", user!.UserId);

        var iterator = _watchListContainer.GetItemQueryIterator<WatchList>(query);
        var existing = new List<WatchList>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            existing.AddRange(response);
        }

        if (!existing.Any())
        {
            return new NotFoundResult();
        }

        var watchList = existing.First();
        await _watchListContainer.DeleteItemAsync<WatchList>(watchList.Id, new PartitionKey(watchList.UserId));

        // Remove user from the book's Web PubSub group
        try
        {
            await _webPubSub.Client.RemoveUserFromGroupAsync(bookId, user.UserId);
            _logger.LogInformation($"Removed user {user.UserId} from group {bookId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to remove user {user.UserId} from group {bookId}");
        }

        return new NoContentResult();
    }

    [Function("GetUserWatchList")]
    public async Task<IActionResult> GetUserWatchList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watchlist")] HttpRequest req)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", user!.UserId);

        var iterator = _watchListContainer.GetItemQueryIterator<WatchList>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(user.UserId)
            }
        );
        var watchList = new List<WatchList>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            watchList.AddRange(response);
        }

        return new OkObjectResult(watchList);
    }
}
