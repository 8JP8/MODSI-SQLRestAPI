# Azure Functions REST API for Business KPIs ✨

## Overview 🌟
This repository contains an Azure Functions REST API designed to interact with a SQL database. The API serves as a data provider for a dashboard, enabling users to manipulate and visualize various business KPIs. Additionally, the system supports the creation of VR visualizations from these KPIs.

## Features 📂
- **SQL Database Integration**: Securely fetch, update, and delete KPI data.
- **RESTful Endpoints**: A set of endpoints for managing and retrieving business KPIs.
- **Dashboard Support**: Provides data endpoints tailored for dashboard visualizations.
- **VR Visualizations**: Exposes data in formats suitable for VR visualization tools.

## Getting Started 🚀

### Prerequisites 🛠️
1. **Azure Account**: Ensure you have an active Azure subscription.
2. **SQL Database**: A pre-configured SQL database with necessary tables and data.
3. **Tools**:
   - [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) for local development.
   - [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) for deployment.
   - [.NET SDK](https://dotnet.microsoft.com/download) for Azure Functions development.

### Setup ⚙️
1. Setup a Azure MS SQL Database
   - You can use the [DB Structure](/Database/DB_Structure.sql) SQL file to set it up correctly.
2. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/azure-functions-kpi-api.git
   cd azure-functions-kpi-api
   ```

3. Install dependencies:
   ```bash
   dotnet restore
   ```

4. Configure your local settings:
   - Edit `local.settings.json`:
     ```json
     {
       "IsEncrypted": false,
       "Values": {
         "AzureWebJobsStorage": "<Your Azure Storage Connection String>",
         "FUNCTIONS_WORKER_RUNTIME": "dotnet",
         "SqlConnectionString": "<Your SQL Database Connection String>"
       }
     }
     ```

5. Run the functions locally:
   ```bash
   func start
   ```

### Deployment 🚀
1. Login to Azure CLI:
   ```bash
   az login
   ```

2. Create an Azure Function App:
   ```bash
   az functionapp create --resource-group <ResourceGroupName> --consumption-plan-location <Region> --runtime dotnet --functions-version 4 --name <AppName> --storage-account <StorageAccountName>
   ```

3. Deploy the application:
   ```bash
   func azure functionapp publish <AppName>
   ```

## Endpoints 📡

### Pie Chart Management 🥧
- **POST** `/api/PieChart/Add`
  - Add a new pie chart.
- **DELETE** `/api/PieChart/Delete/{id:int}`
  - Delete a pie chart by ID.
- **GET** `/api/PieChart/GetAll`
  - Fetch all pie charts.
- **GET** `/api/PieChart/Get/{id:int}`
  - Retrieve a specific pie chart by ID.
- **PUT** `/api/PieChart/Replace/{id:int}`
  - Replace a pie chart by ID.
- **POST** `/api/PieChart/SendRandom`
  - Send random pie charts.

### 3D Points Management 🎯
- **POST** `/api/3DPoints/Add`
  - Add a new 3D point.
- **DELETE** `/api/3DPoints/Delete/{id:int}`
  - Delete a 3D point by ID.
- **GET** `/api/3DPoints/GetAll`
  - Fetch all 3D points.
- **GET** `/api/3DPoints/Get/{id:int}`
  - Retrieve a specific 3D point by ID.
- **PUT** `/api/3DPoints/Replace/{id:int}`
  - Replace a 3D point by ID.
- **POST** `/api/3DPoints/SendRandom`
  - Send random 3D points.

### User Management 👤
- **POST** `/api/User/Add`
  - Add a new user.
- **DELETE** `/api/User/Delete/{id:int}`
  - Delete a user by ID.
- **GET** `/api/User/GetAll`
  - Retrieve all users.
- **GET** `/api/User/GetByEmail`
  - Retrieve a user by email.
- **GET** `/api/User/Get/{id:int}`
  - Retrieve a user by ID.
- **GET** `/api/User/EmailExists`
  - Check if an email exists.
- **PUT** `/api/User/Update/{id:int}`
  - Update user details by ID.

## License 📜
This project is licensed under the MIT License. See the `LICENSE` file for details.

## Estructured Data 📊
Our project is structured based on the Domain-Driven Design (DDD) approach, and the code is organized into the following folders:
- **Controllers**: Contains the API controllers that handle incoming requests and responses.
- **Services**: Contains the business logic and service classes that interact with the database.
- **Models**: Contains the data models and DTOs (Data Transfer Objects) used in the application.

## We do not use ⚠:
- **WebSocket** - Socket connections are not used in this project, so it DOESN'T provide a persistent connection to the client (real time chats,etc)
- **Message-Oriented Middleware (MOM)** - We don’t use MOM in this project (monolithic system), so no asynchronous magic here — big load, big boom 💥!
