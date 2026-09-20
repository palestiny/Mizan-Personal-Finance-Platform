# DG-001 — Product Scope Decision Gate

## Status
**State:** Proposed — product direction expanded; scope decisions still require explicit Product Owner acceptance.
**Phase:** M0 — Product & Domain Foundation

## Product intent
Mizan is intended to become a **Personal Financial Operating System**, not merely an expense-entry application.

Its product progression is:
**Capture → Understand → Explain → Predict → Recommend → Act**

Authoritative financial records and explicit business rules remain the financial source of truth. AI, UI state, caches, and inferred insights must not become financial truth.

## Global product direction
Mizan may start from Egypt and initially be used personally, but this is a validation context, not a product boundary.

The product must not be customized around one country, language, culture, provider, person's habits, input method, or future workflow.

The long-term target is a globally applicable product supporting different countries, languages, currencies, institutions, circumstances, and financial workflows through extensible localization and integration boundaries.

## Commercial ambition
Mizan is intended to become a real service that solves validated customer problems, closes meaningful market gaps, and generates sustainable recurring revenue.

Recurring value should come from reduced financial administration, effortless capture, trusted visibility, explanations, proactive insights, forecasting, scenarios, personalized intelligence, and controlled automation.

Revenue mechanisms must not compromise correctness, privacy, user ownership, portability, auditability, or user control.

See:
- docs/01-Product/MIZAN-PRODUCT-VISION.md
- docs/01-Product/MIZAN-PRODUCT-STRATEGY.md

## Product problem
People need more than a place to record expenses. They need a coherent answer to:

**What is happening with my money, what does it mean, and what should I do next?**

The product should connect financial evidence, trusted financial state, understanding, prediction, and user-controlled action.

## Input and evidence boundary
A financial event may arrive through manual entry, receipt/image evidence, voice, bank notifications/messages, imports, future integrations, or other machine-readable sources.

These are input/evidence channels, not separate financial truths.

Future conceptual flow:
**Evidence/Input → Extraction → Candidate Financial Event → Validation → Confirmation/Policy → Authoritative Financial Record → Intelligence**

## Economic-effect boundary
A payment is not automatically a personal expense.

The product must remain capable of distinguishing:
1. personal financial effect;
2. transfer between owned accounts;
3. payment on behalf of another person;
4. recoverable amount / expected reimbursement;
5. reimbursement received;
6. advance/shared expense;
7. obligation or expected future payment;
8. informational/evidence-only event with no balance effect.

Exact semantics belong to DG-002/DG-004.

## Core product loop
The minimum trustworthy financial loop remains:
**Account → Financial Record → Balance → History**

The future product loop expands around it:
**Capture → Financial Truth → Understanding → Intelligence → Action**

The first loop must be trustworthy before the second can safely become authoritative.

## Proposed MVP boundary
The MVP should establish the smallest complete and trustworthy financial core:
- account creation/management;
- income;
- expense;
- transfers;
- balances;
- history;
- correction rules;
- local persistence/recovery;
- required security/privacy;
- foundations that do not block future evidence and AI capabilities.

Advanced AI, forecasting, automation, bank connectivity, household collaboration, investment management, and professional variants remain later capabilities unless evidence or a specific design gate promotes them.

The architecture must not make those capabilities impossible.

## Product-wide principles
- **Trust before intelligence:** incorrect financial truth cannot be repaired by AI.
- **AI assists; domain validates:** AI may interpret and propose; domain rules validate financial operations.
- **Evidence is not truth:** receipt, voice transcription, notification, or model output becomes financial truth only through approved domain processing.
- **Global core, localized edges:** market/provider/language differences should be isolated in extensible boundaries.
- **User control:** consequential changes and automation require explicit authorization and auditable policy.
- **Data ownership:** privacy, exportability, and user control are foundational.
- **Build for the vision, implement for the evidence:** preserve future capability without building the entire future now.

## Strategic success criteria
Mizan should eventually demonstrate:
- materially lower friction for financial capture;
- accurate representation of complex real-world financial effects;
- explainable important financial changes;
- useful measurable AI assistance;
- recurring value from proactive intelligence;
- user trust in financial state;
- willingness among a meaningful user segment to pay for the service.

These are strategic outcomes to validate, not current implementation acceptance criteria.

## Decisions still required

### D1 — Initial user boundary
- Individual only
- Individual + household/shared

### D2 — MVP financial scope
- Expense-only
- Complete personal-finance core
- Other explicitly defined boundary

### D3 — Account scope
Define initial account concepts without coupling the domain to providers.

### D4 — Currency scope
- Single currency
- Multi-currency

### D5 — Offline requirement
- Mandatory in MVP
- Required after MVP
- Not required

### D6 — Historical correction policy
- Direct edit
- Compensating/reversal
- Mixed policy

### D7 — Data portability
- Export
- Import + export
- Neither initially

### D8 — Product generality
**Working proposal:** global product from inception; Egypt/personal use is validation context only.

### D9 — Financial-effect classification
**Working proposal:** transaction design must distinguish personal effects, transfers, recoverable/reimbursable movements, shared/advance payments, and informational/no-balance events. Exact semantics belong to DG-002/DG-004.

## Gate dependency
DG-001 must be sufficiently resolved before finalizing:
**DG-002 Domain Model → DG-003 Financial Invariants → DG-004 Transaction Model → DG-005 Balance Model**

AI architecture, integrations, and advanced intelligence must be designed against the trusted financial foundation rather than defining that foundation.

## Decision record
**Decision:** Open
**Decision owner:** Khaled
**Rationale:** Product vision and strategic direction have been expanded; explicit product decisions remain to be accepted or modified.
