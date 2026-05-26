# Testing Strategy

## Purpose

This document defines the planned backend testing structure for AdventureWorksAIWorkspace.

## Test Projects

The API solution uses layered test projects under:

```txt
source/AdventureWorksAIWorkspaceAPI/tests/
  Unit.Tests/
  Application.Tests/
  Functional.Tests/
  Integration.Tests/
  Architecture.Tests/
```

All test projects use:

- xUnit for test execution.
- NSubstitute for mocks and substitutes.
- AwesomeAssertions for readable assertions.

## Unit Tests

Unit tests should cover isolated domain and service behavior without external systems.

Suggested scope:

- Domain entities and value objects.
- Pure business rules and invariants.
- SQL safety rule helpers.
- Prompt parsing helpers, if implemented as deterministic services.
- Small infrastructure adapters with mocked dependencies.

## Application Tests

Application tests should cover use cases and CQRS behavior at the application layer.

Suggested scope:

- Command and query handlers.
- FluentValidation validators.
- Wolverine command/query dispatch behavior where useful.
- Mapster DTO mapping rules.
- Application service registration.
- Error and result models returned by use cases.

## Functional Tests

Functional tests should cover API behavior through an in-memory ASP.NET Core test host.

Suggested scope:

- Wolverine HTTP endpoints.
- Request and response contracts.
- ProblemDetails validation responses.
- Authentication and authorization behavior.
- API middleware behavior.
- Basic smoke tests for the HTTP host.

## Integration Tests

Integration tests should cover real infrastructure boundaries or close substitutes.

Suggested scope:

- Application database persistence.
- AdventureWorks read-only database access.
- SQL query execution and result shaping.
- External AI client integration through controlled test doubles.
- Export provider integration.
- Testcontainers-based SQL Server scenarios when database behavior matters.

## Architecture Tests

Architecture tests should protect project structure and dependency rules.

Suggested scope:

- Domain must not depend on Application, Infrastructure, or Api.
- Application must not depend on Infrastructure or Api.
- Infrastructure must not depend on Api.
- Feature naming conventions.
- Endpoint, command, query, handler, validator, and DTO placement conventions.
- Accidental cross-layer references.
