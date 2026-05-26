# Exporting PDF and Excel

## Purpose

This document describes export requirements for generated reports.

## PDF Export

PDF export is expected to be part of the MVP.

A PDF report should contain:

- Report title.
- Original user prompt.
- AI-generated summary.
- Charts.
- Tables where relevant.
- Report generation date.
- Optional generated SQL section.

## PDF Layout Principles

- The PDF should be readable as a standalone business report.
- The most important insights should appear near the beginning.
- Charts should be clear and not overly compressed.
- Large tables may need pagination or truncation.

## PDF Export Questions

- Should generated SQL be included by default?
- Should users choose which charts appear in the PDF?
- Should the PDF include report metadata and tags?
- Should export files be stored or generated on demand?

## Excel Export

Excel export is a future feature and may not be part of the MVP.

Possible Excel export content:

- Raw query result data.
- Summary sheet.
- Separate sheets for each chart dataset.
- Report metadata.

## Excel Export Questions

- Should Excel include charts or only data tables?
- Should Excel export use raw result data or transformed chart data?
- Should Excel export be available for all reports or only table-based reports?
