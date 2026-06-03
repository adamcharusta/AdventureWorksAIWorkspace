# Testing Strategy

## Purpose

This document defines the planned testing structure for AdventureWorksAIWorkspace, covering both the .NET backend and the React frontend.

## Backend Test Projects

The API solution uses layered test projects under:

```txt
source/api/tests/
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

## Frontend Testing

The frontend uses two complementary runners. Each has a dedicated mocking strategy that suits the runtime it executes in.

```txt
source/web/
  src/                  # Vitest unit and component tests live next to app/features/shared source
  cypress/component/    # Cypress component tests
  cypress/e2e/          # Cypress end-to-end tests
```

### Vitest (jsdom)

Vitest is the default test runner for unit and component tests that do not require a real browser.

Stack:

- Vitest with the jsdom environment for DOM APIs.
- React Testing Library for component rendering and queries.
- `@testing-library/jest-dom` for DOM assertions.
- MSW v2 for HTTP mocking.

Conventions:

- A shared MSW server lives in `src/test/server.ts`. Lifecycle hooks in `src/test/setup.ts` start it before all tests with `onUnhandledRequest: 'error'`, reset handlers after each test, and close it after the suite.
- Happy-path scenarios should use the generated MSW handler factories from `src/api/generated/**/*.msw.ts` so that fixture data follows the same shape as the contract.
- Ad-hoc `http.get` / `http.post` handlers should be used for failure modes such as 4xx, 5xx, and network errors.
- Tests should exercise the real `fetch` -> `customFetch` mutator -> hook -> component pipeline rather than mocking the generated hooks.

### Cypress (component)

Cypress component tests run inside a Vite dev server provided by the Cypress runner. Tests are colocated under `cypress/component/`.

Mocking strategy:

- `cy.intercept` is the default HTTP mocking tool. It is runner-native, requires no Service Worker registration in the component test bundle, and integrates with `cy.wait` for ordered assertions.
- MSW is intentionally not mounted in the Cypress component test runner. The combined complexity of registering a browser Service Worker inside the Cypress dev server is not justified for component-scoped tests.
- Fixtures may be shared between Vitest and Cypress through `cypress/fixtures/*.json` and the generated TypeScript model types under `src/api/generated/model/`.

### Cypress (end-to-end)

End-to-end tests under `cypress/e2e/` exercise the assembled application against either:

- A locally running API + web app pair.
- Future mocked HTTP responses for fully isolated runs.

End-to-end coverage is intentionally lighter than Vitest and component test coverage. It should focus on critical user flows such as login, report generation, report reopen, favorite, tag, and export.

### Shared Conventions

- The generated API client must never be modified by hand; tests should import from `src/api/generated/**` only.
- Mock data should reflect realistic backend responses. Faker-based generators inside the generated MSW handlers are acceptable for randomised cases; specific fixtures should be used for deterministic assertions.
- Tests must not depend on the order other tests run in. Use MSW handler resets and `cy.intercept` per test setup.
