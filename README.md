# DeveloperStore Sales API

A comprehensive .NET 8 Web API for managing sales records, implementing Domain-Driven Design (DDD) principles with the External Identities pattern for cross-domain entity references.

## Overview

This API provides complete CRUD operations for sales management with the following capabilities:

### Sales Features
- **Sale Management**: Create, read, update, and delete sales records
- **Customer Management**: Handle customer information and relationships
- **Product Catalog**: Manage products with pricing and inventory
- **Discount Engine**: Automatic quantity-based discount calculations
- **Branch Operations**: Multi-location sales tracking
- **Status Management**: Track cancelled/active sales and items

### Business Rules
The system implements specific quantity-based discounting tiers:

- **4-9 items**: 10% discount applied automatically
- **10-20 items**: 20% discount applied automatically
- **Maximum limit**: 20 identical items per product
- **No discounts**: Applied for quantities below 4 items

### Event Publishing
The API publishes domain events for:
- `SaleCreated` - When a new sale is recorded
- `SaleModified` - When sale details are updated
- `SaleCancelled` - When a sale is cancelled
- `ItemCancelled` - When individual items are cancelled

## Prerequisites

Before running this project, ensure you have the following installed:

- **.NET 8.0 SDK** or later
- **Docker Desktop** (for containerized setup)
- **PostgreSQL 13+** (for local development without Docker)
- **Git** for version control

## Tech Stack

### Backend
- **.NET 8** - Core framework
- **ASP.NET Core Web API** - REST API framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **MediatR** - CQRS and mediator pattern implementation
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

### Database & Storage
- **PostgreSQL 13** - Primary relational database
- **MongoDB 8.0** - Document database for NoSQL operations
- **Redis 7.4** - Caching and session storage

### Testing
- **xUnit** - Unit testing framework
- **FluentAssertions** - Assertion library
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

### DevOps & Tools
- **Docker** - Containerization
- **Swagger/OpenAPI** - API documentation
- **Health Checks** - Application monitoring

## Project Structure

```
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Application/    # Application layer (CQRS, handlers)
│   ├── Ambev.DeveloperEvaluation.Common/         # Shared utilities and extensions
│   ├── Ambev.DeveloperEvaluation.Domain/         # Domain entities, services, events
│   ├── Ambev.DeveloperEvaluation.IoC/            # Dependency injection configuration
│   ├── Ambev.DeveloperEvaluation.ORM/            # Data access layer with EF Core
│   └── Ambev.DeveloperEvaluation.WebApi/         # Web API controllers and configuration
└── tests/
    ├── Ambev.DeveloperEvaluation.Functional/     # End-to-end functional tests
    ├── Ambev.DeveloperEvaluation.Integration/    # Integration tests
    └── Ambev.DeveloperEvaluation.Unit/           # Unit tests
```

## Configuration

### Environment Variables

The application uses the following environment variables:

```bash
# Database Configuration
ConnectionStrings__DefaultConnection="Host=localhost;Database=developer_evaluation;Username=developer;Password=Pass@word"

# JWT Authentication
Jwt__SecretKey="YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong"

# ASP.NET Core
ASPNETCORE_ENVIRONMENT="Development"
ASPNETCORE_HTTP_PORTS="8080"
ASPNETCORE_HTTPS_PORTS="8081"
```

### Database Credentials

**PostgreSQL:**
- Database: `developer_evaluation`
- Username: `developer`
- Password: `ev@luAt10n` (Docker) / `Pass@word` (Local)

**MongoDB:**
- Username: `developer`
- Password: `ev@luAt10n`

**Redis:**
- Password: `ev@luAt10n`

## Getting Started

### Option 1: Docker Setup (Recommended)

1. **Clone the repository**
   ```bash
   git clone <your-repository-url>
   cd abi-gth-omnia-developer-evaluation/template/backend
   ```

2. **Start all services with Docker Compose**
   ```bash
   docker-compose up -d
   ```

   This will start:
   - Web API on `http://localhost:8080` and `https://localhost:8081`
   - PostgreSQL on `localhost:5432`
   - MongoDB on `localhost:27017`
   - Redis on `localhost:6379`

3. **Verify the application is running**
   ```bash
   # Check health status
   curl http://localhost:8080/health
   
   # Access Swagger documentation
   # Open: http://localhost:8080/swagger
   ```

### Option 2: Local Development Setup

1. **Setup PostgreSQL Database**
   ```sql
   CREATE DATABASE developer_evaluation;
   CREATE USER developer WITH ENCRYPTED PASSWORD 'Pass@word';
   GRANT ALL PRIVILEGES ON DATABASE developer_evaluation TO developer;
   ```

2. **Update Connection String**
   
   Ensure `appsettings.json` has the correct connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=developer_evaluation;Username=developer;Password=Pass@word"
     }
   }
   ```

3. **Run Database Migrations**
   ```bash
   cd src/Ambev.DeveloperEvaluation.WebApi
   dotnet ef database update
   ```

4. **Start the Application**
   ```bash
   dotnet run
   ```

   The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

## Running Tests

### Run All Tests
```bash
# From the backend directory
dotnet test
```

### Run Specific Test Categories
```bash
# Unit tests only
dotnet test tests/Ambev.DeveloperEvaluation.Unit/

# Integration tests only
dotnet test tests/Ambev.DeveloperEvaluation.Integration/

# Functional tests only
dotnet test tests/Ambev.DeveloperEvaluation.Functional/
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## API Documentation

### Swagger UI
When running in Development mode, access interactive API documentation at:
- **Local**: `https://localhost:5001/swagger`
- **Docker**: `http://localhost:8080/swagger`

### Available Endpoints

#### Authentication (`/api/auth`)
- `POST /api/auth/login` - User authentication
- `POST /api/auth/register` - User registration

#### Sales Management (`/api/sales`)
- `GET /api/sales` - List all sales
- `GET /api/sales/{id}` - Get sale by ID
- `POST /api/sales` - Create new sale
- `PUT /api/sales/{id}` - Update existing sale
- `DELETE /api/sales/{id}` - Cancel sale

#### User Management (`/api/users`)
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Deactivate user

### Health Checks
- `GET /health` - Application health status
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

## Development Guidelines

### Architecture Principles
- **Domain-Driven Design (DDD)**: Clear separation of domain, application, and infrastructure layers
- **CQRS Pattern**: Command and Query Responsibility Segregation using MediatR
- **External Identities Pattern**: Denormalized entity references across domains
- **Clean Architecture**: Dependency inversion and separation of concerns

### Code Standards
- Follow C# coding conventions
- Use async/await for I/O operations
- Implement comprehensive validation using FluentValidation
- Write unit tests for all business logic
- Use structured logging with Serilog

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/Ambev.DeveloperEvaluation.ORM

# Update database
dotnet ef database update --project src/Ambev.DeveloperEvaluation.WebApi
```

## Troubleshooting

### Common Issues

**Database Connection Issues**
- Verify PostgreSQL is running and accessible
- Check connection string in `appsettings.json`
- Ensure database and user exist with proper permissions

**Docker Issues**
- Ensure Docker Desktop is running
- Check port conflicts (8080, 5432, 27017, 6379)
- Try `docker-compose down` and `docker-compose up -d` to restart

**Migration Issues**
- Ensure connection string is correct
- Run `dotnet ef database drop` and `dotnet ef database update` to reset

**JWT Authentication Issues**
- Verify JWT secret key length (minimum 32 bytes)
- Check token expiration settings
- Ensure proper Authorization header format: `Bearer <token>`

### Logs and Debugging
- Application logs are written to console and files
- Use structured logging queries to filter specific events
- Enable detailed Entity Framework logging in Development mode

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is part of a developer evaluation and is intended for assessment purposes.

---

For additional support or questions, please refer to the project documentation or contact the development team.