# Product Discovery — Mizan

## Status

**Discovery document — not a final product specification**

This document captures the current product direction and questions that must be resolved through DG-001. It deliberately avoids turning unvalidated assumptions into requirements.

## Product direction

Mizan is intended to be a mobile-first personal financial system that helps an individual maintain an accurate, understandable record of their financial life.

The initial direction is broader than an expense tracker. The system may need to represent:

- Accounts
- Income
- Expenses
- Transfers
- Balances
- Financial transaction history

The authoritative financial state must come from explicit financial records and defined business rules, not from AI-generated conclusions, UI state, caches, or inferred values.

## Problem to investigate

People need a reliable way to understand what money they have, where it came from, where it went, and how their financial state changed over time.

The product must make correctness understandable to the user while remaining practical for frequent mobile use.

The exact problem statement, target segment, and measurable product outcomes are still open for DG-001.

## Candidate primary user

**Individual personal-finance user**

This is the current working hypothesis. Household/shared-finance scenarios, multiple users, and organizational use are not yet part of the approved initial scope.

## Candidate core jobs

The following are discovery candidates, not final requirements:

1. Record money received.
2. Record money spent.
3. Move money between owned accounts.
4. See current balances.
5. Review financial history.
6. Correct an incorrectly recorded financial event without silently corrupting history.
7. Use the system reliably when connectivity is unavailable, if offline-first is approved.
8. Understand the effect of a financial action before it is committed.

## Candidate product qualities

The product should be evaluated against:

- Financial correctness
- Reliability
- Privacy
- Security
- Recoverability
- Offline usability
- Performance
- Accessibility
- Maintainability
- Observability
- Auditability
- Data portability
- Testability

The final priority and measurable targets remain open.

## Candidate scope questions for DG-001

### Users and ownership

- Is Mizan strictly single-user initially?
- Is multi-device use required for the MVP?
- Are shared accounts/households in or out of the initial product boundary?

### Financial scope

- What account types are required initially?
- Is multi-currency required for MVP?
- Are credit cards, loans, liabilities, or negative balances required?
- Are recurring transactions required?
- Are budgets/goals part of the initial product or later?

### History and correction

- Can a posted transaction be edited directly?
- When should a correction create a new compensating record instead?
- What does “delete” mean for financial history?
- What history must remain auditable?

### Data portability

- Is export required for MVP?
- Which formats are acceptable?
- Is import part of the initial scope?

### Connectivity

- Is offline operation mandatory for the first release or a later milestone?
- What must remain available offline?
- What happens when two devices change the same financial data?

### Security and privacy

- What authentication model is required?
- What data must be encrypted locally?
- What recovery path exists when the user loses access?
- Which privacy/legal requirements are applicable to the target release?

## Current non-goals for the foundation

The following should not drive the initial architecture unless a later product decision requires them:

- AI as a source of financial truth
- Advanced forecasting
- Autonomous financial decisions
- Social/community features
- Premature bank integrations
- Microservice decomposition without a demonstrated need

These are scope-control statements, not permanent product prohibitions.

## DG-001 acceptance criteria

DG-001 should not close until the repository contains an approved product decision covering:

- Problem statement
- Target user
- Core jobs/use cases
- MVP scope
- Explicit non-goals
- Success/acceptance outcomes
- Major product constraints
- Known open questions and their owners/status
