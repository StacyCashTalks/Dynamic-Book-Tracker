using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Api;

public class CosmosDbUpdates
{
    private readonly ILogger<CosmosDbUpdates> _logger;

    public CosmosDbUpdates(ILogger<CosmosDbUpdates> logger)
    {
        _logger = logger;
    }

    [Function("BookUpdates")]
    public Task Run([CosmosDBTrigger(
        databaseName: "%CosmosDbDatabaseName%",
        containerName: "%CosmosDbBooksContainerName%",
        Connection = "CosmosDbConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Book> input)
    {
        if (input.Count > 0)
        {
            _logger.LogInformation("Observed {Count} book document changes. Borrow and return realtime events are published by the API action handlers.", input.Count);
        }

        return Task.CompletedTask;
    }
}
