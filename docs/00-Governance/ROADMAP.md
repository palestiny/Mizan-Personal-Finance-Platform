# Mizan Roadmap

This roadmap is a dependency map, not a promise to build every future capability. It is intentionally sized for a small product team and should expand only when evidence justifies it.

## Product evolution

Mizan's strategic progression is:

Trust → Frictionless Capture → Understanding → Intelligence → Action → Expansion

The engineering roadmap supports that progression without implementing the whole future product upfront.

## M0 — Product & Domain Foundation

Goal: define and approve the smallest trustworthy financial core.

Outputs:
- Product scope and initial problem/wedge
- Product strategy and validation hypotheses
- Domain vocabulary
- Operation/effect model
- Financial invariants
- Transaction semantics
- Balance semantics
- Explicit non-goals and future boundaries

Primary gates:
- DG-001 Product Scope
- DG-013 Product Strategy
- DG-002 Domain Model
- DG-003 Financial Invariants
- DG-004 Transaction Model
- DG-005 Balance Model

Exit condition: the core financial model is approved, testable, and small enough to implement.

## M1 — Thin Technical Foundation

Goal: prove the approved financial core end-to-end with the smallest practical architecture.

Work:
- repository/application structure;
- test infrastructure and CI;
- minimal domain/application layer;
- one persistence path;
- one client/API path as needed;
- basic security boundary;
- a thin vertical slice for account + income/expense + transfer + balance/history.

Do not introduce distributed services, event infrastructure, or elaborate synchronization unless a concrete requirement justifies them.

Architecture gates DG-006–DG-012 are opened here only when the vertical slice exposes a real decision that materially constrains the next implementation step.

## M2 — Trustworthy Financial Core

Goal: turn the thin slice into a reliable usable core.

Capabilities:
- account lifecycle;
- monetary value rules;
- income/expense/transfer;
- balance derivation;
- history;
- correction/reversal;
- idempotency and atomicity;
- recovery/rebuild verification;
- export if approved.

Exit condition: core financial state is correct, explainable, recoverable, and tested.

## M3 — Frictionless Capture

Goal: reduce the effort required to maintain financial truth.

Start with the lowest-cost validated capture improvements, then add channels based on evidence:
- natural-language entry;
- receipt/image extraction;
- voice;
- notification/import ingestion;
- evidence/proposal workflow.

AI remains proposal/interpretation only; domain validation remains authoritative.

## M4 — Understanding

Goal: turn financial records into understandable daily value.

Potential capabilities:
- categories/merchants/counterparties;
- recurring patterns;
- cash-flow views;
- commitments and recoverables where validated;
- timeline and Financial Inbox;
- explain-my-money experiences.

Only capabilities that demonstrate user value should move forward.

## M5 — Intelligence

Goal: use trusted history for proactive value.

Potential capabilities:
- anomaly detection;
- forecasting assistance;
- affordability analysis;
- scenarios;
- personalized explanations;
- recommendations.

Intelligence must remain grounded in authoritative data and explicit assumptions.

## M6 — Action

Goal: let users move from insight to approved action.

Potential capabilities:
- reminders;
- rules;
- plans;
- user-approved workflows;
- controlled automation.

Consequential actions require explicit authorization, policy, and auditability.

## M7 — Expansion

Only after the core product demonstrates recurring value:
- household/shared finance;
- multi-currency;
- bank integrations;
- international/localized capabilities;
- freelancer/creator/professional variants;
- partner ecosystem.

Each expansion gets its own scope/design gate.

## Right-sizing rules

1. One product, one financial core. Avoid microservices until scale or organizational boundaries require them.
2. Build one vertical slice before broad platform architecture.
3. Introduce AI where it reduces friction or creates validated value, not because the roadmap says AI.
4. Future-ready means preserving extension points, not implementing future features.
5. Architecture gates are opened just in time.
6. Every major new capability gets a design gate and measurable exit criteria.
7. Market evidence can change sequencing.
