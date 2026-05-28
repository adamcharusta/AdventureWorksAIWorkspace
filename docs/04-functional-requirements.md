# Functional Requirements

## Purpose

This document describes what AdventureWorksAIWorkspace should do from a functional perspective.

## FR-001: User Authentication

The system should support user authentication so reports can be associated with specific users.

## FR-002: Report Creation from Natural Language

The user should be able to create a new report by entering a natural language prompt.

## FR-003: AI Intent Recognition

The system should identify the business intent behind the user's prompt, including metrics, filters, time range, dimensions, and expected output.

## FR-004: SQL Generation

The system should generate SQL queries for the AdventureWorks database based on the user's request.

## FR-005: SQL Validation

The system should validate generated SQL before execution.

## FR-006: SQL Execution

The system should execute validated read-only SQL queries against AdventureWorks.

## FR-007: Chart Recommendation

The system should recommend chart types based on the query result shape and business intent.

## FR-008: Dashboard Rendering

The frontend should render generated reports using charts, tables, KPI cards, and summaries.

## FR-009: Report Persistence

The system should save generated reports to the application database.

## FR-010: Report Conversation History

The system should save conversation messages related to each report.

## FR-011: Recent Reports

The user should be able to view recent reports from the left sidebar.

## FR-012: Favorite Reports

The user should be able to mark and unmark reports as favorites.

## FR-013: Report Tags

The user should be able to assign tags to reports.

## FR-014: Report Search and Filtering

The user should be able to search and filter saved reports.

## FR-015: PDF Export

The user should be able to export a report to PDF.

## FR-016: SQL Query Reuse

The system should store generated SQL queries when useful, so similar future requests may reuse or adapt them to reduce token usage.

## FR-017: AI Explanation

The system should provide an explanation of what was generated, including the business meaning of the report and optionally the SQL query.

## FR-018: Error Handling

The system should show clear error messages when SQL generation, validation, execution, or visualization fails.

## FR-019: Collapsible Left Sidebar

The frontend should provide a collapsible sidebar for report management.

## FR-020: Collapsible Right Sidebar

The frontend should provide a collapsible sidebar for AI chat and report refinement.

## FR-021: Closed User Registration

The system should not allow public self-registration in the MVP.

Only users with the Admin role should be able to create new user accounts.

## FR-022: Role-Based Authorization

The system should support two initial roles:

- Admin
- User

Admin users should be able to manage application users and access administrative functions. User accounts should be limited to their own reports, conversations, favorites, tags, and exports by default.

## FR-023: First Login Password Change

When an account is created with the configured initial template password, the user should be required to change their password during the first login flow before accessing the main application.

This requirement applies to both Admin and User accounts, including the first bootstrapped Admin account.

## FR-024: Initial Admin Bootstrap

The system should support bootstrapping the first Admin account from secure startup configuration so the application can be initialized without public registration.

The first Admin account should be created with the configured initial template password and must be marked as requiring password change on first login.

Admin bootstrap credentials must not be committed to the repository.

## FR-025: JWT-Based Login

The system should authenticate users by issuing a JSON Web Token (JWT) on successful login.

The login endpoint should accept either user name or email as the identifier, together with the user's password.

The issued access token should include the user identifier (sub), user name (name), and assigned role(s) (role) as claims, so the frontend and downstream authorization can identify the user and apply role-based rules without an additional database lookup.

If the user account is marked as requiring a password change on first login, the login endpoint must not issue a normal access token. Instead it should return a response that indicates the password change requirement and routes the user through the first-login password change flow defined in FR-023.

The access token signing key must not be committed to the repository and must be provided through secure configuration (User Secrets for local development, environment variables or a secret store for deployed environments).

## FR-026: Refresh Token Rotation

The system should issue a refresh token alongside the access token at login so the client can obtain a new access token after expiration without prompting the user to log in again.

Refresh tokens should be persisted in the application database in a non-reversible form (hash), bound to the issuing user, and have an explicit expiration time.

The refresh endpoint should rotate the refresh token: the presented refresh token is revoked and a new one is issued together with a new access token. Reuse of a revoked refresh token must be rejected.

Refresh tokens should be revocable on logout and on password change, so existing sessions cannot continue after a security-relevant action.

## FR-027: Admin User Deletion

Admin users should be able to delete user accounts from the admin panel.

Only users with the Admin role may delete user accounts.

The system must prevent an authenticated Admin user from deleting their own account.
