# Technical Decisions

## Purpose

This document records important technical and product decisions for AdventureWorksAIWorkspace.

Use this document as a lightweight Architecture Decision Record log.

---

## Decision: Use separate databases for application data and AdventureWorks data

### Date

2026-05-26

### Status

Accepted

### Context

The application needs to store users, saved reports, generated SQL metadata, tags, favorites, and export history. It also needs to analyze Microsoft AdventureWorks business data.

Mixing application data with AdventureWorks analytical data would make responsibilities unclear and increase risk.

### Decision

Use two separate SQL Server databases:

1. Application database for application-owned data.
2. AdventureWorks database for business analysis data.

### Consequences

Benefits:

- Clear separation of responsibilities.
- Safer access control.
- Easier to keep AdventureWorks read-only.
- Better long-term maintainability.

Trade-offs:

- Requires managing two database connections.
- Requires clear infrastructure configuration.

---

## Decision: Use React, TypeScript, MUI, and MUI Charts for the frontend

### Date

2026-05-26

### Status

Proposed

### Context

The frontend needs a modern dashboard interface with sidebars, charts, tables, and chat-like interaction.

### Decision

Use React with TypeScript, Material UI, and MUI Charts.

### Consequences

Benefits:

- Strong TypeScript support.
- Mature component library.
- Consistent UI system.
- Built-in charting option through MUI Charts.

Trade-offs:

- MUI Charts may have limitations for advanced BI visualizations.
- Future custom visualizations may require additional chart libraries.

---

## Decision: Use .NET 10 REST API for the backend

### Date

2026-05-26

### Status

Proposed

### Context

The backend needs to handle authentication, report persistence, AI orchestration, SQL validation, SQL execution, and export logic.

### Decision

Use .NET 10 REST API as the backend technology.

### Consequences

Benefits:

- Strong ecosystem for APIs.
- Good SQL Server support.
- Good authentication and authorization support.
- Suitable for structured backend architecture.

Trade-offs:

- .NET 10 availability and project templates should be confirmed during implementation.
- Some libraries may need compatibility checks.
