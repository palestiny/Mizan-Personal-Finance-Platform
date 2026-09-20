# DG-001 — Product Scope Decision Gate

## Status

**State:** Proposed — awaiting Product Owner decision  
**Phase:** M0 — Product & Domain Foundation

This document converts the current product direction into a decision-ready scope proposal. It does not silently approve the scope.

## 1. Product intent

Mizan is intended to be a personal financial system, not merely an expense-entry application.

Its core responsibility is to represent a user's financial state and its history accurately enough that the user can rely on the system when answering questions such as:

- What money do I currently have?
- Where is that money held?
- What money came in?
- What money went out?
- When did a financial change happen?
- What changed the balance of an account?
- Can I trust the historical record?

The system's financial truth must come from authoritative financial records and explicit business rules. AI, UI state, cached values, and inferred insights must not become the source of financial truth.

## 2. Proposed initial target user

### Primary

An individual managing their own personal finances.

### Explicitly unresolved

The following should not be assumed into the initial scope:

- Household/shared finances
- Multiple users collaborating on one financial space
- Organizational/business accounting

These can be revisited after the personal-finance core is validated.

## 3. Proposed core user outcomes

The initial product should enable a user to:

1. Establish one or more financial accounts.
2. Record money received as income.
3. Record money spent as an expense.
4. Move money between owned accounts as a transfer.
5. See the resulting account balances.
6. Review the financial history that produced those balances.
7. Correct mistakes using explicit financial-history rules.
8. Continue using the core financial functions when connectivity is unavailable, if offline-first remains a release requirement.
9. Understand the financial effect of an operation before committing it.

These are proposed product outcomes, not yet implementation requirements.

## 4. Proposed MVP boundary

### In scope

The MVP should concentrate on the smallest complete financial loop:

**Account → Financial Record → Balance → History**

Candidate MVP capabilities:

- Account creation and management
- Income recording
- Expense recording
- Transfer recording
- Balance viewing
- Transaction/history viewing
- Basic categorization if it proves necessary for the core workflows
- Financial record correction according to approved rules
- Local persistence
- Reliable recovery from ordinary application failures
- Security/privacy controls required for the selected release model

### Deliberately not required to close DG-001

The following should remain separate decisions rather than being smuggled into the MVP:

- Advanced budgeting
- Goals
- Forecasting
- AI financial recommendations
- Autonomous financial actions
- Bank connectivity
- Investment portfolio management
- Social/community features
- Complex household collaboration
- Business accounting

A later product decision may promote any of these.

## 5. Product boundary principle

Mizan's MVP should optimize for **financial correctness and trustworthy daily use**, not for the number of features delivered.

A feature belongs in the MVP only if one of these is true:

- It is required to complete the core financial loop.
- It is required to preserve financial correctness.
- It is required to protect user data.
- It is required for reliable operation of an approved MVP workflow.
- Without it, a core user outcome cannot be completed safely.

Otherwise it should remain outside the MVP until evidence justifies inclusion.

## 6. Scope options

### Option A — Expense Tracker First

Focus on expenses, with income and transfers added later.

**Advantages**
- Smaller initial feature set.
- Faster path to a visible UI.

**Costs / risks**
- Establishes the wrong conceptual center if Mizan is intended to represent the user's financial state.
- Makes account balances and transfers secondary concerns.
- Risks requiring later domain restructuring.

### Option B — Complete Personal-Finance Core

Start with accounts, income, expenses, transfers, balances, and history as one coherent financial core.

**Advantages**
- Matches the product direction in the engineering kickoff material.
- Establishes the financial model before reports and intelligence.
- Allows balances to be validated against authoritative history from the beginning.
- Reduces the chance that an expense-only model becomes the accidental domain architecture.

**Costs / risks**
- Larger foundation than an expense-only prototype.
- Requires earlier decisions about financial invariants, corrections, transfers, and balance semantics.

### Option C — Ledger-First General Financial Engine

Build a generalized accounting/ledger engine before defining the user-facing personal-finance product.

**Advantages**
- Potentially powerful and general.
- Could support more financial domains later.

**Costs / risks**
- High risk of premature generalization.
- Increases conceptual and implementation complexity before Mizan's user needs are validated.
- Can optimize for an accounting abstraction rather than the actual personal-finance product.

## 7. Proposed direction for decision

The evidence currently available from the project reference material supports **Option B as the working proposal** because it matches the stated long-term product direction while remaining bounded around the personal financial core.

This is a recommendation for the Product Owner to accept, modify, or reject. It is not yet an approved decision.

## 8. Decisions still required before DG-001 can close

### D1 — Initial user boundary

Choose:

- Individual only
- Individual + household/shared finance

### D2 — MVP financial scope

Choose:

- Expense-only
- Complete personal-finance core
- Other explicitly defined boundary

### D3 — Account scope

Define which account concepts are required initially, for example:

- Cash
- Bank account
- Wallet
- Other user-defined account types

The exact domain representation belongs to DG-002.

### D4 — Currency scope

Choose whether MVP is:

- Single-currency
- Multi-currency

If multi-currency is selected, conversion semantics become a later domain/architecture decision.

### D5 — Offline requirement

Choose whether offline capability is:

- Mandatory for MVP
- Required after MVP
- Not required

This affects DG-006 and downstream architecture.

### D6 — History correction policy

At product level, define whether users can:

- Edit historical records directly
- Reverse/correct records through compensating operations
- Use a mixed policy depending on record state

The detailed financial invariant belongs to DG-003/DG-004.

### D7 — Data portability

Choose whether MVP must provide:

- Export
- Import + export
- Neither initially

## 9. Success criteria for DG-001

DG-001 is closed only when the repository contains an explicit accepted decision for:

- Target user
- Product problem
- Core outcomes
- MVP scope
- Account scope at product level
- Currency scope at product level
- Offline requirement
- History/correction product policy
- Data portability scope
- Explicit non-goals

The decision must also identify unresolved questions that are intentionally deferred to DG-002 or later gates.

## 10. Gate dependency

DG-001 must be sufficiently resolved before finalizing:

**DG-002 Domain Model → DG-003 Financial Invariants → DG-004 Transaction Model → DG-005 Balance Model**

Technical architecture should not be finalized before these product/domain dependencies are understood.

## 11. Decision record

**Decision:** Open  
**Decision owner:** Khaled  
**Date:** Not yet decided  
**Rationale:** Pending Product Owner decision

