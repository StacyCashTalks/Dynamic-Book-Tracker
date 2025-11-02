using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;
using StacyClouds.SwaAuth.Api;
using System.Text.Json;

namespace Api;

public class BookFunctions
{
    private readonly ILogger<BookFunctions> _logger;
    private readonly Container _booksContainer;

    public BookFunctions(ILogger<BookFunctions> logger, IConfiguration configuration)
    {
        _logger = logger;

        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };
        var cosmosClient = new CosmosClient(configuration["CosmosDbConnectionString"], options);
        var databaseName = configuration["CosmosDbDatabaseName"] ?? "BookTracker";
        var containerName = configuration["CosmosDbContainerName"] ?? "Books";
        var database = cosmosClient.GetDatabase(databaseName);

        _booksContainer = database.GetContainer(containerName);
    }

    [Function("GetBooks")]
    public async Task<IActionResult> GetBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books")] HttpRequest req)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _booksContainer.GetItemQueryIterator<Book>(query);

        var books = new List<Book>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            books.AddRange(response);
        }

        return new OkObjectResult(books);
    }

    [Function("GetBook")]
    public async Task<IActionResult> GetBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/{id}/{owner}")] HttpRequest req,
        string id)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        try
        {
            var response = await _booksContainer.ReadItemAsync<Book>(id, new PartitionKey(user!.UserId));
            return new OkObjectResult(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }
    }

    [Function("CreateBook")]
    public async Task<IActionResult> CreateBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books")] HttpRequest req)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        var book = await JsonSerializer.DeserializeAsync<Book>(req.Body, JsonSettings.CamelCaseOptions);
        if (book == null)
        {
            return new BadRequestObjectResult("Invalid book data");
        }

        book.Id = Guid.NewGuid().ToString();
        book.OwnerId = user!.UserId!;

        var response = await _booksContainer.CreateItemAsync(book, new PartitionKey(book.OwnerId));
        return new CreatedResult($"/api/books/{book.Id}", response.Resource);
    }

    [Function("UpdateBook")]
    public async Task<IActionResult> UpdateBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "books/{id}")] HttpRequest req,
        string id)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        try
        {
            var existingBook = await _booksContainer.ReadItemAsync<Book>(id, new PartitionKey(user!.UserId));

            var updatedBook = await JsonSerializer.DeserializeAsync<Book>(req.Body, JsonSettings.CamelCaseOptions);
            if (updatedBook == null)
            {
                return new BadRequestObjectResult("Invalid book data");
            }

            // Keep the same ID and OwnerId
            updatedBook.Id = id;
            updatedBook.OwnerId = existingBook.Resource.OwnerId;

            var response = await _booksContainer.ReplaceItemAsync(updatedBook, id, new PartitionKey(updatedBook.OwnerId));
            return new OkObjectResult(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }
    }

    [Function("BorrowBook")]
    public async Task<IActionResult> BorrowBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books/{id}/{owner}/borrow")] HttpRequest req,
        string id, string owner)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        try
        {
            var response = await _booksContainer.ReadItemAsync<Book>(id, new PartitionKey(owner));
            var book = response.Resource;

            if (!book.InStock)
            {
                return new BadRequestObjectResult("Book is not available");
            }

            book.InStock = false;
            book.LoanedToUserId = user!.UserId!;

            var updateResponse = await _booksContainer.ReplaceItemAsync(book, id, new PartitionKey(book.OwnerId));
            return new OkObjectResult(updateResponse.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }
    }

    [Function("ReturnBook")]
    public async Task<IActionResult> ReturnBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books/{id}/{owner}/return")] HttpRequest req,
        string id, string owner)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        try
        {
            var response = await _booksContainer.ReadItemAsync<Book>(id, new PartitionKey(owner));
            var book = response.Resource;

            if (book.InStock)
            {
                return new BadRequestObjectResult("Book is already in stock");
            }

            if (book.LoanedToUserId != user!.UserId && book.OwnerId != user.UserId)
            {
                return new UnauthorizedObjectResult("You cannot return this book");
            }

            book.InStock = true;
            book.LoanedToUserId = null;

            var updateResponse = await _booksContainer.ReplaceItemAsync(book, id, new PartitionKey(book.OwnerId));
            return new OkObjectResult(updateResponse.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }
    }

    [Function("DeleteBook")]
    public async Task<IActionResult> DeleteBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "books/{id}")] HttpRequest req,
        string id)
    {
        if (!StaticWebAppApiAuthentication.TryParseHttpHeaderForClientPrincipal(req.Headers, out var user))
        {
            return new UnauthorizedResult();
        }

        try
        {
            var response = await _booksContainer.ReadItemAsync<Book>(id, new PartitionKey(user!.UserId));
            var book = response.Resource;

            if (book.OwnerId != user!.UserId)
            {
                return new UnauthorizedObjectResult("You can only delete your own books");
            }

            await _booksContainer.DeleteItemAsync<Book>(id, new PartitionKey(book.OwnerId));
            return new NoContentResult();
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new NoContentResult();
        }
    }
}
