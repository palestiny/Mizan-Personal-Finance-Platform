# DG-001 — Product Scope Decision Gate

## Status
**State:** Accepted
**Phase:** M0 — Product & Domain Foundation
**Decision owner:** Khaled

## Accepted decisions

| Decision | Accepted option |
|---|---|
| D1 Initial user boundary | **Individual only** |
| D2 MVP financial scope | **Complete personal-finance core** |
| D3 Account scope | **Cash + Bank + Wallet**, with controlled extensibility and no provider-specific core entities |
| D4 Currency | **Single-currency MVP**, while keeping currency explicit in the domain |
| D5 Offline | **Architecture-ready; full offline-first release after MVP** |
| D6 Historical correction | **Mixed policy**, with explicit correction history and immutable financial effects after the applicable correction boundary |
| D7 Data portability | **Export in MVP; import later unless validated as necessary** |
| D8 Product generality | **Global product foundation; Egypt/personal use is validation context only** |
| D9 Financial-effect classification | **Domain supports richer financial semantics; MVP implementation starts with income, personal expense, and owned-account transfer, without structurally blocking recoverables/reimbursements/advance/shared cases** |

## Product intent
Mizan is a **Personal Financial Operating System**, not merely an expense-entry application.

Its long-term progression is:
**Capture → Understand → Explain → Predict → Recommend → Act**

The authoritative financial model remains the source of truth. AI, UI state, caches, and inferred insights must not become financial truth.

## MVP boundary
The MVP establishes the smallest complete and trustworthy financial core:
- account creation/management;
- income;
- expense;
- owned-account transfers;
- balances;
- history;
- correction rules;
- trustworthy persistence/recovery path;
- required security/privacy;
- foundations that preserve future evidence and AI capabilities.

Advanced AI, forecasting, automation, bank connectivity, household collaboration, investment management, and professional variants remain later capabilities unless promoted by evidence and a later gate.

## Global and commercial boundary
Egypt and personal use are the initial validation context, not product boundaries. The core must remain globally applicable through explicit localization and integration boundaries.

Mizan is intended to become a real commercial service solving validated customer problems and generating sustainable recurring value. Monetization must not compromise correctness, privacy, ownership, portability, auditability, or user control.

## Accepted financial pipeline
**Evidence/Input → Extraction/Interpretation → Financial Proposal → Domain Validation → Confirmation/Policy → Accepted Financial Operation → Authoritative Financial Effects → Intelligence**

## Accepted core financial loop
**Account → Accepted Financial Operation → Financial Effects → Balance → History**

## Product principles
- Trust before intelligence.
- AI assists; domain validates.
- Evidence is not truth.
- Global core, localized edges.
- User control over consequential changes and automation.
- Data ownership and portability.
- Build for the vision; implement for validated evidence.

## Consequences
These decisions constrain the next gates:
**DG-002 Domain Model → DG-003 Financial Invariants → DG-004 Transaction Model → DG-005 Balance Model**

They explicitly prevent premature scope expansion into household sharing, multi-currency, full offline synchronization, broad imports, provider-specific financial models, and generalized accounting.

## Decision record
**Decision:** Accepted
**Approval:** Product Owner accepted the above recommendations on 2026-09-20.
