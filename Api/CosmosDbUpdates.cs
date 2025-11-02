using System.Text.Json;
using Azure.Messaging.WebPubSub;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Api;

public class CosmosDbUpdates
{
    private readonly ILogger<CosmosDbUpdates> _logger;
    private readonly WebPubSubServiceClient _webPubSubClient;

    public CosmosDbUpdates(ILogger<CosmosDbUpdates> logger, WebPubSub webPubSub)
    {
        _logger = logger;
        _webPubSubClient = webPubSub.Client;
    }

    [Function("BookUpdates")]
    public async Task Run([CosmosDBTrigger(
        databaseName: "%CosmosDbDatabaseName%",
        containerName: "%CosmosDbBooksContainerName%",
        Connection = "CosmosDbConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Book> input)
    {
        if (input.Count > 0)
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

                    // Notify all users watching this book via group message
                    var watcherNotification = new Notification
                    {
                        Type = "BookAvailable",
                        Message = $"The book '{book.Name}' by {book.Author} is now available!",
                        Book = book
                    };

                    await _webPubSubClient.SendToGroupAsync(
                        book.Id,
                        JsonSerializer.Serialize(watcherNotification),
                        Azure.Core.ContentType.ApplicationJson);

                    _logger.LogInformation($"Sent availability notification to group {book.Id} for book {book.Name}");
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