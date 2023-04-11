# SecureCustomerFeedback API

The Customer Feedback API is an ASP.NET Core API that provides endpoints for submitting and retrieving customer feedback. It uses .NET7, Azure Key Vault, EntityFramework, and Sql Server to securely store and manage customer feedback data.

## Technologies Used

- ASP.NET Core
- .NET7
- Azure Key Vault
- EntityFramework
- Sql Server

## Purpose

The purpose of this API is to provide a secure and reliable way for businesses to collect and manage customer feedback. By using Azure Key Vault to manage the encryption and decryption of sensitive data, this API ensures that customer feedback is protected from unauthorized access and stored securely.

## Installation

To install and run this API, follow these steps:

Clone the repository to your local machine
Open the project in Visual Studio 2022 or later
Create an Azure Key Vault and update the appsettings.json file with your vault's ClientId, ClientSecret, TenantId, and VaultUri
Create a Sql Server database and update the appsettings.json file with your database's connection string
Run the project and test the API using a tool like Postman or Swagger.

## Usage

The following endpoints are available in the Customer Feedback API:
 
GET /feedback/{id} - Retrieves a specific customer feedback item by ID
POST /feedback - Submits a new customer feedback item

## License

GPL 2.0 License. 

## Security
This API uses a two-tier encryption approach to ensure that customer feedback data is protected at all times. The first tier is a data encryption key (DEK) that is generated for each customer based on their email address. The DEK is then encrypted using a master encryption key (MEK) stored in Azure Key Vault. When a customer submits feedback, the API uses their DEK to encrypt the feedback data before storing it in the database. When feedback is retrieved, the API uses the customer's DEK and the MEK to decrypt the feedback data and return it in plain text.
