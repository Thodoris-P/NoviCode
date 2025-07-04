# NoviCode

## Currency Exchange & Wallet Management Service

The service is designed to facilitate secure and efficient currency transactions and wallet management for users.
## Gateway Library
- The gateway library provides a way of fetching and parsing ECB exchange rates as strongly-typed objects.
- It is fully designed to be an autonomous library that can be used in any .NET application.
  - It provides an interface for fetching the excange rates and a ServiceCollectionExtension for easy integration (registers the DI)
- The core project follows the adapter pattern to allow for easy integration with different data sources if need be.
  - It has its own Interface and models for the exchange rates.
  - And a ExchangeRatesProvider implementation (the adapter) that implements the interface and fetches the data from the ECB library.

## Background Worker
- A simple Quartz background job that relies only on the IExchangeRatesService interface runs every minute and updates the rates.
- The service implementation handles the fetching and the consecuent update of the data.
- There is also a cache decorator for the exchange rates repository that handles the caching of the exchange rates using IDistributedCache.
  - It returns the data when available in the cache, otherwise it fetches it from the database and updates the cache.
  - When the rates are updated, the cache is also updated.

## Wallet API
- The Wallet API is a simple REST API that allows users to manage their wallets and perform currency exchanges.
- More specifically, users can create wallets, check their balance, and adjust the balance.
- The API controller relies on the IWalletFacade interface to perform the operations.
  - The facade was created to decouple the currency conversion logic, and mapping, from the WalletService.
  - It also uses the Result<T> pattern to handle errors and return meaningful responses.
    - A custom mapper is used to map the IResult<T> to IActionResult for the API responses.
- The factory pattern is used on the WalletService to abstract the AdjustFunds strategy implementations and decouple the WalletService from the concrete implementations of each strategy.
- The Api also has an exception handler middleware that handles exceptions and returns a meaningful error response.
- Configuration classes were created using the IOptions<T> pattern to handle the configuration of the different services.

## Solution Structure
### NoviCode.Api
  - The API project that exposes the REST endpoints for the wallet management.
  - It contains the API-specific components such as controllers, middleware, and configuration classes.
### NoviCode.Core
  - The core project that contains the business logic and domain models.
### NoviCode.Gateway
  - The gateway library that provides the implementation for fetching and parsing ECB exchange rates.
### NoviCode.Data
  - The data access layer that contains the repositories, database context, caching, and migrations.
### NoviCode.Core.Tests
- The unit tests for the core project.

## 🚀 Getting Started

Follow these steps to run the NoviCode API locally and in Docker:

1. **Clone the repository**

   ```bash
   git clone https://github.com/your-org/novicode.git
   ```

2. **Configure your connection strings**

  - Open `appsettings.json` and update the `DatabaseSettings:ConnectionString` to:
    ```json
    "Server=sqlserver,1433;Database=NoviCode;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
    ```
  - Add the Redis settings:
    ```json
    "Redis": {
      "Configuration": "localhost",
      "InstanceName": "0"
    }
    ```
    - if you change any settings, make sure to update the `docker-compose.yml` file as well.
    

3. **Build and run with Docker Compose**

   ```bash
   docker-compose up --build
   ```

  - This will start three containers:
    - **api**: Your .NET Web API on [http://localhost:5000](http://localhost:5000)
    - **sqlserver**: SQL Server on port 1433
    - **redis**: Redis on port 6379

4. **Verify the API**

    - Once all services are up, open your browser or use `curl`:
      ```bash
      curl http://localhost:8080/api/health
      ```
    - You should receive a 200 response indicating the API is running.  

5. **Run migrations**  
   ```bash
   dotnet ef database update --project NoviCode.Data
   ```

You’re all set to develop and test the NoviCode API locally! Let me know if you encounter any issues.

## Important Notes
- After further research, it seems that the IDistributedCache does not support the GetOrCreate pattern to prevent cache-stampede.