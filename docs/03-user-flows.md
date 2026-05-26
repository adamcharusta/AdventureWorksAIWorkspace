# User Flows

## Purpose

This document describes high-level user flows for AdventureWorksAIWorkspace.

## Flow 1: Generate a New Report

1. User opens the application.
2. User opens the AI chat sidebar.
3. User enters a natural language business question.
4. System sends the request to the backend.
5. Backend asks AI to interpret the request and generate SQL.
6. System validates the generated SQL.
7. System executes the query against AdventureWorks.
8. System generates chart and dashboard configuration.
9. Frontend renders the report in the center workspace.
10. Report is saved to the user's report history.

## Flow 2: Refine an Existing Report

1. User opens a previously generated report.
2. User asks a follow-up question in the AI chat.
3. System uses the existing report context.
4. AI generates a refined query or visualization plan.
5. System validates and executes the query.
6. Dashboard updates with the refined report.
7. Conversation history is saved.

## Flow 3: Open a Recent Report

1. User opens the left sidebar.
2. User selects a report from recent reports.
3. System loads the saved report.
4. Center workspace displays the report dashboard.
5. Right sidebar displays the report conversation history.

## Flow 4: Mark a Report as Favorite

1. User opens a report.
2. User clicks the favorite action.
3. System marks the report as favorite.
4. Report appears in the favorites section.

## Flow 5: Tag a Report

1. User opens report settings or metadata panel.
2. User adds one or more tags.
3. System saves the tags.
4. User can filter reports by tag later.

## Flow 6: Export Report to PDF

1. User opens a report.
2. User clicks Export to PDF.
3. System generates a PDF containing report title, prompt, summary, charts, and tables.
4. User downloads the PDF.
5. Export metadata may be stored for audit/history.

## Flow 7: View Generated SQL

1. User opens a report.
2. User chooses to view technical details.
3. System displays generated SQL and explanation.
4. User may copy or inspect the SQL.
5. System should make clear that generated SQL is read-only and validated before execution.
