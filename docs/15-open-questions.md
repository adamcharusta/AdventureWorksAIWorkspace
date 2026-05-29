# Open Questions

## Purpose

This document tracks unresolved product, business, and technical questions.

---

## Question: Should generated SQL be visible by default?

### Context

The system will generate SQL queries from natural language prompts. Some users may benefit from seeing SQL, while non-technical users may find it distracting.

### Possible Options

- Show SQL by default.
- Hide SQL behind an advanced/details section.
- Allow the user to choose a preference.

### Current Assumption

Generated SQL should be hidden by default but available in a technical details section.

---

## Question: Should reports store raw result data or only SQL and chart configuration?

### Context

Saved reports need to be reopened later. Storing raw data improves historical consistency, but increases storage usage and potential data freshness issues.

### Possible Options

- Store raw result snapshots.
- Store only SQL and regenerate results.
- Store summarized metadata and optionally cache limited result data.

### Current Assumption

For MVP, store SQL, chart configuration, summary, and metadata. Raw result snapshot storage should be evaluated later.

---

## Question: Should users be able to manually edit generated SQL?

### Context

Technical users may want to adjust SQL, but this increases safety and UX complexity.

### Possible Options

- Do not allow manual SQL editing.
- Allow editing only in advanced mode.
- Allow editing only for admin users.

### Current Assumption

Manual SQL editing should not be part of the MVP.

---

## Question: Which authentication approach should be used?

### Context

Reports are user-specific, so authentication is required.

### Possible Options

- ASP.NET Identity.
- External OAuth provider.
- Simple local authentication for demo purposes.

### Current Assumption

Use ASP.NET Core Identity backed by EF Core and the application database.

Public self-registration should be disabled for the MVP. Admin users should create user accounts with the configured initial template password, and newly created users should be required to change their password during first login.

The first bootstrapped Admin account should follow the same rule: it starts with the configured initial template password and must require password change on first login inside the application.

---

## Question: How should temporary passwords be handled beyond MVP?

### Context

Admin users will create accounts for other users, and the first Admin account will be bootstrapped during startup. The MVP uses a configured initial template password with forced password change on first login. The longer-term production flow may need a stronger delivery mechanism.

### Possible Options

- Use a shared initial template password provided through secure configuration and force password change on first login.
- Generate unique temporary passwords and force password change on first login.
- Send a one-time password setup link by email.
- Generate a password reset token and require immediate password setup.

### Current Assumption

For MVP, the shared initial template password is acceptable if it is treated as a secret and requires immediate change on first login. The first bootstrapped Admin account must follow the same rule. For production hardening, evaluate unique temporary passwords, one-time setup links, or password reset tokens.

---

## Question: How should the first-login password change flow be exposed when JWT login is rejected?

### Context

Login rejects users whose accounts are marked as requiring a password change. They cannot obtain a normal access token, so a dedicated mechanism is needed to let them change the temporary password.

### Possible Options

- Return a short-lived, single-purpose password-change token alongside the rejection response, accepted only by the change-password endpoint.
- Require the client to submit the temporary password again on the change-password endpoint, without issuing any intermediate token.
- Send a password reset link by email and reuse the standard reset flow.

### Current Assumption

For MVP, the login endpoint rejects the login with a clear "password change required" response. The exact mechanism for the change-password endpoint (token vs. re-submitted credentials) is left to FR-023 implementation.

---

## Question: Which OpenAI model should be the default, and should tasks use different models?

### Context

The backend will call an OpenAI model for SQL generation, chart suggestions, and business summaries. These tasks differ in difficulty and cost sensitivity. A single model is simpler, but task-specific models could balance quality against token cost.

### Possible Options

- Use one default model for all AI tasks.
- Use a stronger model for SQL generation and a cheaper model for summaries.
- Make the model configurable per task through options.

### Current Assumption

Start with a single configurable default model bound through `OpenAiOptions`. Revisit task-specific models after measuring quality and cost on representative reports.

---

## Question: Should AI responses use structured outputs (JSON schema)?

### Context

SQL generation and chart suggestions need predictable, machine-readable output. Free-form text requires fragile parsing, while structured outputs constrain the model to a known shape.

### Possible Options

- Use structured outputs / JSON schema responses for SQL and chart suggestions.
- Use plain text responses and parse them in the Application layer.
- Mix both depending on the task.

### Current Assumption

Prefer structured outputs for SQL and chart suggestions so the Application layer receives predictable shapes. Plain text may remain acceptable for human-facing summaries.

---

## Question: How much AdventureWorks schema context should be sent to the model?

### Context

The model needs schema knowledge to generate correct SQL, but sending the full AdventureWorks schema increases token cost and may expose unnecessary structure. This also affects prompt-injection surface.

### Possible Options

- Send the full AdventureWorks schema.
- Send a curated semantic subset of tables and columns.
- Retrieve only schema fragments relevant to the question.

### Current Assumption

For MVP, send a curated semantic subset of relevant tables and columns. Evaluate retrieval-based schema context if accuracy or token cost becomes a problem.

---

## Question: What resilience strategy should outbound AI calls use?

### Context

Outbound calls to the OpenAI API add latency and a new failure mode (timeouts, rate limits, transient errors). The user experience must degrade gracefully when the model is slow or unavailable.

### Possible Options

- Timeout only.
- Timeout plus limited retries with backoff.
- Timeout, retries, and a circuit breaker.

### Current Assumption

Start with a request timeout and limited retries with backoff on the typed `HttpClient`. Add a circuit breaker if rate limiting or outages prove disruptive.

---

## Question: Should Excel export be part of MVP?

### Context

PDF export is expected, but Excel export may require additional design decisions.

### Possible Options

- Include Excel export in MVP.
- Add Excel export after MVP.
- Support only CSV initially.

### Current Assumption

Excel export should be treated as a future feature.

---

## Question: Should a report have one conversation or support multiple branches?

### Context

The MVP chat model attaches one conversation to one report. This keeps persistence and UI simpler, but future report refinement could benefit from branching, where users compare alternative analyses without overwriting the main report path.

### Possible Options

- One conversation per report for MVP.
- Multiple conversations per report, with one marked as active.
- Report versions, where each version has its own conversation and generated SQL history.

### Current Assumption

Use one active conversation per report for MVP. Revisit branching only after the basic saved report chat workflow is stable.

---

## Question: How should report titles be created?

### Context

Reports need titles for the sidebar and saved report list. The title could come from the user, be generated by AI from the first prompt, or start as an AI-generated default that the user can edit.

### Possible Options

- Require the user to enter a title before generating the report.
- Generate a title from the first prompt.
- Generate a title automatically and allow the user to rename it later.

### Current Assumption

Generate a short title from the first prompt and allow the user to rename it later.

---

## Question: Should generated SQL link to source and assistant messages?

### Context

Generated SQL belongs to a report, but for auditing and explainability it is useful to know which user message caused it and which assistant response presented the result.

### Possible Options

- Link generated SQL only to the report.
- Link generated SQL to the source user message.
- Link generated SQL to both the source user message and the assistant message that used it.

### Current Assumption

Link generated SQL to the report and source user message in the MVP. Add assistant-message linking later if the UI needs exact traceability from a chat bubble to a SQL artifact.

---

## Question: What should happen when persistence, AI generation, validation, or SQL execution fails mid-flow?

### Context

The persisted report chat workflow includes several steps: saving the user message, calling AI, saving generated SQL, validating SQL, executing SQL, saving assistant messages, and updating report status. Failures can happen at different stages.

### Possible Options

- Use a single transaction around database writes and persist only successful report generations.
- Persist partial state with statuses such as `Failed`, `Rejected`, or `ExecutionFailed`.
- Persist the user message first, then append assistant failure messages when downstream steps fail.

### Current Assumption

Persist the user message and report record, then save downstream outcomes as status-bearing artifacts. Validation rejections and execution failures should remain visible in the report conversation instead of disappearing.
