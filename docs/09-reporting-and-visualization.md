# Reporting and Visualization

## Purpose

This document describes report and visualization requirements for AdventureWorksAIWorkspace.

## Report Composition

A generated report should contain:

- Report title.
- Original user prompt.
- AI-generated business summary.
- One or more charts.
- Optional tables.
- Optional KPI cards.
- Generated SQL explanation.
- Report metadata.

The MVP should persist the latest rendered report snapshot, including the tabular result returned from AdventureWorks and the AI-selected chart specifications. This allows the center workspace to render a saved report with charts, table data, and AI-written insights when the user reopens it from the sidebar.

## Visualization Types

## KPI Card

Used for single important values.

Examples:

- Total revenue.
- Total orders.
- Average order value.
- Number of customers.

## Bar Chart

Used for comparing categories.

Examples:

- Sales by product category.
- Revenue by region.
- Orders by salesperson.

## Line Chart

Used for trends over time.

Examples:

- Monthly sales trend.
- Quarterly revenue trend.
- Year-over-year comparison.

## Pie Chart

Used for showing proportions.

Examples:

- Sales share by category.
- Region contribution percentage.

Pie charts should be avoided when there are too many categories.

## Table

Used for detailed data.

Examples:

- Top products by revenue.
- Customer order details.
- Regional sales breakdown.

## Dashboard Layout Principles

- Important KPIs should appear near the top.
- Trend charts should be visually prominent when time is relevant.
- Tables should support detailed inspection.
- The dashboard should not be overloaded with too many charts.
- AI should choose charts based on business intent and result shape.

## Report Metadata

Reports should support:

- Title, used as the editable report name shown in the report sidebar.
- Owner user.
- Status.
- Tags.
- Favorite status.
- Created date.
- Updated date.
- Related conversation history.
- Generated SQL history.

## Conversation History

Each report should preserve the conversation that created and refined it.

The report conversation should include:

- User messages.
- Assistant messages.
- Message timestamps.
- Message ordering.
- Links from assistant responses to generated SQL artifacts when the response produced or reused SQL.

The frontend chat panel should be able to reload the conversation when a saved report is opened.

## Generated SQL History

Each report should preserve generated SQL attempts separately from chat text.

Stored SQL metadata should include:

- SQL text.
- Source user prompt or source message.
- Validation status and rejection reason when applicable.
- Execution status and error message when applicable.
- Token usage when reported by the AI provider.
- Result metadata such as row count and column count.

The full rendered report snapshot should be stored separately from generated SQL metadata. SQL metadata remains useful for audit/debugging, while the report snapshot powers the dashboard UI.

## Open Questions

- Should users be able to manually edit chart types?
- Should users be able to rearrange dashboard widgets?
- Should chart definitions be stored as JSON?
