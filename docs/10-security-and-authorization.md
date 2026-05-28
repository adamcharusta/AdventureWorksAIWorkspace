# Security and Authorization

## Purpose

This document describes initial security and authorization considerations for AdventureWorksAIWorkspace.

## Authentication

The system should support authenticated users through ASP.NET Core Identity backed by the application database.

Authentication is needed because reports, favorites, tags, and conversation history are user-specific.

Public self-registration should not be available in the MVP.

User accounts should be created by an Admin user. New users should be created with the configured initial template password and forced to change it during the first login flow before accessing the main application.

Admin users may delete other user accounts, but the API must prevent an authenticated Admin from deleting their own account.

The first Admin account should be bootstrapped through secure startup configuration so the application can be initialized without opening public registration. The first Admin account should receive the same configured initial template password rule and must change the password on first login inside the application.

## JWT Authentication

The API should authenticate clients using bearer JSON Web Tokens (JWT).

Login flow:

1. The client submits an identifier (user name or email) and a password to the login endpoint.
2. The server validates the credentials through ASP.NET Core Identity.
3. If the user account requires a password change on first login, the login endpoint must reject the login with a response that signals the first-login flow, and must not issue a normal access token.
4. Otherwise the server issues a signed JWT access token together with a refresh token.

Access token claims:

- `sub` carries the user identifier.
- `name` carries the user name.
- `role` carries the assigned role names (Admin, User).
- Standard claims such as `iss`, `aud`, `exp`, and `iat` must be present.

Token signing:

- Tokens must be signed with a symmetric key supplied through secure configuration.
- The signing key must not be committed to the repository. Use User Secrets for local development and environment variables or a production secret store for deployed environments.
- Token validation must verify issuer, audience, lifetime, and signature.

Refresh tokens:

- A refresh token is issued at login alongside the access token and persisted in the application database as a hash (not in plain form), associated with the issuing user.
- The refresh endpoint accepts the refresh token, validates that it is still active, revokes it, and issues a new access token together with a new refresh token.
- Reuse of a revoked refresh token must be rejected.
- Refresh tokens should be revocable on logout and on password change.

Access token lifetime should be short (for example minutes). Refresh token lifetime should be longer but still bounded (for example days).

## Authorization

Users should only access their own reports by default.

The MVP should include two roles:

- Admin
- User

Admin users may manage users, assign roles, deactivate users, access audit-oriented views, and perform future administrative operations.

User accounts may create reports, refine reports, save reports, mark reports as favorites, manage their own tags, and export their own reports.

Authorization should use policies for reusable rules even when those policies initially map to roles. Initial policy examples:

- RequireAuthenticatedUser
- RequireAdmin
- OwnReportAccess
- ManageUsers

Future authorization may include report sharing and organization/workspace-level permissions.

## User Provisioning

The preferred user provisioning flow is:

1. Admin creates a user account.
2. The system assigns the User role by default unless the Admin explicitly assigns another role.
3. The account is created with the configured initial template password.
4. The account is marked as requiring password change.
5. The user changes the temporary password during the first login flow.
6. The temporary password can no longer be used after the password change.
7. The user can access the main application only after the password requirement is satisfied.

This flow applies to regular User accounts and to Admin accounts created by an existing Admin.

For security, temporary passwords must require immediate change on first login.

When an Admin deletes a user account, the action should be restricted to non-current accounts so the active administrator cannot remove their own access.

For MVP, the configured initial template password may be shared across pre-provisioned accounts, including the first Admin account. It must be provided through secure configuration and treated as a secret.

## Initial Admin Bootstrap

The system needs a safe way to create the first Admin account.

Recommended approach:

1. Use startup configuration to define whether initial Admin seeding is enabled.
2. Use startup configuration to define the initial Admin username or email.
3. Use the configured initial template password for the initial Admin account.
4. Assign the Admin role to the initial Admin account.
5. Mark the initial Admin account as requiring password change.
6. Require the Admin to change the temporary password immediately after the first successful login.
7. Disable or make bootstrap idempotent after the initial Admin account exists.

The initial Admin template password follows the same temporary password rule as other pre-provisioned accounts: it must be treated as a secret and must not remain valid after first login.

Store secret bootstrap values, such as the initial Admin template password, outside committed configuration files.

Use development User Secrets for local development.

Use environment variables or a production secret store for deployed environments.

Plain-text temporary passwords and Admin bootstrap passwords must not be committed to `appsettings.json`, source code, documentation examples with real values, or version control.

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
- Initial Admin bootstrap/template password.
- Shared initial template password, if used for MVP or local development.

Secrets must not be committed to the repository.

## Audit and Logging

The system should consider logging:

- User creation.
- User deletion.
- Role assignment changes.
- Account activation and deactivation.
- First login password change completion.
- First Admin bootstrap result.
- Generated SQL.
- SQL validation results.
- SQL execution errors.
- Report generation requests.
- Export actions.

Logs should not expose sensitive user data or secrets.

## Open Questions

- Should there be an admin panel for generated SQL audit?
- Should users be able to see generated SQL by default?
- How strict should SQL validation be in the MVP?
- Should production user onboarding keep the shared initial template password model, or move to unique temporary passwords or one-time setup links?
