# AdventureWorksAIWorkspace

AdventureWorksAIWorkspace is an AI-powered business intelligence workspace for analyzing the Microsoft AdventureWorks SQL Server database using natural language prompts.

The application will allow users to ask business questions, generate safe SQL queries with AI assistance, execute those queries against AdventureWorks, and display the results as dashboard reports with charts, tables, KPIs, and summaries.

## Project Status

This project is currently in the planning and documentation phase.

Production code should not be generated until the user explicitly requests implementation.

## Core Idea

A user can ask a question such as:

> Show me a sales report for product X in Q3 of year Y in region Z.

The system should:

1. Understand the business intent.
2. Generate or reuse an optimized SQL query.
3. Validate the SQL query for safety.
4. Execute the query against the AdventureWorks database.
5. Decide which visualizations should be displayed.
6. Render a dashboard report.
7. Save the report as a conversation-like artifact.
8. Allow the user to reopen, tag, favorite, and export reports.

## Planned Technology Stack

### Backend

- .NET 10 REST API
- MSSQL for application data
- Authentication and authorization
- OpenAI / ChatGPT API integration
- SQL query persistence and reuse
- Report persistence
- PDF export support

### Analytical Database

- Microsoft AdventureWorks SQL Server database
- Separate from the application database
- Used as the business data source for reports and analysis

### Frontend

- React
- TypeScript
- Material UI / MUI
- MUI Charts
- Dashboard-oriented layout
- Collapsible sidebars
- AI chat panel

## Main UI Concept

The main application view should contain:

- Left collapsible sidebar for recent reports, saved reports, favorite reports, tags, and report management.
- Center workspace for dashboards, charts, tables, summaries, insights, and export actions.
- Right collapsible sidebar for AI chat, follow-up questions, report refinement, and query explanations.

## Documentation

Project documentation is maintained in the `/docs` folder.

Recommended reading order:

1. `docs/00-project-overview.md`
2. `docs/01-business-requirements.md`
3. `docs/03-user-flows.md`
4. `docs/04-functional-requirements.md`
5. `docs/06-architecture.md`
6. `docs/08-ai-sql-workflow.md`
7. `docs/12-backlog.md`
8. `docs/15-open-questions.md`

## Agent Rules

AI agents working in this repository must follow:

- `AGENTS.md`
- `CLAUDE.md`, when using Claude

The default role of agents is project manager, business analyst, technical analyst, and documentation assistant.

Agents must not generate production code until explicitly requested.

## Language Rule

All repository content must be written in English.

The user may communicate with agents in Polish, but documentation, backlog items, GitHub issue drafts, technical decisions, and all repository files must be created and maintained in English.
