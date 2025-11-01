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

    public WatchListFunctions(ILogger<WatchListFunctions> logger, IConfiguration configuration)
    {
        _logger = logger;

        var cosmosClient = new CosmosClient(configuration["CosmosDbConnectionString"]);
        var databaseName = configuration["CosmosDbDatabaseName"] ?? "BookTracker";
        var database = cosmosClient.GetDatabase(databaseName);

        _watchListContainer = database.GetContainer("WatchList");
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
            UserId = user.UserId
        };

        var createResponse = await _watchListContainer.CreateItemAsync(watchList, new PartitionKey(watchList.Id));
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
        await _watchListContainer.DeleteItemAsync<WatchList>(watchList.Id, new PartitionKey(watchList.Id));

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

        var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
            .WithParameter("@userId", user!.UserId);

        var iterator = _watchListContainer.GetItemQueryIterator<WatchList>(query);
        var watchList = new List<WatchList>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            watchList.AddRange(response);
        }

        return new OkObjectResult(watchList);
    }
}
