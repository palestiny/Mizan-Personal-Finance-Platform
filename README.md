# Mizan — Personal Finance Platform

## Current status

**Phase:** M0 — Product & Domain Foundation  
**Implementation status:** No production code yet  
**Source of truth:** This GitHub repository

Mizan is being designed as a mobile-first personal financial system. The initial product direction is broader than an expense tracker: it must eventually represent accounts, income, expenses, transfers, balances, and financial history accurately and reliably.

## Engineering approach

We will not start implementation by creating arbitrary folders, models, APIs, or UI screens.

For major areas the workflow is:

**Understand → Map → Design → Trade-offs → Decide → Document → Test → Implement → Review → Verify**

Major decisions require an explicit design gate. Proposed ideas are not treated as approved decisions until the project owner accepts them.

## Quality priorities

The project is intended to achieve professional production quality through:

- Financial correctness and explicit invariants
- Security and privacy
- Reliability and recoverability
- Offline usability where appropriate
- Deterministic financial state
- Idempotent operations and safe retries
- Testability and automated verification
- Observability and operational readiness
- Accessibility and maintainability
- Data portability and auditability

“Large-company quality” does **not** mean introducing unnecessary microservices or complexity before the domain requires them.

## M0 design gates

The initial design-gate map is:

- DG-001 — Product Scope
- DG-002 — Financial Domain Model
- DG-003 — Financial Invariants
- DG-004 — Transaction Model
- DG-005 — Balance Model
- DG-006 — Offline-First Strategy
- DG-007 — Persistence Architecture
- DG-008 — Synchronization
- DG-009 — Mobile Technology
- DG-010 — Security & Privacy
- DG-011 — Backend Architecture
- DG-012 — Production & Observability

The order may change if evidence shows a dependency requires it.

## Repository rule

GitHub is the source of truth. Important project decisions, checkpoints, risks, and architecture decisions must be documented here.

See `docs/00-Governance/` for the current project state and engineering controls.
