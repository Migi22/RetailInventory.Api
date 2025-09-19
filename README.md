# 🛒 Retail Inventory Management - ASP.NET Core Web API

A RESTful API built with **ASP.NET Core 8.0** to manage retail inventory data.  
This backend powers the WPF client app and supports **JWT-based authentication**. 

🚧 *Note: This project is still in active development.*

---

## ✨ Features

- ✅ JWT Authentication (login & token generation)
- ✅ SQL Server integration
- ✅ Entity Framework Core migrations
- ✅ Role-based authorization (Admin, Staff, Owner)
- ✅ CRUD APIs for Products, Categories, Stores
- 🚧 Unit & integration testing
- 🚧 CI/CD pipeline with Docker & GitHub Actions
- ✅ Postman collection for automated API testing

---

## 🛠 Tech Stack

- **Framework**: ASP.NET Core 8 Web API  
- **Database**: SQL Server 2022  
- **ORM**: Entity Framework Core  
- **Authentication**: JWT tokens  
- **Testing**: Postman + Newman (CI/CD)  
- **CI/CD**: GitHub Actions + Docker Compose  


---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download)
- [SQL Server 2022](https://www.microsoft.com/en-us/sql-server)  

### Run the API locally
```sh
cd RetailInventory.Api
dotnet restore
dotnet ef database update
dotnet run


