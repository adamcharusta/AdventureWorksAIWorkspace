# Project Overview

## Project Name

AdventureWorksAIWorkspace

## Summary

AdventureWorksAIWorkspace is an AI-powered business intelligence workspace for analyzing the Microsoft AdventureWorks SQL Server database using natural language prompts.

The application should allow users to ask business questions, generate SQL queries with AI assistance, execute those queries safely against AdventureWorks, and display the results as interactive dashboard reports.

## Problem Statement

Traditional business intelligence tools usually require users to understand data structures, SQL, metrics, filters, and chart configuration. This creates friction for users who want fast business insights but do not want to manually build queries and dashboards.

AdventureWorksAIWorkspace should reduce that friction by allowing users to ask business questions naturally and receive generated reports with charts, tables, KPIs, and summaries.

## Example Scenario

A user asks:

> Show me a sales report for product X in Q3 of year Y in region Z.

The system should:

1. Understand the requested product, period, and region.
2. Generate a safe SQL query for AdventureWorks.
3. Execute the query.
4. Select useful visualizations.
5. Display a report dashboard.
6. Save the report as a reusable conversation-like artifact.

## Product Goals

- Make AdventureWorks data explorable through natural language.
- Generate business reports without requiring manual SQL writing.
- Present results as useful dashboards rather than raw data only.
- Persist reports so users can revisit and refine them later.
- Support PDF export for generated reports.
- Keep the system safe when executing AI-generated SQL.

## MVP Scope

The first version should focus on:

- User authentication.
- Natural language report generation.
- SQL generation for AdventureWorks.
- SQL validation and read-only execution.
- Report dashboard rendering.
- Chart and table visualization.
- Report persistence.
- Recent reports.
- Favorite reports.
- PDF export.

## Future Scope

Potential future features:

- Excel export.
- Advanced report sharing.
- Manual chart editing.
- Report folders.
- Scheduled reports.
- Multiple analytical databases.
- Advanced semantic layer over AdventureWorks.
- Query performance optimization.
