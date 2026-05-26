# Technical Decisions

## Purpose

This document records important technical and product decisions for AdventureWorksAIWorkspace.

Use this document as a lightweight Architecture Decision Record log.

---

## Decision: Use separate databases for application data and AdventureWorks data

### Date

2026-05-26

### Status

Accepted

### Context

The application needs to store users, saved reports, generated SQL metadata, tags, favorites, and export history. It also needs to analyze Microsoft AdventureWorks business data.

Mixing application data with AdventureWorks analytical data would make responsibilities unclear and increase risk.

### Decision

Use two separate SQL Server databases:

1. Application database for application-owned data.
2. AdventureWorks database for business analysis data.

### Consequences

Benefits:

- Clear separation of responsibilities.
- Safer access control.
- Easier to keep AdventureWorks read-only.
- Better long-term maintainability.

Trade-offs:

- Requires managing two database connections.
- Requires clear infrastructure configuration.

---

## Decision: Use React, TypeScript, MUI, and MUI Charts for the frontend

### Date

2026-05-26

### Status

Proposed

### Context

The frontend needs a modern dashboard interface with sidebars, charts, tables, and chat-like interaction.

### Decision

Use React with TypeScript, Material UI, and MUI Charts.

### Consequences

Benefits:

- Strong TypeScript support.
- Mature component library.
- Consistent UI system.
- Built-in charting option through MUI Charts.

Trade-offs:

- MUI Charts may have limitations for advanced BI visualizations.
- Future custom visualizations may require additional chart libraries.

---

## Decision: Use .NET 10 REST API for the backend

### Date

2026-05-26

### Status

Proposed

### Context

The backend needs to handle authentication, report persistence, AI orchestration, SQL validation, SQL execution, and export logic.

### Decision

Use .NET 10 REST API as the backend technology.

### Consequences

Benefits:

- Strong ecosystem for APIs.
- Good SQL Server support.
- Good authentication and authorization support.
- Suitable for structured backend architecture.

Trade-offs:

- .NET 10 availability and project templates should be confirmed during implementation.
- Some libraries may need compatibility checks.

---

## Decision: Use CQRS-oriented backend project structure

### Date

2026-05-26

### Status

Accepted

### Context

The backend will need to coordinate AI-assisted report generation, SQL validation, query execution, persistence, report history, favorites, and export workflows. These operations have different command-style and query-style responsibilities.

### Decision

Use a CQRS-oriented backend structure with four projects:

1. `Domain`
2. `Application`
3. `Infrastructure`
4. `Api`

The dependency direction is:

- `Application` references `Domain`.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application` and `Infrastructure`.

### Consequences

Benefits:

- Clear separation between business concepts, use cases, infrastructure implementations, and HTTP endpoints.
- Easier testing of application logic without the API host.
- Better fit for future command/query handlers.
- Lower risk of leaking infrastructure details into domain logic.

Trade-offs:

- More projects and references to maintain.
- CQRS conventions must be documented and followed consistently.

---

## Decision: Use Wolverine, Wolverine HTTP, Mapster, FluentValidation, and AwesomeAssertions

### Date

2026-05-26

### Status

Accepted

### Context

The backend needs a low-ceremony way to dispatch commands and queries, expose HTTP endpoints, validate command/query inputs, map between domain/application models and DTOs, and write readable tests.

### Decision

Use the following libraries:

1. `WolverineFx` 6.x as the in-process mediator and future messaging foundation.
2. `WolverineFx.Http` 6.x for HTTP endpoint composition.
3. `Mapster` for DTO mapping.
4. `FluentValidation` for command/query validation through Wolverine's validation middleware.
5. `AwesomeAssertions` for expressive test assertions.

### Consequences

Benefits:

- CQRS handlers can stay small and convention-based.
- HTTP endpoints can be wired directly into Wolverine's execution pipeline.
- Validation rules can be expressed close to command/query models.
- Wolverine HTTP validation failures can be exposed as `ProblemDetails`.
- DTO mapping can be centralized and tested without manual boilerplate.
- Tests can use readable assertion syntax without relying on FluentAssertions 8+ commercial licensing.

Trade-offs:

- Wolverine relies on conventions and assembly scanning, so project structure must stay consistent.
- Runtime endpoint and handler discovery should be verified in integration tests.
- Validation execution must be wired into the command/query pipeline before business handlers depend on it.
- Mapster mapping rules must be documented when mappings become non-trivial.
- Wolverine 6 disallows service-location-based handlers by default, so handlers should avoid mapper implementations that require `IServiceProvider`.

---

## Decision: Use Serilog with Seq for API logging

### Date

2026-05-26

### Status

Accepted

### Context

The API needs structured logs for request diagnostics, Wolverine handler execution, AI workflow troubleshooting, SQL validation auditing, and future production observability.

### Decision

Use Serilog as the API logging provider and configure Seq as the local structured log sink.

The default Seq endpoint is:

```txt
http://localhost:5341
```

### Consequences

Benefits:

- Structured logs can be queried by properties such as application name, request path, status code, machine name, and thread id.
- Request logs are captured consistently through Serilog request logging middleware.
- Seq provides a local developer-friendly log viewer.

Trade-offs:

- Developers need a running Seq instance to see logs in Seq.
- Production environments must override the Seq endpoint and any required API key through configuration or secrets.

---

## Decision: Use layered xUnit test projects

### Date

2026-05-26

### Status

Accepted

### Context

The backend will include domain logic, CQRS handlers, HTTP endpoints, infrastructure adapters, SQL validation, database access, AI orchestration, and export flows. These areas need different testing scopes instead of one broad test project.

### Decision

Use separate test projects for unit, application, functional, integration, and architecture tests.

All test projects should use:

1. xUnit for test execution.
2. NSubstitute for mocks and substitutes.
3. AwesomeAssertions for readable assertions.

Architecture tests should protect dependency direction and project structure.

### Consequences

Benefits:

- Clear separation between fast isolated tests and slower infrastructure tests.
- Easier CI filtering by test project.
- Architecture rules can fail quickly when a layer starts depending on the wrong project.
- Shared test dependencies keep the test stack consistent.

Trade-offs:

- More projects to maintain.
- Integration tests may require local infrastructure such as Docker or SQL Server once real database scenarios are added.

---

## Decision: Use Swagger UI for development API documentation

### Date

2026-05-26

### Status

Accepted

### Context

Developers need a quick way to inspect available HTTP endpoints during local development as Wolverine HTTP endpoints and future API endpoints are added.

### Decision

Use Swashbuckle Swagger UI in the API project and expose it only in the Development environment.

The local Swagger UI route is:

```txt
/swagger
```

The generated OpenAPI document route is:

```txt
/swagger/v1/swagger.json
```

The development launch profiles should open Swagger UI by default.

### Consequences

Benefits:

- Developers can inspect available endpoints during local development.
- The generated OpenAPI document can support future client generation or contract checks.
- Swagger UI remains disabled outside the Development environment by default.

Trade-offs:

- Endpoint metadata must be kept accurate as Wolverine HTTP endpoints are added.
- Some advanced endpoint conventions may need explicit OpenAPI metadata.

---

## Decision: Add Weather Forecasts as the first reference vertical slice

### Date

2026-05-26

### Status

Accepted

### Context

The backend infrastructure now includes CQRS projects, Wolverine as the mediator, Wolverine HTTP, FluentValidation, Mapster, layered dependency injection, Swagger, Serilog, Seq, and multiple test projects. A small endpoint is needed to verify that these choices work together before implementing business-critical reporting features.

### Decision

Add a sample `GET /api/weather-forecasts` endpoint that flows through the full backend slice:

1. API endpoint through Wolverine HTTP.
2. Query dispatch through Wolverine's message bus.
3. Query validation through FluentValidation.
4. Application handler.
5. Infrastructure provider behind an application abstraction.
6. Domain model.
7. DTO mapping through Mapster.
8. Functional Swagger visibility.

### Consequences

Benefits:

- Provides a working reference for future CQRS endpoint slices.
- Verifies runtime Wolverine endpoint and handler discovery.
- Verifies application validation and HTTP validation behavior.
- Gives the test projects a concrete feature to cover.

Trade-offs:

- The endpoint is sample-only and must not be treated as a product feature.
- It should be removed or replaced once real reporting endpoints cover the same architectural flow.

---

## Decision: Use centralized ProblemDetails exception handling

### Date

2026-05-26

### Status

Accepted

### Context

The API needs consistent error responses and structured logging as CQRS handlers, infrastructure providers, and future reporting workflows start throwing application and technical exceptions.

### Decision

Use ASP.NET Core `IExceptionHandler` as the centralized API exception boundary.

Handled exceptions should be returned as ProblemDetails responses. Application `NotFoundException` failures should use:

```json
{
  "status": 404,
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "The specified resource was not found.",
  "detail": "Resource-specific message."
}
```

Validation failures should return HTTP 400 with validation errors. Unexpected failures should return HTTP 500 with a generic detail message.

### Consequences

Benefits:

- Endpoint code stays focused on request dispatching instead of repeated `try/catch` blocks.
- API clients receive predictable error payloads.
- Exceptions are logged consistently at the API boundary.

Trade-offs:

- Application use cases should throw known exception types when a specific HTTP mapping is required.
- New exception categories should be added deliberately to avoid leaking internal details.
