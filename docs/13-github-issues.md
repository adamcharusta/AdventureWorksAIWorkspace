# GitHub Issue Drafts

## Purpose

This document acts as a staging area for GitHub issue drafts.

Issues should be reviewed here before being created as real GitHub issues.

Agents must not create real GitHub issues without explicit user approval.

---

## Issue: Create initial project documentation structure

### Context

The project needs a documentation-first foundation before implementation begins.

### Goal

Create the initial `/docs` folder structure and high-level documentation files.

### Scope

- Project overview
- Business requirements
- User flows
- Functional requirements
- Non-functional requirements
- Architecture
- Data model assumptions
- AI SQL workflow
- Reporting and visualization
- Security and authorization
- Exporting
- Backlog
- GitHub issue drafts
- Technical decisions
- Open questions
- Glossary

### Out of Scope

- Production code
- API implementation
- Frontend implementation

### Acceptance Criteria

- [ ] `/docs` folder exists.
- [ ] Initial documentation files are created.
- [ ] README links to documentation.
- [ ] All repository content is written in English.

### Labels

- documentation
- analysis

---

## Issue: Define AI SQL workflow

### Context

The core value of the application depends on transforming natural language prompts into safe SQL queries and useful dashboards.

### Goal

Define the AI SQL workflow before implementation.

### Scope

- Intent extraction
- SQL generation
- SQL validation
- Query execution
- Chart planning
- Report persistence
- Token usage optimization

### Out of Scope

- OpenAI API implementation
- SQL parser implementation
- Query execution code

### Acceptance Criteria

- [ ] AI SQL workflow is documented.
- [ ] SQL safety rules are listed.
- [ ] Open questions are captured.
- [ ] Backlog tasks are created.

### Labels

- ai
- architecture
- security

---

## Issue: Define report persistence model

### Context

Reports should behave similarly to ChatGPT or Claude conversations and must be saved for future use.

### Goal

Define the initial data model for saved reports and report conversations.

### Scope

- Report entity
- Report conversation
- Report messages
- Generated SQL metadata
- Chart definitions
- Tags
- Favorites
- Export history

### Out of Scope

- EF Core implementation
- Database migrations
- API endpoints

### Acceptance Criteria

- [ ] Report-related entities are documented.
- [ ] Required metadata is listed.
- [ ] Open questions are documented.
- [ ] Backlog tasks are updated.

### Labels

- data-model
- reporting

---

## Issue: Define dashboard layout requirements

### Context

The main UI should contain a left report sidebar, center dashboard workspace, and right AI chat sidebar.

### Goal

Document the dashboard layout and interaction requirements.

### Scope

- Left collapsible sidebar
- Center workspace
- Right collapsible sidebar
- Report rendering area
- Chat interaction area
- Report management interactions

### Out of Scope

- React implementation
- MUI component implementation
- Styling details

### Acceptance Criteria

- [ ] Main layout is documented.
- [ ] Sidebar responsibilities are described.
- [ ] Dashboard workspace responsibilities are described.
- [ ] UX open questions are documented.

### Labels

- frontend
- ux
- reporting

---

## Issue: Define PDF export requirements

### Context

Users should be able to export generated reports to PDF.

### Goal

Define what PDF export should include and how it should behave.

### Scope

- PDF content
- PDF layout principles
- Export metadata
- Open questions

### Out of Scope

- PDF generation library selection
- Backend implementation
- Frontend export button implementation

### Acceptance Criteria

- [ ] PDF export content is documented.
- [ ] PDF layout principles are documented.
- [ ] Open questions are captured.
- [ ] Future Excel export assumptions are documented.

### Labels

- export
- reporting
