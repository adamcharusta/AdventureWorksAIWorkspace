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

## Question: Should Excel export be part of MVP?

### Context

PDF export is expected, but Excel export may require additional design decisions.

### Possible Options

- Include Excel export in MVP.
- Add Excel export after MVP.
- Support only CSV initially.

### Current Assumption

Excel export should be treated as a future feature.
