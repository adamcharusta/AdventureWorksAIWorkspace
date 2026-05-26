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
