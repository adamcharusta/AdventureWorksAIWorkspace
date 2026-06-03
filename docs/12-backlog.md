# Backlog

## Purpose

This document contains the initial product backlog for AdventureWorksAIWorkspace.

## Epic: Project Documentation

### Tasks

- [ ] Create initial project overview documentation.
- [ ] Create business requirements documentation.
- [ ] Create user flow documentation.
- [ ] Create functional requirements documentation.
- [ ] Create non-functional requirements documentation.
- [ ] Create architecture documentation.
- [ ] Create data model assumptions.
- [ ] Create AI SQL workflow documentation.
- [ ] Create reporting and visualization documentation.
- [ ] Create security and authorization documentation.
- [ ] Create export documentation.
- [ ] Create glossary.

## Epic: Report Management

### Tasks

- [ ] Define report entity.
- [ ] Define report metadata.
- [ ] Define report conversation model.
- [ ] Define report chat endpoint contracts.
- [ ] Define generated SQL history model.
- [ ] Define report ownership and access checks.
- [ ] Define recent reports behavior.
- [ ] Define favorite reports behavior.
- [ ] Define report tagging behavior.
- [ ] Define report search and filtering behavior.

## Epic: AI SQL Workflow

### Tasks

- [ ] Define AI prompt structure.
- [ ] Define report chat context strategy for follow-up messages.
- [ ] Define AdventureWorks schema context strategy.
- [ ] Define intent extraction format.
- [ ] Define SQL generation rules.
- [ ] Define SQL validation rules.
- [ ] Define SQL execution constraints.
- [ ] Define SQL reuse and caching strategy.
- [ ] Persist generated SQL validation and execution metadata.

## Epic: Backend Data Access

### Tasks

- [ ] Define EF Core usage for the application database.
- [ ] Define read-only AdventureWorks query execution strategy.
- [ ] Define database connection configuration for both databases.
- [ ] Define application database migration strategy.
- [ ] Define query result shape for AI-generated analytical queries.

## Epic: Dashboard Rendering

### Tasks

- [ ] Define dashboard layout model.
- [ ] Define chart definition model.
- [ ] Define supported chart types.
- [ ] Define table rendering behavior.
- [ ] Define KPI rendering behavior.
- [ ] Define AI summary placement.

## Epic: Security

### Tasks

- [ ] Define ASP.NET Core Identity authentication approach.
- [ ] Define Admin and User role authorization rules.
- [ ] Define closed registration behavior.
- [ ] Define Admin-managed user provisioning flow.
- [ ] Define Admin-managed user deletion and self-delete protection.
- [ ] Define configured initial template password strategy.
- [ ] Define forced first login password change flow.
- [ ] Define initial Admin bootstrap strategy.
- [ ] Define database access rules.
- [ ] Define SQL safety rules.
- [ ] Define prompt injection safeguards.
- [ ] Define secret management approach.

## Epic: Export

### Tasks

- [ ] Define PDF export content.
- [ ] Define PDF layout rules.
- [ ] Define export history requirements.
- [ ] Define future Excel export assumptions.

## Epic: Future Enhancements

### Tasks

- [ ] Define report sharing concept.
- [ ] Define report folders concept.
- [ ] Define scheduled reports concept.
- [ ] Define manual chart editing concept.
- [ ] Define semantic layer concept.

## Epic: Refactoring and Technical Debt

These items capture refactoring opportunities identified during a codebase review. They do not
add product behavior; they reduce duplication, improve separation of concerns, and align the code
with decisions already recorded in [14-technical-decisions.md](14-technical-decisions.md). Detailed
issue drafts are staged in [13-github-issues.md](13-github-issues.md) (prefixed `REF-`).

### High priority

- [x] REF-1: Remove the unused `GenerateReport` vertical slice (backend).
- [x] REF-2: Convert `ReportChatWorkflow` into an injectable service and decompose `ProcessAsync` (backend).
- [x] REF-3: Extract the sample-report seed data out of `AppDbContextInitializer` (backend).
- [x] REF-4: Generate the Reports API client with Orval instead of the hand-written `report-api.ts` (frontend). The Reports feature now uses the generated `reports` tag and model types; `report-api.ts` is removed and `report-types.ts` re-exports the generated model.
- [x] REF-6: Register a global `JsonStringEnumConverter` for API responses (backend / API contract).

### Medium priority

- [x] REF-7: Extract a shared current-user-id resolution helper for endpoints (backend).
- [x] REF-8: Split `UserService` by responsibility (token, authentication, user management) (backend).
- [x] REF-9: Extract shared AI response parsing and `JsonSerializerOptions` helpers (backend).
- [x] REF-10: Decompose the large `ChatDrawer.tsx` and `AdminPanelPage.tsx` components (frontend).

### Low priority

- [ ] REF-11: Replace manual JSON round-tripping in report persistence with EF Core value converters (backend). _Will not do as specified: holding typed `Result`/`Charts` on the `Report` entity would make the Domain layer depend on the Application DTOs `TabularResult`/`ChartSpec`, which violates the enforced Domain → Application layering (`LayerDependencyTests`). Storing JSON in Domain and converting in `ReportMapping` (Application) is the correct pattern for this architecture. The duplicated `JsonSerializerOptions` part was already addressed by REF-9._

### Cleanup

- [x] REF-5: Regenerate the Orval client to drop the stale `weather-forecasts` tag (frontend). Regenerated against the running API; the `weather-forecasts` tag is gone and `reports`/`health` tags are now generated with string enums.
