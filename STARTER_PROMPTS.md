# Starter Prompts

## Initial Prompt for Codex or Claude

```txt
We are starting a new project called AdventureWorksAIWorkspace.

Before writing any application code, read and follow the rules from AGENTS.md and/or CLAUDE.md.

Important: do not generate production code yet. Your current role is Project Manager, Business Analyst, Product Analyst, Technical Analyst, and Documentation Assistant.

The project is an AI-powered business intelligence dashboard application connected to the ChatGPT/OpenAI API. It will analyze the Microsoft AdventureWorks database. Users will ask business questions in natural language, for example:

"Show me a sales report for product X in Q3 of year Y in region Z."

The AI should understand the request, generate SQL for AdventureWorks, validate it, execute it, and compose a dashboard report using appropriate charts such as line charts, bar charts, pie charts, tables, KPIs, and summaries.

Reports should be saved like conversations in ChatGPT or Claude. The user should be able to see recent reports, reopen previous reports, mark reports as favorites, tag them, and export them to PDF. Excel export may be added in the future.

Planned stack:
- Backend: .NET 10 REST API
- Application database: MSSQL
- Analytical/business database: Microsoft AdventureWorks SQL Server database
- Frontend: React + TypeScript
- UI library: Material UI / MUI
- Charts: MUI Charts
- AI integration: ChatGPT/OpenAI API

The main UI concept:
- Left collapsible sidebar for report management, recent reports, favorites, tags, and saved reports.
- Center workspace for the generated dashboard, charts, tables, summaries, and export actions.
- Right collapsible sidebar for AI chat, follow-up questions, report refinement, and query explanations.

Language rule:
All repository files, documentation, backlog items, GitHub issue drafts, technical decisions, open questions, diagrams, README updates, AGENTS.md updates, and CLAUDE.md updates must be written in English.

The user may communicate in Polish, but repository content must always be created and maintained in English.

If the user provides requirements in Polish, translate them into clear English before saving them into documentation.

You may answer the user in Polish during the conversation, but anything written to the repository must be in English.

Your first task:
1. Review the current documentation structure in the /docs folder.
2. Update README.md only if the high-level project description changes.
3. Improve documentation where needed.
4. Break the product idea into smaller business and technical tasks.
5. Create or update GitHub issue drafts in Markdown.
6. Add open questions where assumptions are unclear.
7. Do not create .NET, React, SQL execution, authentication, OpenAI integration, or production source code yet.

Use English for all repository documentation.
When making assumptions, clearly document them.
When identifying risks, document them.
When a technical decision is proposed, add it to the technical decisions document.
```

## Prompt for Updating Documentation After a New Requirement

```txt
Update the project documentation according to the latest requirement below.

Requirement:
[PASTE NEW REQUIREMENT HERE]

Rules:
- Do not write production code.
- Update the relevant files in /docs.
- Update README.md only if the high-level project description changes.
- Add or update backlog items.
- Add GitHub issue drafts if new implementation tasks appear.
- Add open questions if something is unclear.
- Add technical decisions if this requirement affects architecture, stack, security, data model, AI workflow, or reporting behavior.
- All repository content must be written in English, even if the requirement is provided in Polish.
- Keep tasks small and actionable.
```

## Prompt for Creating Real GitHub Issues

```txt
Create real GitHub issues from docs/13-github-issues.md.

Rules:
- Do not modify production code.
- First check whether GitHub access is available.
- If GitHub CLI is available, use `gh issue create`.
- If GitHub MCP tools are available, use them.
- If neither is available, stop and explain what access is missing.
- Do not create more than 10 issues without asking for confirmation.
- Do not create duplicate issues.
- Preserve the issue title, context, goal, scope, out of scope, acceptance criteria, and notes.
- Apply labels only if they already exist or if the tooling supports creating them safely.
- After creating issues, update docs/13-github-issues.md with GitHub issue links or issue numbers.
- All issue titles and bodies must be written in English.
```
