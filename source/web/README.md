# AdventureWorksAIWorkspace Web

This is the React + TypeScript web application for AdventureWorksAIWorkspace.

## Source Structure

```txt
src/
  app/        # Application routing and top-level app composition
  api/        # OpenAPI-driven API client, custom fetch mutator, generated files
  features/   # Product feature modules
  shared/     # Cross-feature UI, hooks, library helpers, theme support
  test/       # Shared Vitest/MSW test utilities
```

Generated files under `src/api/generated/` are produced by Orval and must not be edited by hand.

## Commands

```powershell
npm run dev
npm run build
npm run test
npm run lint
npm run api:gen
```
