# Azure Functions REST API for Business KPIs

## Overview
This repository contains an Azure Functions REST API designed to interact with a SQL database. The API serves as a data provider for a dashboard, enabling users to manipulate and visualize various business KPIs. Additionally, the system supports the creation of VR visualizations from these KPIs.

## Features
- **SQL Database Integration**: Securely fetch, update, and delete KPI data.
- **RESTful Endpoints**: A set of endpoints for managing and retrieving business KPIs.
- **Dashboard Support**: Provides data endpoints tailored for dashboard visualizations.
- **VR Visualizations**: Exposes data in formats suitable for VR visualization tools.

## Getting Started

### Prerequisites
1. **Azure Account**: Ensure you have an active Azure subscription.
2. **SQL Database**: A pre-configured SQL database with necessary tables and data.
3. **Tools**:
   - [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) for local development.
   - [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) for deployment.
   - [.NET SDK](https://dotnet.microsoft.com/download) for Azure Functions development.

### Setup
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

### Deployment
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

## Endpoints
For more details, please refer to the [API_Tests.json](API_Tests.json) file.
### User Management

-   **`POST` /api/User/Login**
    -   Body: `{"Email": "", "Password": ""}`
-   **`POST` /api/User/Add**
    -   Body: `{"Name": "", "Email": "", "Password": "", "Username": "", "Salt": ""}`
-   **`GET` /api/User/GetAll**
-   **`GET` /api/User/Get/{id}**
-   **`GET` /api/User/GetByEmail** (requires `email` query parameter)
-   **`DELETE` /api/User/Delete/{id}**
-   **`PUT` /api/User/UpdateById/{id}**
    -   Body: `{"Name": "", "Tel": "", "Photo": ""}`
-   **`GET` /api/User/CheckToken**

### Company Management

#### Departments:
-   **`POST` /api/departments**
    -   Body: `{"name": "", "description": "", "managerId": ""}`
-   **`GET` /api/departments**
-   **`GET` /api/departments/{id}**
-   **`PUT` /api/departments/{id}**
    -   Body: `{"name": "", "description": "", "managerId": ""}`
-   **`DELETE` /api/departments/{id}**
-   **`POST` /api/departments/{departmentId}/kpis/{kpiId}**
-   **`DELETE` /api/departments/{departmentId}/kpis/{kpiId}**

#### KPIs:
-   **`POST` /api/kpis**
    -   Body: `{"name": "", "description": "", "target": "", "unit": "", "frequency": ""}`
-   **`GET` /api/kpis**
-   **`GET` /api/kpis/{id}**
-   **`PUT` /api/kpis/{id}**
    -   Body: `{"Value_1": "", "Value_2": ""}`
-   **`DELETE` /api/kpis/{id}**

#### Roles & Permissions:
-   **`POST` /api/roles**
    -   Body: `{"name": "", "description": ""}`
-   **`GET` /api/roles**
-   **`GET` /api/roles/{id}**
-   **`PUT` /api/roles/{id}**
    -   Body: `{"name": "", "description": ""}`
-   **`DELETE` /api/roles/{id}**
-   **`PUT` /api/roles/{roleId}/departments/{departmentId}/permissions**
    -   Body: `{"canRead": "", "canWrite": ""}`

### Room Management

-   **`POST` /api/Room/Add**
    -   Body: `{"Id": "", "JsonData": ""}`
-   **`GET` /api/Room/Get/{id}**

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

## Estructured Data
Our project is structured based on the Domain-Driven Design (DDD) approach, and the code is organized into the following folders:
- **Controllers**: Contains the API controllers that handle incoming requests and responses.
- **Services**: Contains the business logic and service classes that interact with the database.
- **Models**: Contains the data models and DTOs (Data Transfer Objects) used in the application.
