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
