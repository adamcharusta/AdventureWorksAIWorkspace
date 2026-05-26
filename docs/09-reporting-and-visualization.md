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

- Title.
- Tags.
- Favorite status.
- Created date.
- Updated date.
- Related conversation history.

## Open Questions

- Should users be able to manually edit chart types?
- Should users be able to rearrange dashboard widgets?
- Should chart definitions be stored as JSON?
- Should reports store result snapshots or regenerate results from SQL?
