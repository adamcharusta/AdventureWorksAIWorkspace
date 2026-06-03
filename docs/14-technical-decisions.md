# Technical Decisions

## Purpose

This document records important technical and product decisions for AdventureWorksAIWorkspace.

Use this document as a lightweight Architecture Decision Record log.

---

## Decision: Model optional AI conclusions as a separate field, not an extension of the summary

### Date

2026-05-29

### Status

Accepted

### Context

Every successful report turn already produces a short business **summary** (insights). We want the AI to optionally add deeper analysis or recommendations — but only when it judges they add value. Two options were considered:

1. Extend the existing always-present `Summary` so it sometimes carries longer conclusions.
2. Add a separate, optional `Conclusions` field alongside `Summary`.

### Decision

Add a separate, nullable `Conclusions` field on both `Report` (latest turn) and `GeneratedSqlQuery` (per turn), mirroring the existing nullable `Summary` columns. The summary stays short and always present; conclusions are optional and may be null.

This is captured as FR-032 and detailed in [09-reporting-and-visualization.md](09-reporting-and-visualization.md#ai-conclusions).

Implementation notes (2026-05-29): `Conclusions` is a nullable `nvarchar(4000)` column on both `Report` and `GeneratedSqlQuery` (migration `AddReportConclusions`). The visualization step (`AiReportVisualizer`) asks for an optional `conclusions` property and treats absent/empty as null; the heuristic fallback never fabricates conclusions. The report chat workflow persists conclusions per turn and clears them on failed turns. The frontend renders a "Conclusions" panel under the insights only when a value is present.

### Consequences

Benefits:

- The summary keeps a stable, predictable role (always a short answer), while conclusions can vary in length and be omitted without affecting the summary.
- Conclusions are snapshot per turn, so a revisited report shows exactly what was generated.

Trade-offs:

- One extra nullable column on two tables and a small extension to the visualization prompt and DTOs.
- Open questions on length, structure, and UI placement remain (tracked in [15-open-questions.md](15-open-questions.md)).

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

## Decision: Use EF Core for application persistence and Dapper for AdventureWorks query execution

### Date

2026-05-27

### Status

Accepted

### Context

The backend needs to work with two SQL Server databases that have different responsibilities.

The application database is owned by AdventureWorksAIWorkspace. It stores users, saved reports, report conversations, generated SQL metadata, chart definitions, tags, favorites, export history, and other application-owned records.

The AdventureWorks database is an external analytical source. It should be queried through read-only credentials and should not be modified by the application. The AI SQL workflow may generate queries with different result shapes depending on the user's business question.

Using the same full ORM approach for both databases would make the design look simpler, but it would not match the actual usage patterns. The application database benefits from strongly typed entities and migrations. The AdventureWorks database benefits from controlled, validated, read-only SQL execution and flexible tabular result shaping.

### Decision

Use a hybrid data access strategy:

1. Use EF Core with the SQL Server provider as the primary ORM for the application database.
2. Use Dapper with a SQL Server connection factory as the preferred MVP data access approach for AdventureWorks read-only analytical query execution.
3. Keep separate connection strings, credentials, permissions, health checks, options, and logging metadata for each database.
4. Use EF Core migrations only for the application database.
5. Do not generate or maintain a full EF Core model for AdventureWorks during MVP.
6. Keep all data access implementations in the Infrastructure project behind Application-level abstractions.
7. Return AdventureWorks query results as a generic tabular result model containing columns, rows, data types, row count, and execution metadata.
8. Require SQL validation, command timeouts, result-size limits, read-only credentials, and audit logging before any AI-generated SQL is executed.

### Consequences

Benefits:

- EF Core fits application-owned relational data, migrations, relationships, and ASP.NET Core Identity-backed user storage.
- Dapper fits read-only analytical SQL where result shapes are dynamic and SQL text is already part of the workflow.
- The application does not need to model the entire AdventureWorks schema before useful reporting can be built.
- Database ownership boundaries remain clear.
- The AdventureWorks path can stay close to SQL Server behavior, which is useful for query validation, execution limits, and diagnostics.

Trade-offs:

- The backend will use two data access patterns, so conventions must be documented clearly.
- Dapper does not provide migrations, change tracking, or aggregate persistence features; it should not become the default application database persistence layer.
- Query result shaping, data type handling, and pagination/limits must be designed explicitly.
- Integration tests will need coverage for both EF Core persistence and AdventureWorks read-only query execution.

Follow-up decisions:

- Decide the exact Identity user extension fields needed for profile and onboarding state.
- Decide the exact generic tabular result contract returned by AdventureWorks query execution.
- Decide whether a future semantic layer should introduce selected typed read models over AdventureWorks.
- Benchmark representative AI-generated reports before optimizing beyond the initial hybrid approach.

Reference notes:

- Microsoft documents EF Core `DbContext` as the common ASP.NET Core pattern for relational data access, with scoped lifetime registration by default.
- Microsoft documents EF Core raw SQL support, including parameterization guidance and `FromSqlRaw` risks.
- The Dapper project documents parameterized queries, dynamic parameters, SQL Server support, and performance-oriented query mapping.

---

## Decision: Use the official OpenAI .NET SDK behind an Application abstraction for AI features

### Date

2026-05-28

### Status

Accepted

### Context

The core value of AdventureWorksAIWorkspace depends on calling a Large Language Model to turn natural language business questions into safe SQL, to suggest chart configurations, and to produce business summaries. The backend needs a single, well-defined way to talk to the model so the AI SQL workflow can stay testable and so the rest of the system never depends on a specific vendor SDK.

Three integration styles were considered:

1. The official `OpenAI` .NET SDK.
2. A hand-written typed `HttpClient` that calls the OpenAI REST API directly with manual JSON DTOs.
3. The `Azure.AI.OpenAI` SDK targeting Azure OpenAI deployments.

A hand-written client gives full control but requires maintaining request/response DTOs, serialization, streaming, and error handling by hand. `Azure.AI.OpenAI` is the right choice only if the model is hosted on Azure OpenAI rather than `api.openai.com`, which is not the current assumption.

### Decision

Use the official `OpenAI` .NET SDK as the AI client library, hidden behind an Application-level abstraction.

Adopt the following integration model:

1. Define AI capabilities as Application-owned interfaces (for example, an SQL generation contract and a result summarization contract). The Application project must not reference the OpenAI SDK directly.
2. Implement those interfaces in the Infrastructure project using the `OpenAI` SDK.
3. Register the SDK client through a typed `HttpClient` (`AddHttpClient`) so timeouts, resilience policies, and request logging can be configured centrally.
4. Bind model configuration through the options pattern (for example, `OpenAiOptions` with `ApiKey`, `Model`, `BaseUrl`, and `TimeoutSeconds`), mirroring the existing `JwtOptions` approach.
5. Never commit the API key to `appsettings.json` or source control. Use development User Secrets and environment variables, consistent with the Identity bootstrap secret rules.
6. Keep prompt construction, schema context shaping, and AI workflow orchestration in the Application layer; keep only transport, serialization, and SDK specifics in Infrastructure.
7. Treat every model response as untrusted input. Generated SQL must pass the SQL safety validator before it can be executed against AdventureWorks.
8. Cover the AI client with infrastructure registration tests, and cover the AI workflow handlers with application tests using a substituted AI abstraction.

### Consequences

Benefits:

- The official SDK reduces boilerplate for chat completions, structured outputs, streaming, and error handling.
- The Application layer stays vendor-neutral, so the model provider can be swapped by replacing the Infrastructure implementation only.
- A typed `HttpClient` registration centralizes timeouts, retries, and request logging for outbound AI calls.
- The options pattern keeps the model name and endpoint configurable per environment without code changes.
- Treating AI output as untrusted preserves the read-only AdventureWorks safety model.

Trade-offs:

- The application takes a dependency on an external paid API; cost, rate limits, and token usage must be monitored.
- Outbound calls add latency and a new failure mode, so timeouts, retries, and graceful degradation must be designed.
- SDK version changes may require updates in the Infrastructure implementation.
- Prompt content sent to the model must be reviewed for prompt injection and for accidental disclosure of sensitive schema or data.

Follow-up decisions:

- Decide the default model and whether different tasks (SQL generation vs. summary) should use different models.
- Decide whether to use structured outputs / JSON schema responses for SQL and chart suggestions.
- Decide how much AdventureWorks schema context to send: full schema, a curated semantic subset, or retrieved fragments.
- Decide the resilience strategy (retry, circuit breaker, fallback) for outbound AI calls.

Reference notes:

- OpenAI documents an official .NET SDK for chat completions, structured outputs, and streaming.
- Microsoft documents typed `HttpClient` registration through `IHttpClientFactory` for outbound HTTP integrations.
- Microsoft documents the options pattern for binding strongly typed configuration sections.

Implementation notes (2026-05-29):

- The vendor-neutral transport abstraction is `IAiChatClient` (Application). Its implementation `OpenAiChatClient` (Infrastructure) wraps `OpenAI.Chat.ChatClient`.
- The SDK is bridged onto an `IHttpClientFactory`-managed `HttpClient` through `System.ClientModel.Primitives.HttpClientPipelineTransport`, set on `OpenAIClientOptions.Transport`. This satisfies the typed-`HttpClient` requirement because the SDK uses System.ClientModel rather than consuming `HttpClient` directly.
- The API key is read lazily in the client constructor (guarded), not at DI registration time. This keeps the application bootable without a key for non-AI flows; the guard fires only when an AI feature is first used.
- The higher-level capability `IAiSqlGenerator` (interface in Application) builds the prompt and parses the model output. Its implementation `AiSqlGenerator` lives in `Infrastructure/Services` alongside the other service implementations, so prompt construction sits in Infrastructure rather than Application. The AI **workflow orchestration** (generate → validate → execute) remains in the Application layer, in the `GenerateReport` command handler. This is a deliberate adjustment to the original "prompt construction in Application" wording in favour of a single, uniform location for all service implementations.

---

## Decision: Model report chat persistence around reports, messages, and generated SQL artifacts

### Date

2026-05-29

### Status

Proposed

### Context

The reporting workflow is becoming chat-driven. Users should ask an initial business question, receive an AI-generated report, continue refining the report through follow-up messages, and later reopen the same report with its conversation history.

The system also needs to preserve generated SQL for audit, troubleshooting, reuse, and token-cost reduction. Generated SQL is related to the conversation, but it has different lifecycle metadata than a chat message: validation status, execution status, token usage, row/column counts, and execution errors.

### Decision

Use `Report` as the durable parent record for the user-facing report.

Adopt the following MVP persistence model:

1. `Report` stores ownership, metadata, and the latest rendered dashboard snapshot: `UserId`, `Title`, `OriginalPrompt`, `Summary`, `ResultJson`, `ChartsJson`, `Status`, `IsFavorite`, `CreatedAt`, and `UpdatedAt`.
2. `ReportConversation` stores the one active conversation attached to a report, using `ReportId` as the relationship back to its parent report.
3. `ReportMessage` stores user, assistant, and system messages in chronological order.
4. `GeneratedSqlQuery` stores each generated or reused SQL attempt separately from the chat transcript.
5. Generated SQL records link back to the report and, when possible, to the source user message that produced the SQL.
6. The persisted chat API should use report-centered endpoints, such as creating a report from the first message and appending messages to an existing report.
7. Report ownership must be enforced on every report read, chat, update, favorite, and export operation.
8. Report titles are AI-suggested during initial generation and can be renamed by the owning user.

### Consequences

Benefits:

- Saved reports can be reopened with both dashboard metadata and conversation history.
- The report sidebar can behave like a chat history list by showing each saved report title.
- The center workspace can reopen a saved report with its latest chart configuration and tabular result without immediately rerunning SQL.
- Multiple SQL attempts can be tracked for one report as the user refines it.
- SQL validation and execution metadata remains auditable without cluttering the chat transcript.
- The sidebar can query lightweight report metadata without loading full conversations or query results.
- Future SQL reuse can search generated SQL artifacts independently from chat messages.

Trade-offs:

- The first persisted endpoint needs a transaction boundary that creates report records, messages, SQL artifacts, and status updates together.
- A follow-up prompt needs context selection rules so the AI receives enough history without resending the entire transcript every time.
- Large analytical result sets may increase application database size, so query result limits and future retention/versioning rules matter.
- If users later need branching conversations, the one-conversation-per-report MVP model will need versioning or branching support.

Follow-up decisions:

- Decide whether the first persisted endpoint should be `POST /api/reports` or keep `POST /api/reports/generate` and evolve its contract.
- Decide whether generated SQL should link to both the source user message and the assistant message that presented the result.
- Decide the exact transaction and failure behavior when AI generation succeeds but persistence or SQL execution fails.

---

## Decision: Use Docker Compose with an AdventureWorks init job for local SQL Server

### Date

2026-05-27

### Status

Accepted

### Context

Developers need a repeatable local SQL Server environment for the AdventureWorks analytical database. The database should be restored automatically when it is missing, while avoiding repeated downloads and restores once local data already exists.

### Decision

Use the root `compose.yaml` to run:

1. A `sqlserver` service based on `mcr.microsoft.com/mssql/server:2025-latest`.
2. A one-shot `adventureworks-init` service that waits for SQL Server health, checks `sys.databases`, downloads the AdventureWorks backup only if needed, detects logical file names with `RESTORE FILELISTONLY`, and restores the database.

The default local sample database is `AdventureWorks2025`, restored from Microsoft's AdventureWorks backup release.

### Consequences

Benefits:

- Developers can start a local AdventureWorks SQL Server with one Compose command.
- SQL Server data persists across container restarts through a Docker volume.
- The backup file is cached in a Docker volume and is not downloaded repeatedly.
- The restore behavior is idempotent because the init job first checks whether the database exists.
- The init job does not rely on guessed logical file names inside the `.bak` file.

Trade-offs:

- The first startup requires downloading a large `.bak` file.
- The setup is intended for local development and testing, not production.
- Developers must provide a strong local `MSSQL_SA_PASSWORD` through environment configuration.

Reference notes:

- Microsoft Learn documents direct AdventureWorks `.bak` downloads and Linux `RESTORE DATABASE` syntax.
- Microsoft Learn documents SQL Server Linux containers, `MSSQL_SA_PASSWORD`, and `/opt/mssql-tools18/bin/sqlcmd`.

---

## Decision: Run Seq, API, and web app through the local Docker Compose stack

### Date

2026-05-27

### Status

Accepted

### Context

Developers need a single local command that can start the observable application stack, not only the SQL Server dependency. The API should run with local configuration, the web app should proxy API calls inside the Docker network, and structured logs should be visible in Seq.

### Decision

Extend the root `compose.yaml` with:

1. `seq` using the official `datalust/seq` image, with local no-auth first-run configuration.
2. `api` built from `source/api/src/Api/Dockerfile`.
3. `web` built from `source/web/Dockerfile`.

The API runs with `ASPNETCORE_ENVIRONMENT=Development`, receives required local secrets through environment variables, connects to SQL Server through the internal service name `sqlserver`, and sends Serilog events to Seq through `http://seq`.

The web app runs through nginx on port 8080 inside the container and proxies `/api/` requests to `http://api:8080`.

Compose health checks gate local startup: SQL Server is checked with `sqlcmd`, Seq is checked over local HTTP, the API exposes `/health`, and the web app waits for the API to become healthy.

### Consequences

Benefits:

- A developer can start the local application stack with `docker compose up -d --build`.
- Seq is available locally for structured API log inspection.
- The web app and API communicate through stable Docker service names.
- Host port mappings are configurable through `.env`.
- Startup ordering is less fragile because dependent services wait for health checks instead of process start alone.

Trade-offs:

- The Compose setup needs local development secrets for SQL Server, JWT signing, and the initial Admin account.
- Local Compose currently uses the SQL Server `sa` login for API database access. Least-privilege local logins should be added later.
- API startup still depends on valid EF Core migration state for the application database.
- Seq authentication is disabled in local Compose only and must not be copied to production.

Reference notes:

- Seq documentation describes the `datalust/seq` Docker image, `ACCEPT_EULA=Y`, `/data` persistence, and host port mapping to container port `80`.
- Docker Compose documentation describes defining and running multi-container applications from a Compose file.

---

## Decision: Use ASP.NET Core Identity with closed registration and Admin-managed user provisioning

### Date

2026-05-27

### Status

Proposed

### Context

The application needs authenticated users because reports, conversations, favorites, tags, generated SQL metadata, and export history are user-specific.

The MVP should not allow public self-registration. User access should be controlled by administrators. The initial product model needs two roles:

1. Admin
2. User

The system also needs a way to create the first Admin account before any administrator exists.

### Decision

Use ASP.NET Core Identity backed by EF Core and the application database.

Adopt the following MVP authentication and authorization model:

1. Disable public self-registration.
2. Create two initial roles: Admin and User.
3. Allow Admin users to create and manage user accounts.
4. Assign the User role by default when an Admin creates a normal user.
5. Create provisioned accounts with the configured initial template password.
6. Require all provisioned accounts to change the temporary password during the first login flow before accessing the main application.
7. Apply the same first login password change rule to the first bootstrapped Admin account.
8. Use policy-based authorization for application boundaries, even when the first policies map directly to roles.
9. Bootstrap the first Admin account from secure startup configuration.
10. Create the first Admin account with the configured initial template password.
11. Do not commit plain-text passwords or real bootstrap credentials to `appsettings.json` or source control.
12. Store bootstrap secrets in development User Secrets, environment variables, or a production secret store.

### Consequences

Benefits:

- ASP.NET Core Identity provides a standard foundation for password hashing, user stores, roles, lockout, security stamps, password reset tokens, and future MFA.
- Closed registration matches the desired controlled-access product model.
- Admin-managed user provisioning avoids unknown public users entering the system.
- Forced first login password change reduces risk from temporary or bootstrap credentials.
- Policy-based authorization leaves room for future ownership, sharing, workspace, or audit permissions.

Trade-offs:

- The product needs an Admin user management experience.
- Initial Admin bootstrap must be designed carefully to avoid persistent default credentials.
- First login password change needs a clear UX and backend state.
- Email delivery or another secure handoff mechanism may be needed for password setup links.

Follow-up decisions:

- Decide whether production onboarding should keep the shared initial template password model, use unique temporary passwords, emailed one-time links, or password reset tokens.
- Decide whether Admin users can create other Admin users in MVP.
- Decide whether inactive/locked users should remain visible in report ownership history.
- Decide the exact API/session model for the React frontend, such as cookie-based Identity endpoints or token-based authentication.

Reference notes:

- Microsoft documents adding role services to Identity through `AddRoles`.
- Microsoft documents role-based and policy-based authorization for ASP.NET Core.
- Microsoft documents account confirmation and password recovery flows for ASP.NET Core Identity.
- Microsoft documents that passwords and other secrets should not be stored in committed configuration files such as `appsettings.json`.

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

Superseded

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

### Superseded Note

2026-06-03: The Weather Forecasts reference slice (endpoint, domain, application, infrastructure, and tests) has been removed from the codebase now that the AI SQL generation and report endpoints cover the same architectural flow. The records below that mention `WeatherForecastDto` describe the historical state of the frontend tooling at the time those decisions were made.

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

---

## Decision: Adopt Orval as the OpenAPI client generator for the frontend

### Date

2026-05-26

### Status

Accepted

### Context

The initial frontend setup used `openapi-typescript` plus a hand-written `apiClient` (built on `openapi-fetch`) and a manually written `useWeatherForecasts` hook. Because the API endpoint returned `Results.Ok<IReadOnlyList<WeatherForecastDto>>`, Swashbuckle described the 200 response as `IResult`, which `openapi-typescript` rendered as an empty `Record<string, never>`. The hook therefore had to mirror the DTO type by hand and cast the response with `as unknown as WeatherForecast[]`.

Both problems compound as the API grows: every new endpoint requires a hand-written hook, and any schema imprecision on the API side leaks into casts on the frontend.

### Decision

Replace `openapi-typescript` and `openapi-fetch` with [Orval](https://orval.dev/) configured to generate:

1. TanStack Query hooks per tag (`react-query` client, `fetch` HTTP client).
2. MSW v2 request handlers and faker-driven mocks per tag.
3. Zod schemas per operation for runtime response validation.
4. Per-schema model files under `src/api/generated/model/`.

The configuration lives in `source/web/orval.config.ts` and runs through `npm run api:gen` (one-shot) and `npm run api:gen:watch` (poll the OpenAPI document).

### Consequences

Benefits:

- One generator command produces typed hooks, mocks, and Zod validators for every API endpoint.
- Per-tag file splitting keeps the generated code navigable as the API grows.
- Generated MSW handlers and faker factories give Vitest tests a single source of truth for fixture data.
- The Zod output enables optional runtime contract enforcement if a slice opts in to it.

Trade-offs:

- Orval pulls in a large dependency graph (Spectral, IBM OpenAPI ruleset). Two npm `overrides` were required (`ajv@^8.17.1` and `commander@^14.0.3`) to resolve peer dependency clashes with `eslint-plugin-jsx-a11y` and `cypress`.
- Generated files must never be edited; downstream code should depend only on the public hook/type surface.
- The OpenAPI document must remain accurate. Imprecise schemas reach the frontend immediately.

---

## Decision: Mark non-nullable schema properties as required in the OpenAPI document

### Date

2026-05-26

### Status

Accepted

### Context

Swashbuckle defaults to treating every record property as optional in the generated OpenAPI document. With C# nullable reference types enabled, this caused every property of `WeatherForecastDto` to be emitted as `string?` / `number?` on the frontend even though the backend always populates them. Orval propagated the optionality straight into the generated hooks, which forced `?? ''` and `?? 0` fallbacks in JSX consumers.

### Decision

Enable two adjustments in the Swashbuckle configuration:

1. Call `SupportNonNullableReferenceTypes()` so the generated schema honors C# nullable reference type annotations.
2. Register a custom `RequireNonNullableSchemaFilter` (`source/api/src/Api/OpenApi/RequireNonNullableSchemaFilter.cs`) that promotes every non-nullable property to the schema's `required` array. This works around a known Swashbuckle limitation where record primary constructor parameters are not added to `required` automatically.

The schema filter also calls `UseAllOfToExtendReferenceSchemas()` so referenced schemas can be extended without losing the `$ref`.

### Consequences

Benefits:

- DTO properties that the backend always populates are typed as non-optional on the frontend, eliminating defensive fallbacks in components.
- The OpenAPI document accurately reflects backend nullability, which improves the value of generated Zod validators.

Trade-offs:

- Properties that should remain optional must be expressed with explicit `?` in C# or removed from the primary constructor.
- Future request body schemas should be reviewed to make sure optional payload fields are intentionally left non-required.

---

## Decision: Use a custom fetch mutator that throws on non-2xx responses

### Date

2026-05-26

### Status

Accepted

### Context

The default Orval `fetch` HTTP client returns the parsed response regardless of HTTP status. With TanStack Query this means a 4xx or 5xx response is treated as a successful query, the `error` field stays empty, and the UI silently renders empty data. Hooks therefore could not surface API failures without each consumer re-checking `data.status`.

### Decision

Provide a custom fetch mutator at `source/web/src/api/customFetch.ts` and wire it through `orval.config.ts` as `override.mutator`. The mutator:

1. Calls `fetch` once with the request init forwarded from TanStack Query.
2. Reads the body as text and attempts `JSON.parse`, falling back to the raw text when parsing fails.
3. Throws a typed `ApiError` (with `status`, `body`, and `message`) when `response.ok` is `false`.
4. Returns the orval-shaped response envelope (`{ data, status, headers }`) on success.

### Consequences

Benefits:

- TanStack Query receives a thrown error on HTTP failures, so the `error` field, `isError` flag, and error boundaries behave correctly without extra checks in components.
- Consumers can `instanceof ApiError` to read `status` and parsed error bodies when they need to differentiate failure modes.
- Future logging or toast notification hooks can attach to the mutator instead of every call site.

Trade-offs:

- All generated calls now go through one mutator. Changes to it ripple through every endpoint.
- The mutator does not yet handle authentication, retries, or request correlation; those concerns must be added intentionally.

---

## Decision: Use MSW for Vitest API mocking; keep cy.intercept for Cypress component tests

### Date

2026-05-26

### Status

Accepted

### Context

Vitest tests previously mocked the API by stubbing the hand-written `apiClient` with `vi.mock`. That coupled tests to the internal client shape and bypassed the actual fetch path, which masked failures in the new custom mutator. Cypress component tests used `cy.intercept`, which natively integrates with the Cypress runner.

### Decision

For Vitest:

1. Install MSW v2 and set up an `msw/node` server in `src/test/server.ts`.
2. Start the server in `src/test/setup.ts` with `onUnhandledRequest: 'error'` and reset handlers after each test.
3. Use the generated MSW handler factories per endpoint for the happy path and ad-hoc `http.get` handlers for failure modes.

For Cypress component tests, keep using `cy.intercept`. Mounting an MSW Service Worker inside the Cypress component test runner would add infrastructure for no real benefit; `cy.intercept` is already runner-native and uses the generated DTO types for fixtures.

### Consequences

Benefits:

- Vitest tests now exercise the real `fetch` -> custom mutator -> hook pipeline.
- The generated MSW handler factory provides a single source of truth for fixture data shared with future tests.
- Cypress tests remain simple and avoid Service Worker registration in the Cypress dev server.

Trade-offs:

- Two mocking technologies coexist; contributors must understand which runner uses which one.
- MSW handlers must stay aligned with the generated request paths; a regenerated client could shift handler URLs.

---

## Decision: Use sonner with MUI Alert wrappers as the toast library

### Date

2026-05-26

### Status

Accepted

### Context

The application will need transient notifications for save outcomes, AI workflow status, export results, and SQL validation failures. The existing UI is built entirely on Material UI, so notifications should feel native to that visual language while staying flexible enough to render arbitrary JSX (e.g., links, secondary actions, custom layouts).

The shortlist considered three modern libraries:

1. `sonner` - fully JSX-driven (`toast.custom`), lightweight, very active.
2. `notistack` - built on MUI Snackbar, MUI-native but with an older imperative API.
3. `react-hot-toast` - similar to sonner in spirit, less active.

### Decision

Use [`sonner`](https://sonner.emilkowal.ski/) and expose a thin MUI-aware wrapper at `source/web/src/shared/lib/toast.tsx`. The wrapper:

1. Renders every toast through `sonner.custom`, returning a MUI `Alert` (filled variant) with optional `AlertTitle` and a close button wired to `sonner.dismiss`.
2. Exposes `toast.success`, `toast.error`, `toast.info`, `toast.warning` plus passthrough `dismiss` and `custom` exports.

The `<Toaster />` provider is mounted once in `src/main.tsx` (`position="top-right"`).

### Consequences

Benefits:

- Toasts use the same MUI severity palette and elevation as in-page alerts.
- Consumers stay decoupled from `sonner`'s native API; switching the underlying library later only touches `src/shared/lib/toast.tsx`.
- Custom JSX inside toasts (links, secondary actions) remains trivial via `toast.custom`.

Trade-offs:

- Toasts are not visible in Vitest tests (no `<Toaster />` in the test render tree). Tests that need to assert toast output must wrap the subject under test with `<Toaster />`.
- Cypress component tests would also need `<Toaster />` mounted to assert toast UI; the existing tests do not currently rely on toasts.

---

## Decision: Track refactoring work as a backlog and record refactoring direction

### Date

2026-06-03

### Status

Accepted

### Context

A codebase review identified several refactoring opportunities across the backend and frontend
(duplication, a dead vertical slice, long methods, an oversized seeder, a hand-written API client that
bypasses the adopted Orval generator, and inconsistent enum serialization). The project is in the
planning and documentation phase, so the question is how to capture this work without changing
production code prematurely.

### Decision

Record the refactoring opportunities as documentation first, before any implementation:

1. Add a "Refactoring and Technical Debt" epic to [12-backlog.md](12-backlog.md) with `REF-` prefixed
   items grouped by priority.
2. Stage detailed, independently scoped issue drafts under "Refactoring Issue Drafts" in
   [13-github-issues.md](13-github-issues.md).
3. Treat every refactoring item as behavior-preserving: success is verified by the existing test
   suites, not by new product behavior.

The recorded refactoring direction is:

- Prefer one canonical SQL generation path (remove the unused `GenerateReport` slice).
- Prefer injectable, single-responsibility services over static helpers and god-classes
  (`ReportChatWorkflow`, `UserService`).
- Keep generated code as the single source of truth for the API client; the Reports feature should
  use Orval like every other tag, and API enums should serialize as stable strings.
- Centralize duplicated cross-cutting concerns (current-user-id resolution, AI response parsing,
  `JsonSerializerOptions`).
- Separate demo/seed data from database bootstrap.

### Consequences

Benefits:

- Refactoring is visible, prioritized, and reviewable before any code changes.
- Each item is small and independently shippable, with clear acceptance criteria.
- The recorded direction keeps future refactoring consistent with existing decisions (Orval, layered
  architecture).

Trade-offs:

- The drafts must be kept in sync as the code evolves; stale drafts should be closed or updated.
- Some items depend on others (for example, the Reports Orval client depends on stable string enums),
  so ordering matters during implementation.
