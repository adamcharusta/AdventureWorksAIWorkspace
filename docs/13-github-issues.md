# GitHub Issue Drafts

## Purpose

This document acts as a staging area for GitHub issue drafts.

Issues should be reviewed here before being created as real GitHub issues.

Agents must not create real GitHub issues without explicit user approval.

---

## Issue: Create initial project documentation structure

### Context

The project needs a documentation-first foundation before implementation begins.

### Goal

Create the initial `/docs` folder structure and high-level documentation files.

### Scope

- Project overview
- Business requirements
- User flows
- Functional requirements
- Non-functional requirements
- Architecture
- Data model assumptions
- AI SQL workflow
- Reporting and visualization
- Security and authorization
- Exporting
- Backlog
- GitHub issue drafts
- Technical decisions
- Open questions
- Glossary

### Out of Scope

- Production code
- API implementation
- Frontend implementation

### Acceptance Criteria

- [ ] `/docs` folder exists.
- [ ] Initial documentation files are created.
- [ ] README links to documentation.
- [ ] All repository content is written in English.

### Labels

- documentation
- analysis

---

## Issue: Define AI SQL workflow

### Context

The core value of the application depends on transforming natural language prompts into safe SQL queries and useful dashboards.

### Goal

Define the AI SQL workflow before implementation.

### Scope

- Intent extraction
- SQL generation
- SQL validation
- Query execution
- Chart planning
- Report persistence
- Token usage optimization

### Out of Scope

- OpenAI API implementation
- SQL parser implementation
- Query execution code

### Acceptance Criteria

- [ ] AI SQL workflow is documented.
- [ ] SQL safety rules are listed.
- [ ] Open questions are captured.
- [ ] Backlog tasks are created.

### Labels

- ai
- architecture
- security

---

## Issue: Define report persistence model

### Context

Reports should behave similarly to ChatGPT or Claude conversations and must be saved for future use.

### Goal

Define the initial data model for saved reports and report conversations.

### Scope

- Report entity
- Report conversation
- Report messages
- Generated SQL metadata
- Chart definitions
- Tags
- Favorites
- Export history

### Out of Scope

- EF Core implementation
- Database migrations
- API endpoints

### Acceptance Criteria

- [ ] Report-related entities are documented.
- [ ] Required metadata is listed.
- [ ] Open questions are documented.
- [ ] Backlog tasks are updated.

### Labels

- data-model
- reporting

---

## Issue: Define data access strategy for application and AdventureWorks databases

### Context

The backend will use two separate SQL Server databases with different responsibilities. The application database stores users, reports, conversations, tags, favorites, generated SQL metadata, chart configuration, and export metadata. The AdventureWorks database is an external analytical source used for read-only business queries generated or assisted by AI.

### Goal

Document the preferred data access strategy for both databases before implementing persistence or query execution code.

### Scope

- Application database ORM choice.
- AdventureWorks read-only query execution approach.
- Connection string and credential separation.
- Migration ownership boundaries.
- Query result shape for dashboard rendering.
- Safety constraints for executing AI-generated SQL.

### Out of Scope

- EF Core implementation.
- Dapper implementation.
- Database migrations.
- SQL validator implementation.
- API endpoints.

### Acceptance Criteria

- [ ] Application database persistence strategy is documented.
- [ ] AdventureWorks query execution strategy is documented.
- [ ] Migration boundaries are documented.
- [ ] Security and read-only constraints are documented.
- [ ] Open questions or follow-up decisions are captured.

### Labels

- architecture
- data-model
- backend
- security

---

## Issue: Define authentication, authorization, and user provisioning model

### Context

The application stores user-specific reports, conversations, favorites, tags, generated SQL metadata, and export history. Public registration is not desired for the MVP. Users should be managed by administrators, and access should be controlled through clear roles and policies.

### Goal

Define the authentication, authorization, and user provisioning model before implementation.

### Scope

- ASP.NET Core Identity-backed authentication.
- Admin and User roles.
- Closed public registration.
- Admin-managed user creation.
- Configured initial template password with forced first login password change.
- Initial Admin bootstrap strategy.
- Secret handling for bootstrap credentials.
- Initial authorization policies.

### Out of Scope

- Authentication implementation.
- Identity UI or API endpoint implementation.
- Email provider implementation.
- Multi-tenant or organization-level permissions.
- Report sharing.

### Acceptance Criteria

- [ ] Public self-registration behavior is documented as disabled for MVP.
- [ ] Admin and User roles are documented.
- [ ] Admin-managed user creation is documented.
- [ ] Initial template password and first login password change behavior is documented.
- [ ] Initial Admin bootstrap rules are documented.
- [ ] The first Admin account is documented as requiring password change on first login.
- [ ] Secret management requirements are documented.
- [ ] Follow-up open questions are captured.

### Labels

- security
- backend
- architecture
- ux

---

## Issue: Define dashboard layout requirements

### Context

The main UI should contain a left report sidebar, center dashboard workspace, and right AI chat sidebar.

### Goal

Document the dashboard layout and interaction requirements.

### Scope

- Left collapsible sidebar
- Center workspace
- Right collapsible sidebar
- Report rendering area
- Chat interaction area
- Report management interactions

### Out of Scope

- React implementation
- MUI component implementation
- Styling details

### Acceptance Criteria

- [ ] Main layout is documented.
- [ ] Sidebar responsibilities are described.
- [ ] Dashboard workspace responsibilities are described.
- [ ] UX open questions are documented.

### Labels

- frontend
- ux
- reporting

---

## Issue: Define PDF export requirements

### Context

Users should be able to export generated reports to PDF.

### Goal

Define what PDF export should include and how it should behave.

### Scope

- PDF content
- PDF layout principles
- Export metadata
- Open questions

### Out of Scope

- PDF generation library selection
- Backend implementation
- Frontend export button implementation

### Acceptance Criteria

- [ ] PDF export content is documented.
- [ ] PDF layout principles are documented.
- [ ] Open questions are captured.
- [ ] Future Excel export assumptions are documented.

### Labels

- export
- reporting

---

## Issue: Integrate the OpenAI .NET SDK behind an Application abstraction

> **Status:** Implemented on 2026-05-29 (on `main`; not filed as a GitHub issue). `IAiChatClient` (Application) + `OpenAiChatClient` (Infrastructure) over a typed `HttpClient`, `OpenAiOptions`, registration test green. See the technical decision "Use the official OpenAI .NET SDK behind an Application abstraction for AI features".

### Context

The AI SQL workflow needs a single, vendor-neutral way to call an OpenAI model for SQL generation and result summarization. The data access and integration strategy is documented in the technical decision "Use the official OpenAI .NET SDK behind an Application abstraction for AI features" and in the backend component mapping in `08-ai-sql-workflow.md`.

### Goal

Add an OpenAI client in the Infrastructure project, hidden behind Application-level interfaces, so AI capabilities can be consumed by workflow handlers without depending on a vendor SDK.

### Scope

- Define Application interfaces for AI capabilities (for example, SQL generation and result summarization).
- Add the official `OpenAI` SDK package to the Infrastructure project.
- Implement the interfaces in Infrastructure using the SDK.
- Register the client through a typed `HttpClient` with timeout configuration.
- Bind `OpenAiOptions` (ApiKey, Model, BaseUrl, TimeoutSeconds) through the options pattern.
- Read the API key from User Secrets / environment variables, not committed configuration.
- Add registration and option-binding tests.

### Out of Scope

- The full AI SQL workflow handler.
- SQL validation logic.
- AdventureWorks query execution.
- Prompt engineering and schema context strategy beyond a minimal first prompt.
- Structured output schemas (tracked as an open question).

### Acceptance Criteria

- [ ] AI capabilities are defined as Application-owned interfaces.
- [ ] The Application project does not reference the OpenAI SDK directly.
- [ ] The Infrastructure client is registered through a typed `HttpClient`.
- [ ] `OpenAiOptions` is bound through the options pattern.
- [ ] The API key is not present in committed `appsettings.json`.
- [ ] Registration is covered by infrastructure tests.

### Notes

- Depends on the technical decision being accepted.
- Open questions on model selection, structured outputs, schema context, and resilience are tracked in `15-open-questions.md`.

### Labels

- ai
- backend
- infrastructure
- architecture

---

## Issue: Add a Dapper-based read-only AdventureWorks query executor

> **Status:** Implemented on 2026-05-29 (on `main`; not filed as a GitHub issue). `IAdventureWorksQueryExecutor` (Application) + `DapperAdventureWorksQueryExecutor` (Infrastructure) with separate read-only connection string, command timeout, row limit, generic `TabularResult`, and execution-error translation to `QueryExecutionException`. Unit + registration tests green.

### Context

AI-generated SQL must be executed against the AdventureWorks analytical database using read-only credentials. The hybrid data access strategy is documented in the technical decision "Use EF Core for application persistence and Dapper for AdventureWorks query execution".

### Goal

Add a read-only query execution component in the Infrastructure project, behind an Application abstraction, that runs validated SQL against AdventureWorks and returns a generic tabular result.

### Scope

- Define an Application interface for AdventureWorks query execution.
- Add Dapper and `Microsoft.Data.SqlClient` packages to the Infrastructure project.
- Add a separate AdventureWorks connection string with read-only credentials and `ApplicationIntent=ReadOnly`.
- Implement the executor using Dapper over a `SqlConnection`.
- Enforce command timeout and result-size limits.
- Return a generic tabular result (columns, rows, data types, row count, execution metadata).
- Add registration tests and basic execution tests.

### Out of Scope

- SQL validation logic (separate issue).
- AI SQL generation.
- Chart planning and report persistence.
- A typed EF Core model for AdventureWorks.

### Acceptance Criteria

- [ ] AdventureWorks query execution is defined as an Application-owned interface.
- [ ] A separate read-only AdventureWorks connection string is configured.
- [ ] The executor enforces a command timeout and a result-size limit.
- [ ] Results are returned as a generic tabular contract.
- [ ] The executor is registered in Infrastructure dependency injection.
- [ ] Registration is covered by infrastructure tests.

### Notes

- The exact generic tabular result contract is a documented follow-up decision.
- The executor must only ever receive SQL that has passed the SQL safety validator.

### Labels

- backend
- infrastructure
- data-model
- security

---

## Issue: Implement the SQL safety validator for AI-generated SQL

> **Status:** Implemented on 2026-05-29 (on `main`; not filed as a GitHub issue). `ISqlSafetyValidator` (Application) + `SqlSafetyValidator` (Infrastructure): single read-only statement only, all listed destructive keywords blocked, comment stripping, audit logging. 24 unit tests green. Wired into the `GenerateReport` workflow handler before execution.

### Context

Model output is untrusted. Before any AI-generated SQL reaches the AdventureWorks executor, it must pass a safety validator that blocks destructive commands. The safety rules are documented in `08-ai-sql-workflow.md` and `CLAUDE.md`.

### Goal

Add a SQL safety validator in the Application project that rejects unsafe SQL before execution.

### Scope

- Define an Application interface for SQL safety validation.
- Block destructive commands: INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, EXEC, MERGE, CREATE, GRANT, REVOKE.
- Return a clear validation result with the reason for rejection.
- Log validation outcomes for auditing.
- Add unit tests covering allowed and blocked statements.

### Out of Scope

- A full SQL parser (a parser-based approach is a future enhancement).
- Table and column allowlists (future enhancement).
- AI SQL generation and query execution.

### Acceptance Criteria

- [ ] SQL safety validation is defined as an Application-owned interface.
- [ ] All listed destructive commands are blocked.
- [ ] Rejections return a clear reason.
- [ ] Validation outcomes are logged.
- [ ] Allowed and blocked statements are covered by unit tests.

### Notes

- This component is a hard prerequisite for executing any AI-generated SQL.
- Future hardening (parser-based validation, allowlists) is tracked as a follow-up.

### Labels

- ai
- backend
- security
- architecture

---

## Issue: Implement persisted report chat workflow

### Context

The current AI reporting slice can generate SQL, validate it, and execute it against AdventureWorks. The product now needs the ChatGPT-like report experience: a user creates a report from a chat message, continues refining it with follow-up messages, and can reopen the report later with chat history and generated SQL history intact.

### Goal

Add persisted report-centered chat endpoints that create reports, append conversation messages, persist generated SQL attempts, and return renderable report results.

### Scope

- Define application database entities for reports, report conversations, report messages, and generated SQL queries.
- Add EF Core configuration and migration for the report persistence model.
- Add a create-report chat endpoint for the first user message.
- Add a follow-up message endpoint for existing reports.
- Save user messages, assistant messages, generated SQL, validation metadata, execution metadata, token usage, and timestamps.
- Enforce authenticated report ownership on read and write operations.
- Return response contracts that the frontend can use to render the chat panel and report workspace.

### Out of Scope

- PDF export.
- Excel export.
- Manual SQL editing.
- Report sharing between users.
- Multi-conversation branching.
- Full chart planning beyond the current tabular result metadata unless separately scoped.

### Acceptance Criteria

- [ ] A report can be created from an authenticated user's first chat message.
- [ ] The first user message is persisted.
- [ ] The assistant response is persisted.
- [ ] Generated SQL attempts are persisted separately from chat messages.
- [ ] SQL validation and execution outcomes are persisted.
- [ ] Follow-up messages can be appended to an existing user-owned report.
- [ ] Users cannot access or append messages to reports owned by another user.
- [ ] Saved report details can be loaded with metadata, conversation history, and generated SQL history.

### Notes

- The existing `POST /api/reports/generate` endpoint can either remain a temporary non-persistent vertical slice or be replaced by the persisted report creation endpoint.
- The data model follows the technical decision "Model report chat persistence around reports, messages, and generated SQL artifacts".

### Labels

- backend
- reporting
- ai
- data-model
- security

---

## Issue: Add optional AI "conclusions" block to report turns

> **Status:** Implemented on 2026-05-29 (on `main`; not filed as a GitHub issue). Nullable `Conclusions` column on `Report` and `GeneratedSqlQuery` (migration `AddReportConclusions`), optional `conclusions` in the visualization prompt/contract, persistence per turn, DTO + GUI ("Conclusions" panel) support. Backend and frontend tests green. See the technical decision "Model optional AI conclusions as a separate field, not an extension of the summary".

### Context

Every successful report turn already produces a short business **summary** (insights, 2–4 sentences) via the report visualization step. Users want the AI to optionally record deeper analysis, takeaways, or recommendations — but only when it actually adds value, not on every turn. This is a separate, optional element from the always-present summary. Tracked as FR-032 in [04-functional-requirements.md](04-functional-requirements.md) and described in [09-reporting-and-visualization.md](09-reporting-and-visualization.md#ai-conclusions).

### Goal

Let the model attach an optional, free-text **conclusions** block to a report turn, persisted and shown alongside that turn's insights, omitted when the model has nothing useful to add.

### Scope

- Add a nullable `Conclusions` field to `Report` (latest turn) and `GeneratedSqlQuery` (per turn), mirroring the existing nullable `Summary` columns.
- Add an EF Core migration for the two new nullable columns, including the matching `.Designer.cs` file.
- Extend the report presentation contract with an optional `Conclusions` value.
- Extend the visualization system prompt and JSON contract with an optional `conclusions` property; treat absent or empty as "no conclusions".
- Persist conclusions in the report chat workflow for both the report and the turn's `GeneratedSqlQuery`.
- Surface conclusions in the report section and details DTOs and in the GUI report view.

### Out of Scope

- Letting users edit or write their own conclusions.
- Using conclusions as input to further SQL generation.
- Export formatting of conclusions (PDF/Excel) — handled with the export work.

### Acceptance Criteria

- [ ] A turn where the model returns conclusions stores and displays them under that turn's insights.
- [ ] A turn where the model returns no conclusions stores null and renders nothing extra (no empty section or heading).
- [ ] Conclusions follow the language of the user's question.
- [ ] Revisiting a saved report shows the same conclusions that were generated (snapshot behavior).
- [ ] The visualization fallback path never fabricates conclusions when the model output is unusable.
- [ ] The new migration adds both nullable columns and ships with its `.Designer.cs` file.

### Notes

- Keep `Conclusions` strictly advisory text; never executable SQL (prompt-injection surface).
- Decisions on placement, length cap, and exact wording are open in [15-open-questions.md](15-open-questions.md).
- Reuse the nullable-`Summary` column shape for consistency.
- Follows the technical decision "Model optional AI conclusions as a separate field, not an extension of the summary".

### Labels

- ai
- reporting
- backend
- frontend
- data-model

---

# Refactoring Issue Drafts

The following drafts come from a codebase refactoring review. They are technical-debt items: they
change internal structure only and must preserve existing behavior, verified by the existing test
suites. They are prefixed `REF-` and tracked under the "Refactoring and Technical Debt" epic in
[12-backlog.md](12-backlog.md). The supporting decision is "Track refactoring work as a backlog and
record refactoring direction" in [14-technical-decisions.md](14-technical-decisions.md).

---

## Issue: REF-1 Remove the unused GenerateReport vertical slice

### Context

`GenerateReport` (`/api/reports/generate`, `GenerateReportCommand`, its handler, `GenerateReportResponse`,
validator, and `ReportOutcome` usage there) was introduced as a transitional reference slice. The
persisted report chat flow (`CreateReport` and `AddReportMessage` driving `ReportChatWorkflow`) now
covers the same generate-validate-execute path more completely, including persistence and retries.
The frontend does not call `/api/reports/generate` anywhere.

### Goal

Remove the dead slice so there is one canonical SQL generation path.

### Scope

- Delete the `Reports/GenerateReport` application folder and the `Generate` endpoint method.
- Delete the related tests (`GenerateReport` handler/validator tests).
- Confirm `ReportOutcome` is still owned by the persisted chat flow and not orphaned.

### Out of Scope

- Any change to the persisted report chat flow behavior.

### Acceptance Criteria

- [ ] `/api/reports/generate` and its command/handler/response/validator are removed.
- [ ] Related tests are removed; the remaining suites build and pass.
- [ ] `GenerateReportResponse` does not break the OpenAPI document or frontend build.
- [ ] No references to the removed slice remain in code.

### Notes

- Verify nothing else (e.g. Wolverine codegen tests) asserts on the generate endpoint.

### Labels

- refactoring
- backend
- tech-debt

---

## Issue: REF-2 Convert ReportChatWorkflow into an injectable service and decompose ProcessAsync

### Context

`ReportChatWorkflow` is an `internal static` class whose `ProcessAsync` method takes nine parameters,
including five collaborators (`IAiSqlGenerator`, `ISqlSafetyValidator`, `IAdventureWorksQueryExecutor`,
`IReportVisualizer`, `IReportIntentClassifier`). `CreateReportCommandHandler` and
`AddReportMessageCommandHandler` both inject the same cluster of services and forward them (a data
clump). `ProcessAsync` itself is ~170 lines with a retry loop and three near-duplicate failure blocks.

### Goal

Make the workflow an injectable, unit-testable service with smaller methods, and shrink the handler
signatures.

### Scope

- Introduce an injectable workflow service (for example `IReportChatPipeline`) registered in DI,
  holding the collaborators as constructor dependencies.
- Reduce handler signatures so they depend on the pipeline plus the repository.
- Decompose `ProcessAsync` into focused private methods (for example `ApplyRejection`,
  `ApplyExecutionFailure`, `ApplySuccess`, and a single-attempt step), removing the duplicated
  "set Failed / null out Result+Charts / return" blocks.

### Out of Scope

- Changing retry count, prompts, or the refine-target behavior.

### Acceptance Criteria

- [ ] The workflow is a registered service; handlers no longer pass five services through.
- [ ] `ProcessAsync` is broken into small methods with no duplicated failure blocks.
- [ ] Existing report flow tests pass unchanged (behavior preserved).
- [ ] Layer dependency architecture tests still pass.

### Notes

- Keep `MaxSqlAttempts` and context-trimming constants as configuration on the service.

### Labels

- refactoring
- backend
- tech-debt

---

## Issue: REF-3 Extract sample-report seed data out of AppDbContextInitializer

### Context

`AppDbContextInitializer` is ~463 lines, dominated by `SeedSampleReportForAdminAsync` (~265 lines of
hard-coded sample report content). This mixes schema/role/admin bootstrap with demo data.

### Goal

Separate demo data from the database bootstrap so the initializer stays focused.

### Scope

- Move the sample-report content into a dedicated provider or an embedded resource (for example JSON).
- Keep migration, role seeding, and initial-admin seeding in the initializer.
- Gate sample-report seeding behind a clear, configurable switch if it is environment-specific.

### Out of Scope

- Changing what the sample report contains.

### Acceptance Criteria

- [ ] `AppDbContextInitializer` no longer hard-codes the sample report body.
- [ ] Sample-report seeding still works in the environments where it is enabled.
- [ ] Initializer size is materially reduced and responsibilities are separated.

### Labels

- refactoring
- backend
- tech-debt

---

## Issue: REF-4 Generate the Reports API client with Orval

### Context

The project adopted Orval to generate typed clients, mocks, and Zod schemas per API tag. The
`authentication` and `users` tags are generated, but the Reports feature uses a hand-written
`src/lib/report-api.ts` (~190 lines of DTO types, fetch functions, and a query-key factory) that must
be kept in sync with the backend by hand. Reports is the central feature, yet it is the only one that
bypasses the generator.

### Goal

Generate the Reports client from the OpenAPI document like the other tags, removing hand-maintained
duplication.

### Scope

- Ensure the backend `Reports` endpoints are tagged and described well enough for generation
  (status codes, response types, nullability).
- Regenerate with Orval and migrate Reports consumers to the generated hooks/types.
- Remove the hand-written `report-api.ts` types and fetch functions; keep only genuinely custom glue.

### Out of Scope

- Changing report endpoint behavior or response shapes (unless required for clean generation, tracked
  separately).

### Acceptance Criteria

- [ ] A generated `reports` tag exists under `src/api/generated/`.
- [ ] Reports consumers use generated hooks/types; `report-api.ts` duplication is removed.
- [ ] Frontend tests (Vitest, Cypress) pass with the generated client/mocks.

### Notes

- Depends on REF-6 for clean enum typing.
- Coordinate with the Orval and custom-fetch-mutator decisions already recorded.

### Labels

- refactoring
- frontend
- tech-debt

---

## Issue: REF-5 Regenerate the Orval client to drop the stale weather-forecasts tag

### Context

The Weather Forecasts backend slice was removed, but the generated frontend client still contains a
`src/api/generated/weather-forecasts/` tag. The generated client was not refreshed.

### Goal

Bring the generated client back in sync with the current OpenAPI document.

### Scope

- Run the OpenAPI generation (`npm run api:gen`) against the current backend.
- Remove the stale `weather-forecasts` generated artifacts.

### Out of Scope

- Any other client regeneration changes unrelated to the removal.

### Acceptance Criteria

- [ ] `src/api/generated/weather-forecasts/` no longer exists.
- [ ] No code references the removed generated tag.
- [ ] Frontend builds and tests pass.

### Notes

- Quick cleanup; closes a loose end from the Weather Forecasts removal.

### Labels

- refactoring
- frontend
- cleanup

---

## Issue: REF-6 Register a global JsonStringEnumConverter for API responses

### Context

The API does not register a global `JsonStringEnumConverter` for HTTP JSON, so enums such as
`ReportStatus`, `ReportMessageRole`, `SqlValidationStatus`, and `ReportOutcome` serialize as numbers.
The hand-written frontend types reflect this with `'Ready' | ... | number` unions, and generated
OpenAPI types inherit the ambiguity.

### Goal

Make API enum serialization stable as strings so the contract and generated types are unambiguous.

### Scope

- Register `JsonStringEnumConverter` in the Wolverine HTTP / API JSON options.
- Update the OpenAPI document and regenerate the frontend client.
- Simplify frontend enum unions to string-only where applicable.

### Out of Scope

- Renaming enum values.

### Acceptance Criteria

- [ ] API responses serialize the listed enums as strings.
- [ ] OpenAPI document reflects string enums; generated types drop the `number` fallback.
- [ ] Backend and frontend tests pass.

### Notes

- Enables cleaner generation in REF-4. Confirm no existing client relies on numeric enum values.

### Labels

- refactoring
- backend
- api-contract

---

## Issue: REF-7 Extract a shared current-user-id resolution helper

### Context

`ReportsEndpoint` and `UserEndpoints` each define their own `SubjectClaimType = "sub"` constant and an
identical `FindFirstValue("sub") ?? FindFirstValue(ClaimTypes.NameIdentifier)` fallback.

### Goal

Resolve the authenticated user id in one place.

### Scope

- Add a single helper (for example a `ClaimsPrincipal`/`HttpContext` extension) in the Api project.
- Use it from all endpoints that read the current user id.

### Out of Scope

- Changing the claim resolution order or authentication behavior.

### Acceptance Criteria

- [ ] One helper resolves the current user id; duplicated constants/logic are removed.
- [ ] All affected endpoints use the helper.
- [ ] Tests pass.

### Labels

- refactoring
- backend
- tech-debt

---

## Issue: REF-8 Split UserService by responsibility

### Context

`UserService` (~350 lines) handles login, first-password reset, JWT issuance, refresh-token
revocation, user CRUD, and role assignment in one class.

### Goal

Apply single responsibility so each concern is independently testable.

### Scope

- Extract a token service (JWT + refresh-token issuance/revocation).
- Extract an authentication service (login, set-first-password).
- Keep user CRUD and role management in a user-management service.
- Re-register the split services in DI.

### Out of Scope

- Changing authentication rules, token lifetimes, or password policy.

### Acceptance Criteria

- [ ] `UserService` responsibilities are split into focused services.
- [ ] DI registrations are updated; consumers depend on the narrower interfaces.
- [ ] Existing auth/user tests pass unchanged.

### Labels

- refactoring
- backend
- security

---

## Issue: REF-9 Extract shared AI response parsing and JSON options helpers

### Context

`AiSqlGenerator`, `AiReportVisualizer`, and `AiReportIntentClassifier` repeat the same shape: build a
System+User message pair, call `IAiChatClient.CompleteAsync`, then parse with a fallback. Code-fence
extraction regex is duplicated in two of them, and `JsonSerializerOptions` (case-insensitive /
camelCase enum) are re-declared in several places (also in `ReportChatWorkflow` and `ReportMapping`).

### Goal

Centralize the duplicated AI parsing and JSON configuration.

### Scope

- Add a shared helper for code-fence/JSON extraction from model output.
- Provide shared, reusable `JsonSerializerOptions` instances.
- Optionally provide a small template for the "complete then parse" pattern.

### Out of Scope

- Changing prompts or model behavior.

### Acceptance Criteria

- [ ] Code-fence extraction and JSON options live in one place and are reused.
- [ ] AI service unit tests pass unchanged.

### Labels

- refactoring
- backend
- ai

---

## Issue: REF-10 Decompose the large ChatDrawer and AdminPanelPage components

### Context

`ChatDrawer.tsx` (~402 lines) and `AdminPanelPage.tsx` (~353 lines) are the largest frontend files.
The project already uses a decomposition pattern (`use-home-page-controller` plus focused
`home-page` hooks) that these files do not yet follow.

### Goal

Bring these files in line with the existing decomposition pattern.

### Scope

- Extract chat state/logic from `ChatDrawer` into a hook and split presentational subcomponents.
- Split `AdminPanelPage` into section components and a controller hook, mirroring the home page.

### Out of Scope

- Changing UI behavior or layout.

### Acceptance Criteria

- [ ] Both files are reduced to composition plus small subcomponents/hooks.
- [ ] Existing component tests pass; behavior is unchanged.

### Labels

- refactoring
- frontend
- tech-debt

---

## Issue: REF-11 Replace manual JSON round-tripping in report persistence with EF Core value converters

### Context

`Report.ResultJson` and `ChartsJson` are stored as JSON strings, and `ReportMapping` deserializes them
in multiple places with `try/catch`. The "sections, or fallback from report fields" logic in
`CreateSections` is dense because of this manual round-tripping.

### Goal

Let the domain hold typed values so the mapping layer stops manually (de)serializing JSON.

### Scope

- Introduce EF Core value converters (or JSON columns) for the result and chart specifications.
- Simplify `ReportMapping` to consume typed properties.

### Out of Scope

- Changing the stored data shape or adding a migration that rewrites existing rows beyond what the
  converter requires.

### Acceptance Criteria

- [ ] Report result/charts are exposed as typed properties on the entity.
- [ ] `ReportMapping` no longer performs ad-hoc JSON deserialization with `try/catch`.
- [ ] A migration (if needed) is added and existing data still reads correctly.
- [ ] Tests pass.

### Notes

- Lower priority; do after REF-2 to avoid overlapping edits in the report flow.
- **Resolution (will not do as specified):** exposing typed `Result`/`Charts` on the `Report`
  entity requires the Domain layer to reference the Application DTOs `TabularResult` and
  `ChartSpec`. That violates the enforced Domain → Application layer rule (`LayerDependencyTests`),
  which is why the data is stored as JSON strings in Domain and converted in `ReportMapping`
  (Application) in the first place. The duplicated `JsonSerializerOptions` concern was already
  resolved by REF-9 (shared `ReportJson.Options`). Closing as architecturally inconsistent unless
  Domain-owned value objects are introduced first (out of scope).

### Labels

- refactoring
- backend
- data-model
