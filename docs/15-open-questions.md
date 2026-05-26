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

Authentication approach is not decided yet.

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
