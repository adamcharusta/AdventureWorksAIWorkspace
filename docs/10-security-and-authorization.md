# Security and Authorization

## Purpose

This document describes initial security and authorization considerations for AdventureWorksAIWorkspace.

## Authentication

The system should support authenticated users.

Authentication is needed because reports, favorites, tags, and conversation history are user-specific.

## Authorization

Users should only access their own reports by default.

Future authorization may include:

- Admin role.
- Report sharing.
- Organization/workspace-level permissions.

## Database Security

The application should use two separate databases:

1. Application database.
2. AdventureWorks analytical database.

The AdventureWorks database should be accessed through read-only credentials.

## AI-Generated SQL Safety

AI-generated SQL must never be trusted blindly.

The system should validate SQL before execution.

Forbidden commands should include:

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

## Prompt Injection Considerations

The system should be designed to reduce prompt injection risk.

Potential safeguards:

- Never allow the AI to override system rules.
- Do not expose secrets in prompts.
- Use structured prompts.
- Validate SQL independently of AI responses.
- Treat AI output as untrusted until validated.

## Secret Management

The following values must be stored securely:

- OpenAI API key.
- Application database connection string.
- AdventureWorks database connection string.
- Authentication secrets.

Secrets must not be committed to the repository.

## Audit and Logging

The system should consider logging:

- Generated SQL.
- SQL validation results.
- SQL execution errors.
- Report generation requests.
- Export actions.

Logs should not expose sensitive user data or secrets.

## Open Questions

- Which authentication mechanism should be used?
- Should there be an admin panel for generated SQL audit?
- Should users be able to see generated SQL by default?
- How strict should SQL validation be in the MVP?
