# Mizan — Personal Finance Platform

## Current status

**Phase:** M0 — Product & Domain Foundation  
**Implementation status:** No production code yet  
**Source of truth:** This GitHub repository

## Product direction

Mizan is being designed as a **Personal Financial Operating System**, not merely an expense tracker.

The long-term vision is to help people **capture → understand → explain → predict → decide → act** on their financial lives with dramatically less effort, while maintaining trustworthy financial truth.

The product may start in Egypt and initially be used personally, but it is **global by design**: it must not be customized around one country, language, provider, person, or workflow.

Mizan is intended to become a real commercial service that solves validated customer problems and creates recurring value strong enough to justify subscription payment.

### Core strategic pillars

- Frictionless financial capture from manual, receipt/image, voice, notifications, imports, and future integrations
- Explicit representation of the economic meaning of financial events, including reimbursements, advances, shared expenses, and payments on behalf of others
- Explainable and recoverable financial truth
- AI-native financial intelligence over trusted structured data
- Proactive insights, forecasting, scenario analysis, recommendations, and controlled automation
- Global extensibility and localization without forking the financial core
- Privacy, user ownership, portability, and user control

### AI boundary

AI is a replaceable intelligence layer, not the financial source of truth.

Conceptual flow:

**Evidence/Input → AI Interpretation → Structured Proposal → Domain Validation → Confirmation/Policy → Authoritative Financial State → AI Analysis**

See:
- `docs/01-Product/MIZAN-PRODUCT-VISION.md`
- `docs/01-Product/MIZAN-PRODUCT-STRATEGY.md`

## Engineering approach

We will not start implementation by creating arbitrary folders, models, APIs, or UI screens.

For major areas the workflow is:

**Understand → Map → Design → Trade-offs → Decide → Document → Test → Implement → Review → Verify**

Major decisions require an explicit design gate. Strategic hypotheses are documented separately from approved domain/business decisions.

## Quality priorities

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
- Explainable AI and controlled automation

“Large-company quality” does **not** mean introducing unnecessary microservices or complexity before the domain requires them.

## M0 design gates

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

GitHub is the source of truth. Important project decisions, checkpoints, risks, product hypotheses, and architecture decisions must be documented here.

See `docs/00-Governance/` for project state and engineering controls.
