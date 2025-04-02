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
4. Run the application:
   ```
   dotnet run
   ```

## Usage
- The API provides endpoints for sending, retrieving, and deleting messages.
- Use tools like Postman or Swagger UI to interact with the API.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request for any enhancements or bug fixes.