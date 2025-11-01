# Dynamic Book Tracker - Setup Guide

## Overview
A book lending tracking system built with:
- **Frontend**: Blazor WebAssembly
- **Backend**: Azure Functions (API)
- **Database**: Azure Cosmos DB
- **Real-time Notifications**: Azure Web PubSub

## Architecture
- Users can create books with basic information (Name, Author, Owner)
- Books can be borrowed by authorized users when in stock
- Book owners receive notifications when their books are borrowed/returned
- Users can watch out-of-stock books and get notified when they become available
- The CosmosDB trigger sends real-time notifications via Web PubSub

## Prerequisites
1. Azure account
2. Azure Static Web Apps (Standard tier)
3. Azure Cosmos DB account
4. Azure Web PubSub resource
5. .NET 9.0 SDK

## Azure Setup

### 1. Create Cosmos DB
1. Create an Azure Cosmos DB account (API: NoSQL)
2. Create a database named `BookTracker`
3. Create two containers:
   - `Books` (Partition key: `/id`)
   - `WatchList` (Partition key: `/id`)
4. Copy the connection string

### 2. Create Web PubSub
1. Create an Azure Web PubSub resource
2. Create a hub named `notifications`
3. Copy the connection string

### 3. Configure Static Web App
1. Create an Azure Static Web App (Standard tier)
2. Connect to your GitHub repository
3. Set build configuration:
   - App location: `Client`
   - Api location: `Api`
   - Output location: `wwwroot`

### 4. Add Application Settings
In your Static Web App configuration, add these settings:
- `CosmosDbConnectionString`: Your Cosmos DB connection string
- `CosmosDbDatabaseName`: `BookTracker`
- `WebPubSubConnectionString`: Your Web PubSub connection string
- `WebPubSubHubName`: `notifications`

## Local Development

### 1. Configure Local Settings
Edit `Api/local.settings.json`:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnectionString": "YOUR_COSMOS_DB_CONNECTION_STRING",
    "CosmosDbDatabaseName": "BookTracker",
    "WebPubSubConnectionString": "YOUR_WEB_PUBSUB_CONNECTION_STRING",
    "WebPubSubHubName": "notifications"
  }
}
```

### 2. Run Locally
```bash
# Start the API
cd Api
func start

# In a separate terminal, start the client
cd Client
dotnet run
```

## API Endpoints

### Books
- `GET /api/books` - Get all books
- `GET /api/books/{id}` - Get a specific book
- `POST /api/books` - Create a new book
- `PUT /api/books/{id}` - Update a book
- `DELETE /api/books/{id}` - Delete a book (owner only)
- `POST /api/books/{id}/borrow` - Borrow a book
- `POST /api/books/{id}/return` - Return a book

### Watch List
- `GET /api/watchlist` - Get user's watch list
- `POST /api/watchlist/{bookId}` - Add book to watch list
- `DELETE /api/watchlist/{bookId}` - Remove book from watch list

### Web PubSub
- `GET /api/webpubsub/negotiate` - Get WebSocket connection URL

## Notifications
The system sends real-time notifications for:
- **Book Borrowed**: Notifies the owner when their book is borrowed
- **Book Returned**: Notifies the owner when their book is returned
- **Book Available**: Notifies users on the watch list when a book becomes available

## Security
- All API endpoints require authentication (Azure Static Web Apps authentication)
- Users can only delete their own books
- Users can only borrow books that are in stock
- Book ownership is tracked and enforced

## Project Structure
```
DynamicBookTracker/
├── Api/                              # Azure Functions backend
│   ├── BookFunctions.cs             # Book CRUD operations
│   ├── WatchListFunctions.cs        # Watch list operations
│   ├── CosmosDbUpdates.cs           # Cosmos DB trigger for notifications
│   ├── WebPubSubConnectionFunction.cs # WebSocket connection endpoint
│   └── WebPubSub.cs                 # Web PubSub client wrapper
├── Client/                           # Blazor WebAssembly frontend
│   ├── Pages/
│   │   └── Books.razor              # Main book management page
│   ├── Layout/
│   │   └── MainLayout.razor         # Layout with notification toasts
│   ├── Services/
│   │   └── NotificationService.cs   # WebSocket notification handler
│   └── wwwroot/
│       └── js/
│           └── notifications.js     # JavaScript for WebSocket
└── Shared/                           # Shared models
    └── Models/
        ├── Book.cs
        ├── WatchList.cs
        └── Notification.cs
```

## Troubleshooting

### Cosmos DB Trigger Not Firing
- Ensure the `leases` container is created (it's auto-created)
- Check connection string in application settings
- Verify the container name matches exactly ("Books")

### Notifications Not Received
- Verify Web PubSub connection string is correct
- Check browser console for WebSocket connection errors
- Ensure user is authenticated
- Check that the hub name is "notifications"

### Authentication Issues
- Ensure Static Web Apps authentication is properly configured
- For local development, you may need to configure authentication providers
