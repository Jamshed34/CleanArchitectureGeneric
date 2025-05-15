# Clean Architecture Template with IdentityServer4 & .NET Aspire

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![IdentityServer4](https://img.shields.io/badge/IdentityServer-4-lightgrey)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green)
![CQRS](https://img.shields.io/badge/Pattern-CQRS-orange)
![License](https://img.shields.io/badge/License-MIT-brightgreen)

A production-ready template implementing Clean Architecture with:
- IdentityServer4 authentication
- CQRS pattern with MediatR  
- .NET Aspire for orchestration
- SQL Server database
- Complete user management

## Table of Contents
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Authentication Flow](#authentication-flow)
- [Testing](#testing)
- [Database Schema](#database-schema)
- [Contributing](#contributing)
- [License](#license)

---

## Project Structure
src/
├── AppHost/ # .NET Aspire orchestration
│ ├── Program.cs # Service definitions
│ └── appsettings.json # Orchestration config
├── Presentation/
│ └── WebApi/ # API Layer
│ ├── Controllers/ # API endpoints
│ ├── appsettings.json # API configuration
│ └── Program.cs # Startup
├── Application/ # Business Logic
│ ├── Commands/ # CQRS Commands
│ ├── Queries/ # CQRS Queries
│ ├── Models/ # DTOs
│ └── Mappings/ # AutoMapper
├── Domain/ # Core Domain
│ ├── Entities/ # Domain models
│ └── Interfaces/ # Contracts
└── Infrastructure/ # Implementation
├── Data/ # Database
├── Identity/ # Auth
└── Repositories/ # Data access

tests/
├── UnitTests/ # Unit tests
└── IntegrationTests/ # Integration tests


---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (or Docker)
- [Node.js](https://nodejs.org/) (optional for frontend)

---

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/clean-architecture-identityserver.git
   cd clean-architecture-identityserver

## Running the Application
1. Start all services:

bash
dotnet run --project src/AppHost

2. Access endpoints:


API: https://localhost:7042
Swagger UI: https://localhost:7042/swagger

