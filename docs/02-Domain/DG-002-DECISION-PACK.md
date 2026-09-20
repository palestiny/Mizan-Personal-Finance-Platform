# DG-002 — Domain Model Decision Pack

## Decision
**State:** Accepted  
**Owner:** Khaled  
**Accepted:** 2026-09-20

The selected model is **Operation + Effect**, with Evidence and Proposal surrounding the authoritative financial core.

## Accepted rationale
A simple Financial Record model is too narrow for Mizan's approved direction, while a full accounting ledger is premature. Operation + Effect preserves the boundary between business meaning and authoritative account mutation, supports AI proposals and multi-effect operations, and keeps accounting complexity out of the MVP.

## Accepted layered model
Evidence → Financial Proposal → Financial Operation → Financial Effects → Account → Balance/History

- Evidence is not financial truth.
- Proposal is not financial truth.
- Only accepted Operations can create authoritative Effects.
- Effects are immutable after acceptance.
- Corrections/Reversals are represented by new Operations and Effects.
- AI and future integrations operate around the authoritative financial core.

## Accepted MVP
- Account
- Financial Operation
- Financial Effect
- Balance
- History
- Income
- Personal Expense
- Owned-Account Transfer
- Cash / Bank / Wallet account classifications
- Explicit currency, single-currency MVP

Evidence remains lightweight. Proposal exists as an architectural boundary but does not require a full persisted lifecycle in the initial MVP path.

## Explicitly future-ready, not MVP behavior
- Recoverables/payment-on-behalf
- Advances/shared expenses
- Reimbursements
- Informational financial events
- Multi-currency
- Full accounting ledger
- Household/shared foundation
- Advanced automation

## Consequences
### Benefits
- Strong separation between interpretation and financial truth.
- AI can improve capture and understanding without becoming the source of truth.
- Multi-effect operations such as owned-account transfers are explicit.
- Immutable effects and correction operations preserve explainability.
- Future integrations and richer economic semantics have clear extension points.
- The model remains smaller than a general ledger/accounting engine.

### Costs
- More concepts and implementation/test work than a record-centric model.
- Domain/application boundaries must be kept explicit.
- Idempotency, atomicity, correction, and rebuild behavior require dedicated invariants and tests.

## Decision record
Product Owner explicitly accepted the recommendation bundle on 2026-09-20. This document is authoritative for the DG-002 decision pack.
