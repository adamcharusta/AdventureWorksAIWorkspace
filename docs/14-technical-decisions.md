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

The configuration lives in `source/AdventureWorksAIWorkspaceGUI/orval.config.ts` and runs through `npm run api:gen` (one-shot) and `npm run api:gen:watch` (poll the OpenAPI document).

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
2. Register a custom `RequireNonNullableSchemaFilter` (`source/AdventureWorksAIWorkspaceAPI/src/Api/OpenApi/RequireNonNullableSchemaFilter.cs`) that promotes every non-nullable property to the schema's `required` array. This works around a known Swashbuckle limitation where record primary constructor parameters are not added to `required` automatically.

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

Provide a custom fetch mutator at `source/AdventureWorksAIWorkspaceGUI/src/api/customFetch.ts` and wire it through `orval.config.ts` as `override.mutator`. The mutator:

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
3. Use the generated MSW handler factory (`getGetWeatherForecastsMockHandler`) for the happy path and ad-hoc `http.get` handlers for failure modes.

For Cypress component tests, keep using `cy.intercept`. Mounting an MSW Service Worker inside the Cypress component test runner would add infrastructure for no real benefit; `cy.intercept` is already runner-native and uses the generated `WeatherForecastDto` types for fixtures.

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

Use [`sonner`](https://sonner.emilkowal.ski/) and expose a thin MUI-aware wrapper at `source/AdventureWorksAIWorkspaceGUI/src/lib/toast.tsx`. The wrapper:

1. Renders every toast through `sonner.custom`, returning a MUI `Alert` (filled variant) with optional `AlertTitle` and a close button wired to `sonner.dismiss`.
2. Exposes `toast.success`, `toast.error`, `toast.info`, `toast.warning` plus passthrough `dismiss` and `custom` exports.

The `<Toaster />` provider is mounted once in `src/main.tsx` (`position="top-right"`).

### Consequences

Benefits:

- Toasts use the same MUI severity palette and elevation as in-page alerts.
- Consumers stay decoupled from `sonner`'s native API; switching the underlying library later only touches `src/lib/toast.tsx`.
- Custom JSX inside toasts (links, secondary actions) remains trivial via `toast.custom`.

Trade-offs:

- Toasts are not visible in Vitest tests (no `<Toaster />` in the test render tree). Tests that need to assert toast output must wrap the subject under test with `<Toaster />`.
- Cypress component tests would also need `<Toaster />` mounted to assert toast UI; the existing tests do not currently rely on toasts.
