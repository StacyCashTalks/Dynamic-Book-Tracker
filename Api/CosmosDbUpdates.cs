using System.Text.Json;
using Azure.Messaging.WebPubSub;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Api;

public class CosmosDbUpdates
{
    private readonly ILogger<CosmosDbUpdates> _logger;
    private readonly WebPubSubServiceClient _webPubSubClient;
    private readonly Container _watchListContainer;

    public CosmosDbUpdates(ILogger<CosmosDbUpdates> logger, WebPubSub webPubSub, IConfiguration configuration)
    {
        _logger = logger;
        _webPubSubClient = webPubSub.Client;

        var cosmosClient = new CosmosClient(configuration["CosmosDbConnectionString"]);
        var databaseName = configuration["CosmosDbDatabaseName"] ?? "BookTracker";
        var database = cosmosClient.GetDatabase(databaseName);
        _watchListContainer = database.GetContainer("WatchList");
    }

    [Function("BookUpdates")]
    public async Task Run([CosmosDBTrigger(
        databaseName: "%CosmosDbDatabaseName%",
        containerName: "Books",
        Connection = "CosmosDbConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Book> input)
    {
        if (input != null && input.Count > 0)
        {
            _logger.LogInformation($"Books modified: {input.Count}");

            foreach (var book in input)
            {
                // Notify owner when book is borrowed/returned
                if (book.InStock)
                {
                    _logger.LogInformation($"Book {book.Name} is back in stock");

                    // Notify owner
                    var ownerNotification = new Notification
                    {
                        Type = "BookReturned",
                        Message = $"Your book '{book.Name}' has been returned and is back in stock",
                        Book = book
                    };

                    await _webPubSubClient.SendToUserAsync(
                        book.OwnerId,
                        JsonSerializer.Serialize(ownerNotification),
                        Azure.Core.ContentType.ApplicationJson);

                    // Notify users on the watchlist
                    var query = new QueryDefinition("SELECT * FROM c WHERE c.BookId = @bookId")
                        .WithParameter("@bookId", book.Id);

                    var iterator = _watchListContainer.GetItemQueryIterator<WatchList>(query);
                    var watchList = new List<WatchList>();

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        watchList.AddRange(response);
                    }

                    _logger.LogInformation($"Notifying {watchList.Count} users on watchlist for book {book.Name}");

                    foreach (var watch in watchList)
                    {
                        var watcherNotification = new Notification
                        {
                            Type = "BookAvailable",
                            Message = $"The book '{book.Name}' by {book.Author} is now available!",
                            Book = book
                        };

                        await _webPubSubClient.SendToUserAsync(
                            watch.UserId,
                            JsonSerializer.Serialize(watcherNotification),
                            Azure.Core.ContentType.ApplicationJson);
                    }
                }
                else
                {
                    _logger.LogInformation($"Book {book.Name} has been borrowed");

                    // Notify owner when book is borrowed
                    var notification = new Notification
                    {
                        Type = "BookBorrowed",
                        Message = $"Your book '{book.Name}' has been borrowed",
                        Book = book
                    };

                    await _webPubSubClient.SendToUserAsync(
                        book.OwnerId,
                        JsonSerializer.Serialize(notification),
                        Azure.Core.ContentType.ApplicationJson);
                }
            }
        }
    }
}