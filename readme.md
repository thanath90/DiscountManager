# Introduction

This API is designed to provide users the capability to apply a discount on an order. The discount can be specified either as a fixed amount or as a percentage of the total order value. 
For this purpose , an endpoint was created that accepts the order details and the discount information, processes it, and returns the final amount after applying the discount.
Addittionally, the API manages the CRUD operations for the discounts table.


## Disclaimer
From this API logging is not implemented alongside the authentication and authorization mechanisms. It is recommended to implement these features in a production environment to ensure security and traceability. Finally is recommended a healthcheck controller to verify the health of the API alongside the external components (DB etc).

## Endpoints
- `POST /apply-discount`: Apply a discount to an order.
- `GET /discounts`: Retrieve all discounts.
- `GET /discounts/{id}`: Retrieve a specific discount by ID.
- `POST /discounts`: Create a new discount.
- `PUT /discounts/{id}`: Update an existing discount by ID.
- `DELETE /discounts/{id}`: Delete a discount by ID.

## Technologies Used
- ASP.NET Core Web API (.NET 8)
- Dapper ORM
- SQLLite Database (for simplicity and for this to be self suficiant and run without the need of external connections; can be replaced with any other relational database)
- Swagger for API documentation and testing
- xUnit for unit testing
- NSubstitute for mocking dependencies in unit tests
- PlantUML for architecture diagrams

## Available DiscountTypes:
1. Percentage
2. FixedAmount

## Initial Seed
Table columns:
  (Name, Type, Priority)
Values:
1. ('Price List Discount', Percentage, 1),
2. ('Promotion', Percentage, 2)
