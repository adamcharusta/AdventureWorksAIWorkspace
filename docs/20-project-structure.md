# Project Structure

## Purpose

This document defines the repository layout conventions for AdventureWorksAIWorkspace.

## Root Layout

```txt
docs/       # Product, architecture, requirements, decisions, and planning docs
docker/     # Docker build helpers and local infrastructure scripts
source/
  api/      # ASP.NET Core backend solution
  web/      # React frontend application
```

Root-level Compose configuration is kept in `compose.yaml` so the full local stack can be started from the repository root.

## Backend Layout

The backend solution lives under `source/api/`:

```txt
source/api/
  AdventureWorksAIWorkspace.sln
  src/
    Api/
    Application/
    Domain/
    Infrastructure/
  tests/
    Application.Tests/
    Architecture.Tests/
    Functional.Tests/
    Integration.Tests/
    Unit.Tests/
```

Backend namespaces and assembly names use the `AdventureWorksAIWorkspace.*` prefix.

The intended dependency direction remains:

```txt
Api -> Application -> Domain
Api -> Infrastructure -> Application
Infrastructure -> Domain
```

Application service contracts and infrastructure service implementations are organized with mirrored topic folders:

```txt
source/api/src/Application/Common/Services/
  AdventureWorks/
  Ai/
  Auth/
  Reports/
  Sql/
  User/

source/api/src/Infrastructure/Services/
  AdventureWorks/
  Ai/
  Auth/
  Sql/
  User/
```

`Application/Common/Services` contains abstractions used by CQRS handlers and application workflows. `Infrastructure/Services` contains technical implementations of those abstractions. Application code must depend on the contracts, not on infrastructure namespaces.

Persistence-specific implementations can still live in `Infrastructure/Database` when the database boundary is the clearest ownership boundary. For example, `IReportRepository` is declared under `Application/Common/Services/Reports`, while `ReportRepository` remains under `Infrastructure/Database`.

## Frontend Layout

The frontend lives under `source/web/`:

```txt
source/web/src/
  app/
  api/
  assets/
  features/
    admin/
    auth/
    reports/
    workspace/
  shared/
  test/
```

### `src/app`

Contains application-level composition:

- top-level router
- route-level app shell decisions
- app-only pages such as not-found

### `src/api`

Contains API boundary code:

- `customFetch.ts`
- custom API error handling integration points
- generated Orval output under `generated/`

Generated files under `src/api/generated/` must not be edited by hand.

### `src/features`

Contains product feature modules. Each feature should own its pages, components, hooks, and feature-specific helpers when practical.

Current feature modules:

- `admin` for user administration.
- `auth` for login, first-password setup, route guards, auth state, JWT helpers, and role helpers.
- `reports` for report visualization components and report-specific data shaping.
- `workspace` for the main report workspace, drawers, report sidebar interactions, and chat-driven report workflow.

### `src/shared`

Contains cross-feature utilities and UI:

- shared theme components
- shared hooks
- toast wrapper
- query client setup
- generic JSON and API error helpers
- global style overrides

Feature code may depend on `shared`, but `shared` should not depend on feature modules.

## Naming Conventions

- Use `api` for the backend source folder and Docker Compose service.
- Use `web` for the frontend source folder and Docker Compose service.
- Use `AdventureWorksAIWorkspace.*` for backend namespaces and assemblies.
- Avoid reintroducing `AdventureWorksAIWorkspaceAPI` or `AdventureWorksAIWorkspaceGUI` in new code or documentation.
