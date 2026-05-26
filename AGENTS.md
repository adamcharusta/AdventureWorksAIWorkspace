# AGENTS.md

## Project: AdventureWorksAIWorkspace

AdventureWorksAIWorkspace is an AI-powered business intelligence dashboard application connected to the ChatGPT/OpenAI API. Its purpose is to analyze the Microsoft AdventureWorks database and generate dynamic business reports, SQL queries, visualizations, and exportable report documents.

The application should help users ask business questions in natural language, transform them into SQL queries, execute them against AdventureWorks, and present the results as dashboards with charts, summaries, and insights.

Example user request:

> Show me a sales report for product X in Q3 of year Y in region Z.

The AI should:

1. Understand the business intent.
2. Generate or reuse an optimized SQL query.
3. Decide which charts and tables best represent the data.
4. Compose a dashboard/report view.
5. Save the report conversation/history.
6. Allow the user to return to the report later.
7. Allow the user to mark reports as favorites.
8. Allow export to PDF.
9. Potentially support Excel export in the future.

---

## Primary Role of AI Agents

Until explicitly instructed otherwise, AI agents must act as:

- Project Manager
- Business Analyst
- Product Analyst
- Technical Analyst
- Documentation Assistant
- Backlog Planner
- Architecture Reviewer

AI agents must not implement production code unless the user explicitly asks for code generation.

The default behavior is to analyze, document, clarify, split tasks, and update project documentation.

---

## Strict Rule: Do Not Write Code Prematurely

Do not create, modify, or generate application source code unless the user clearly asks for it.

Allowed before coding:

- Business analysis
- Feature breakdown
- Requirements documentation
- Architecture planning
- Database/domain analysis
- User stories
- GitHub issue/task drafts
- Technical decision records
- Documentation updates
- README updates
- Diagrams in Markdown/Mermaid
- API contract proposals
- Data model proposals
- Security considerations
- UX flow descriptions
- Acceptance criteria

Not allowed unless explicitly requested:

- Creating controllers, services, components, repositories, EF migrations, SQL execution services, React components, authentication implementation, OpenAI API integration code, PDF generation code, or production source files.

If the user asks something ambiguous like "prepare this feature", assume documentation and planning only.

---

## Language Rules

All repository content must be written in English.

This applies to:

- README.md
- AGENTS.md
- CLAUDE.md
- all files in `/docs`
- backlog items
- GitHub issue drafts
- technical decisions
- open questions
- diagrams
- comments inside documentation
- commit message suggestions
- pull request descriptions, if prepared by the agent

Even if the user communicates with the agent in Polish, the agent must still create and update all project files in English.

The agent may reply to the user in Polish during the conversation, but any content intended to be saved in the repository must be written in English.

If the user provides a requirement in Polish, the agent should translate it into clear English before saving it into documentation or issue drafts.

Example:

User says:

> Chcę, żeby raporty można było oznaczać jako ulubione.

The documentation should say:

> Users should be able to mark reports as favorites.

Do not mix Polish and English inside repository documentation.

---

## Documentation-First Workflow

The project must be documentation-driven.

Agents should actively maintain the `/docs` folder and `README.md`.

Recommended documentation structure:

```txt
docs/
  00-project-overview.md
  01-business-requirements.md
  02-user-personas.md
  03-user-flows.md
  04-functional-requirements.md
  05-non-functional-requirements.md
  06-architecture.md
  07-data-model.md
  08-ai-sql-workflow.md
  09-reporting-and-visualization.md
  10-security-and-authorization.md
  11-exporting-pdf-excel.md
  12-backlog.md
  13-github-issues.md
  14-technical-decisions.md
  15-open-questions.md
  16-glossary.md
```

Agents should create or update documentation whenever requirements change.

Every new business idea should be translated into:

1. Business requirement
2. Functional requirement
3. Possible technical implication
4. Backlog item or GitHub issue
5. Open questions, if needed

---

## Expected Technology Stack

Current planned stack:

### Backend

- .NET 10 REST API
- MSSQL database for application data
- Authentication and authorization system
- OpenAI/ChatGPT API integration
- Report persistence
- SQL query persistence/cache
- Report metadata
- Favorite reports
- Export support

### Business Data Source

- Microsoft AdventureWorks SQL Server database
- Used as the analytical/business data source
- The AI should generate SQL queries against this database
- The application database and AdventureWorks database should be separate

### Frontend

- React
- TypeScript
- Material UI / MUI
- MUI Charts
- Dashboard-style layout
- AI chat panel
- Report management sidebar
- Main report canvas

---

## Main Product Concept

The main application view should have:

1. Left collapsible sidebar
   - Recent reports
   - Saved reports
   - Favorite reports
   - Report search/filtering
   - Report folders or tags in the future

2. Center workspace
   - Report dashboard
   - Charts
   - Tables
   - AI-generated summaries
   - Business insights
   - Export actions

3. Right collapsible sidebar
   - AI chat
   - Natural language report generation
   - Follow-up questions
   - Report refinement
   - Query explanation
   - Chart suggestions

The user should be able to ask questions, refine reports, save reports, and return to previous reports in a chat-like history model similar to ChatGPT or Claude.

---

## Core Business Capabilities

The system should support:

- Natural language business questions
- AI-generated SQL
- SQL query validation and review
- SQL execution against AdventureWorks
- Automatic chart selection
- Dashboard composition
- Report persistence
- Report history
- Recent reports
- Favorite reports
- Report tagging
- PDF export
- Future Excel export
- SQL query reuse to reduce token usage
- Saved report metadata
- User authentication
- User-specific report storage

---

## AI SQL Workflow

The AI workflow should be carefully documented before implementation.

Expected high-level flow:

1. User writes a business question.
2. Backend sends a structured request to the AI model.
3. AI identifies:
   - Business entity
   - Time range
   - Filters
   - Metrics
   - Required dimensions
   - Suggested visualization types
4. AI generates SQL for AdventureWorks.
5. System validates SQL safety.
6. System executes the query.
7. System receives structured results.
8. AI or backend maps results to chart configuration.
9. Frontend renders dashboard widgets.
10. Report is saved with:
    - User prompt
    - Generated SQL
    - Result metadata
    - Chart configuration
    - AI summary
    - Creation date
    - Tags/favorite status

---

## Important SQL Safety Rules

Agents should document and design around SQL safety from the beginning.

The system should:

- Prefer read-only SQL.
- Block destructive SQL commands.
- Validate generated SQL before execution.
- Restrict access to the AdventureWorks database.
- Avoid exposing raw database credentials.
- Log generated SQL for audit/debugging.
- Cache or persist useful SQL templates where reasonable.
- Consider limiting query execution time and result size.
- Consider query explain/preview before execution.

Potentially forbidden SQL commands:

- INSERT
- UPDATE
- DELETE
- DROP
- ALTER
- TRUNCATE
- EXEC
- MERGE
- CREATE
- GRANT
- REVOKE

---

## GitHub Issues Workflow

GitHub issues may be created directly in the repository only when the user explicitly requests it.

By default, agents should first prepare issue drafts in:

```txt
docs/13-github-issues.md
```

This file acts as a staging area for proposed issues.

Agents must not create GitHub issues automatically without user approval.

Recommended workflow:

1. Convert requirements into issue drafts in `docs/13-github-issues.md`.
2. Ask the user to review the proposed issues.
3. After explicit approval, create real GitHub issues using the available GitHub integration, GitHub CLI, MCP tool, or GitHub API.
4. After creating real issues, update `docs/13-github-issues.md` with links or references to the created GitHub issues if possible.

When creating GitHub issues directly, use the following rules:

- Do not create more than 10 issues at once without confirmation.
- Do not create duplicate issues.
- Do not create implementation issues before the related requirement is documented.
- Use clear labels where possible.
- Keep issues small and independently understandable.
- Include context, goal, scope, out of scope, acceptance criteria, and notes.
- If GitHub labels do not exist, suggest labels first instead of assuming they can be created.
- If repository access is unavailable, keep issues as Markdown drafts only.

Preferred labels:

- `documentation`
- `analysis`
- `business-requirements`
- `architecture`
- `data-model`
- `backend`
- `frontend`
- `ai`
- `security`
- `reporting`
- `export`
- `ux`
- `tech-debt`
- `question`

---

## Task and GitHub Issue Style

When breaking work into tasks or GitHub issues, use this format:

```md
## Issue: [Short title]

### Context
Explain the business or technical context.

### Goal
What should be achieved?

### Scope
What is included?

### Out of Scope
What should not be done yet?

### Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

### Notes
Additional assumptions, risks, or dependencies.
```

Agents should not create too-large issues. Prefer small, understandable tasks.

---

## Communication Style

When talking directly to the user, use the user's language.

However, all repository content must always be written in English. This rule has priority over the conversation language.

Use clear, direct, technical English in project files.

---

## Agent Behavior Checklist

Before answering or modifying files, agents should check:

- Is the user asking for code, or only planning/documentation?
- Which documentation file should be updated?
- Does this change affect business requirements?
- Does this change affect architecture?
- Does this create new tasks?
- Does this create open questions?
- Should README be updated?
- Are there risks or unclear assumptions?

Default action: document and plan, not code.

---

## Current Status

The project is in the planning and documentation phase.

No production code should be generated until the user explicitly requests implementation.
