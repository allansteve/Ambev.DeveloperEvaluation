# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
- `dotnet restore` - Restore NuGet packages for all projects
- `dotnet build Ambev.DeveloperEvaluation.sln --configuration Release --no-restore` - Build the entire solution
- `dotnet run --project src/Ambev.DeveloperEvaluation.WebApi` - Run the Web API locally
- `docker-compose up` - Run the application with all dependencies (PostgreSQL, MongoDB, Redis) using Docker

### Testing
- `dotnet test Ambev.DeveloperEvaluation.sln --no-restore --verbosity normal` - Run all tests
- `dotnet test tests/Ambev.DeveloperEvaluation.Unit --no-restore` - Run unit tests only
- `dotnet test tests/Ambev.DeveloperEvaluation.Integration --no-restore` - Run integration tests only
- `dotnet test tests/Ambev.DeveloperEvaluation.Functional --no-restore` - Run functional tests only

### Coverage Reports
- `./coverage-report.sh` (Linux/macOS) or `coverage-report.bat` (Windows) - Generate test coverage reports
- Coverage report will be available at `TestResults/CoverageReport/index.html`

### Database
- EF Core migrations are in `src/Ambev.DeveloperEvaluation.ORM/Migrations/`
- Connection string configured for PostgreSQL by default
- Use `dotnet ef migrations add <name> --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi` to add migrations

## Architecture Overview

This project follows **Clean Architecture** principles with **Hexagonal Architecture** concepts and is organized into the following layers:

### Core Layer (Business Logic)
- **Domain** (`Ambev.DeveloperEvaluation.Domain`) - Domain entities, value objects, domain services, and business rules
- **Application** (`Ambev.DeveloperEvaluation.Application`) - Use cases, commands/queries handlers using MediatR pattern

### Infrastructure Layer (External Concerns)
- **ORM** (`Ambev.DeveloperEvaluation.ORM`) - Entity Framework Core implementation, database context, repositories
- **Common** (`Ambev.DeveloperEvaluation.Common`) - Cross-cutting concerns (logging, security, validation, health checks)

### Presentation Layer (External Interface)
- **WebApi** (`Ambev.DeveloperEvaluation.WebApi`) - REST API controllers, request/response models, middleware

### Configuration Layer
- **IoC** (`Ambev.DeveloperEvaluation.IoC`) - Dependency injection configuration and module initializers

## Key Patterns and Technologies

### CQRS with MediatR
- Commands and queries are separated and handled through MediatR pipeline
- Each use case has its own handler, validator, and profile for mapping
- Pipeline behaviors for cross-cutting concerns (validation, logging)

### Domain-Driven Design
- Rich domain entities with business logic and validation
- Domain events and specifications pattern
- Repository pattern for data access abstraction

### Security
- JWT authentication configured in `Ambev.DeveloperEvaluation.Common.Security`
- BCrypt password hashing
- User roles and permissions system

### Validation
- FluentValidation used throughout the application
- Domain entity validation and request validation
- Custom validation behaviors in the MediatR pipeline

### Testing Stack
- **xUnit** - Primary testing framework
- **FluentAssertions** - Fluent assertion library
- **NSubstitute** - Mocking framework
- **Bogus** - Test data generation
- **Coverlet** - Code coverage analysis

## Database Configuration

### Default Setup
- PostgreSQL database with connection string in `appsettings.json`
- Default database: `DeveloperEvaluation`
- EF Core with code-first migrations

### Docker Development
- PostgreSQL on port 5432 (database: `developer_evaluation`, user: `developer`)
- MongoDB on port 27017 (for future NoSQL features)
- Redis on port 6379 (for caching)

## Project Structure Notes

### Feature Organization
- WebApi features are organized by domain (Users, Auth)
- Each feature contains its own controllers, requests, responses, and profiles
- CQRS pattern separates read and write operations

### Dependency Flow
- Domain layer has no dependencies
- Application layer depends only on Domain
- Infrastructure layers depend on Application and Domain
- WebApi depends on all layers through IoC configuration

### Module Initialization
Dependencies are registered through module initializers:
- `ApplicationModuleInitializer` - Application layer services
- `InfrastructureModuleInitializer` - Infrastructure services and repositories
- `WebApiModuleInitializer` - Web API specific services

## Code Style
- Primary constructors are disabled (`.editorconfig` setting)
- Standard C# naming conventions
- Comprehensive XML documentation on public APIs
- Nullable reference types enabled