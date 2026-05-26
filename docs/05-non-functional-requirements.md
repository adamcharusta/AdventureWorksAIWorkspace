# Non-Functional Requirements

## Purpose

This document describes quality attributes and constraints for AdventureWorksAIWorkspace.

## Security

- Generated SQL must be validated before execution.
- The AdventureWorks database should be accessed using read-only permissions.
- Destructive SQL commands must be blocked.
- Application secrets must not be exposed to the frontend.
- OpenAI API keys must be stored securely.
- Users should only access their own reports unless sharing is explicitly implemented.

## Performance

- Report generation should avoid unnecessary AI calls when cached or reusable SQL is available.
- Query execution should have reasonable timeout limits.
- Query result sizes should be limited to protect the application and database.
- The dashboard should remain responsive when displaying charts and tables.

## Reliability

- The system should handle AI generation failures gracefully.
- SQL validation failures should produce clear user-facing messages.
- Database execution errors should be logged and explained safely.
- Report persistence should not lose conversation history.

## Maintainability

- Business requirements and technical decisions should be documented.
- AI workflow should be separated from SQL execution logic.
- Application database and AdventureWorks database should remain separate.
- Frontend components should be organized around dashboard, report management, and chat features.

## Observability

- Generated SQL should be logged for debugging and audit purposes.
- AI request metadata should be traceable without exposing sensitive information.
- Report generation failures should be measurable.

## Cost Control

- The system should consider storing reusable query patterns.
- Prompts should include only necessary context.
- Large report contexts should be summarized before reuse.
- Future implementation should consider token usage monitoring.

## Usability

- Users should not need to know SQL.
- Generated reports should be readable and business-oriented.
- The main dashboard area should remain the visual focus.
- Sidebars should be collapsible to maximize report space.
