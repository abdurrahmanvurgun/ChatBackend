# ChatBackend

## Overview
ChatBackend is a messaging system backend that provides APIs for managing messages between users. It is designed to work seamlessly with frontend and mobile applications.

## Project Structure
- **Controllers**: Contains the `MessageController` which handles HTTP requests related to messages.
- **Data**: Contains the `MessagingContext` which interacts with the database.
- **Models**: Contains the `Message` model representing the message entity.
- **Services**: Contains the `IMessageService` interface and its implementation `MessageService` for message operations.
- **Properties**: Contains launch settings for different environments.
- **Configuration Files**: Includes `appsettings.Development.json` and `appsettings.json` for application settings.

## Setup Instructions
1. Clone the repository:
   ```
   git clone <repository-url>
   ```
2. Navigate to the project directory:
   ```
   cd ChatBackend
   ```
3. Restore the dependencies:
   ```
   dotnet restore
   ```
4. Configure PostgreSQL connection in `appsettings.json` (example shown there).
5. Apply EF Core migrations and run the application:
   ```bash
   # create a migration (if you change models)
   dotnet ef migrations add InitialCreate --project ChatBackend.csproj
   # apply migrations to the PostgreSQL database
   dotnet ef database update --project ChatBackend.csproj
   # run the app
   dotnet run --project ChatBackend.csproj
   ```

### Docker helper (PostgreSQL + migrator)
If you don't want to install PostgreSQL locally, you can use the included Docker Compose that starts PostgreSQL 18 and a temporary .NET SDK container to run migrations:

```bash
# start db and run migrations (uses defaults from appsettings.json)
docker compose up --build

# To stop and remove containers
docker compose down
```

You can also install the EF Core CLI locally with the helper script:

```bash
chmod +x scripts/install-ef.sh
./scripts/install-ef.sh
```

## Usage
- The API provides endpoints for sending, retrieving, and deleting messages.
- Use tools like Postman or Swagger UI to interact with the API.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request for any enhancements or bug fixes.