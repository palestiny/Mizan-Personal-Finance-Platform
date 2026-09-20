# DG-001 — Product Scope Decision Pack

## Purpose

Make the remaining Product Owner decisions explicit and comparable so DG-001 can be accepted without reopening the domain design later.

This document is a decision aid. It does not approve any option.

## Decision D1 — Initial user boundary

### A — Individual only
The first release models one person managing their own finances.

Impacts:
- simpler ownership model
- simpler authorization
- no shared ownership/concurrency rules in MVP
- household collaboration deferred

Trade-off:
- future household support may require extending ownership/permissions

### B — Individual + household/shared
The first release supports multiple people sharing financial data.

Impacts:
- ownership, membership, roles, permissions, invitations, concurrency, and audit requirements enter the foundation

Trade-off:
- materially larger domain and security surface

**Working candidate:** A, unless household/shared use is a launch requirement.

## Decision D2 — MVP financial scope

### A — Expense-only
Smaller visible feature set.

Risk:
- creates an expense-centric domain that may later need restructuring.

### B — Complete personal-finance core
Accounts + income + expense + transfer + balance + history.

Benefit:
- establishes the financial truth model around actual financial state.

Cost:
- requires the foundation decisions currently being designed.

### C — General accounting/ledger engine
Build a generalized financial engine before the personal-finance workflows.

Risk:
- premature generalization and unnecessary complexity.

**Working candidate:** B.

## Decision D3 — Account scope

Recommended initial account types:

1. Cash
2. Bank account
3. Wallet / e-wallet
4. Other user-defined account type only if it has the same basic balance semantics

Important boundary:
- Account type is classification.
- Financial behavior comes from explicit account/transaction rules.
- We should avoid creating a separate domain model for every provider or institution.

**Working candidate:** Cash + Bank + Wallet, with extensibility through a controlled type mechanism rather than provider-specific entities.

## Decision D4 — Currency scope

### A — Single currency
All MVP accounts use one configured base currency.

Benefits:
- removes exchange-rate ambiguity
- simplifies transfer semantics
- simpler balances and reporting
- smaller test matrix

Cost:
- users cannot correctly represent accounts in different currencies during MVP.

### B — Multi-currency
Each account can have its own currency.

Required additional semantics:
- exchange-rate source
- rate timestamp/effective time
- rounding/precision
- cross-currency transfer representation
- valuation/reporting currency
- historical rate behavior

**Working candidate:** A for MVP unless multi-currency is a hard launch requirement.

## Decision D5 — Offline requirement

### A — Mandatory in MVP
Core financial operations work without connectivity.

Impacts:
- local durable storage
- local authoritative writes
- sync model
- conflict semantics
- recovery strategy

### B — After MVP
Core domain remains offline-capable by design, but synchronization/release support is deferred.

### C — Not required
Connectivity is assumed for the initial product.

**Working candidate:** B as an architecture strategy unless the product requirement explicitly demands full offline-first release behavior.

Important:
Choosing B does not mean designing a server-only domain. The financial core should remain independent of connectivity.

## Decision D6 — Historical correction policy

### A — Direct edit
User can edit an accepted record.

Benefit:
- simplest UX.

Risk:
- historical truth can silently change unless a complete audit trail/versioning policy is added.

### B — Compensating/reversal operation
Accepted records remain immutable; corrections create explicit financial effects.

Benefit:
- strongest historical explainability.

Cost:
- more complex UX and history representation.

### C — Mixed
Use controlled direct edits for records that are still within an allowed correction state, and compensating operations after that boundary.

Benefit:
- balances usability and auditability.

Cost:
- requires precise state/time rules.

**Working candidate:** C, with immutable financial effects after acceptance boundaries and explicit correction history.

This decision has direct impact on the Transaction Model and database design.

## Decision D7 — Data portability

### A — Export
User can export their financial data.

### B — Import + Export
User can move data into and out of Mizan.

Benefit:
- stronger ownership and migration capability.

Cost:
- import validation, duplicate handling, versioning, and failure recovery.

### C — Neither initially
Fastest initial implementation.

Risk:
- increases lock-in and makes recovery/migration harder.

**Working candidate:** A for MVP, with import designed as a later capability unless there is a concrete migration requirement.


## Product-wide extensibility principles

The product is being started from a personal use case in Egypt, but the architecture and domain are **not to be customized around Khaled, Egypt, Arabic, or one financial workflow**. The initial use case is a validation and dogfooding context only.

The target product direction is globally applicable: different countries, languages, currencies, account providers, user circumstances, and future market needs should be supported through explicit extensibility and localization boundaries rather than core-domain rewrites.

The product should be capable of becoming a real commercial service that solves validated customer problems and can generate sustainable revenue. Business/revenue capabilities must remain separate from financial truth and must not weaken correctness, privacy, auditability, or user control.

### Financial-event input boundary

Manual entry is only one possible input. The product must be able to evolve toward receipt/image capture, voice capture, bank notifications/messages, integrations, and other evidence channels. These channels must normalize into the same authoritative domain rules; an image, voice transcription, or bank message is evidence/input, not financial truth by itself.

### Net-financial-effect boundary

Not every payment that a user makes is necessarily a personal expense. The domain must remain capable of distinguishing personal financial effects from pass-through payments, reimbursements/receivables, advances/shared expenses, and informational records with no balance effect. Exact transaction semantics are deferred to DG-004 rather than hard-coded here.

## Cross-decision consistency checks

Before accepting DG-001, the chosen options must satisfy these constraints:

1. Correction policy must preserve the authoritative financial history invariant.
2. Currency choice must be consistent with transfer and balance semantics.
3. Offline choice must not redefine the financial source of truth.
4. Account scope must not introduce provider-specific financial behavior into the core domain.
5. MVP scope must remain bounded to the approved product outcomes.
6. Data portability must not bypass domain validation.
7. User boundary must match the authorization and ownership model.

## Current working proposal

Unless the Product Owner changes them, the design work can use these as explicit **provisional assumptions**, not approved decisions:

| Decision | Working proposal |
|---|---|
| D1 User | Individual only |
| D2 MVP | Complete personal-finance core |
| D3 Accounts | Cash + Bank + Wallet |
| D4 Currency | Single currency |
| D5 Offline | Architecture-ready; full offline-first release after MVP |
| D6 Correction | Mixed policy with explicit correction history |
| D7 Portability | Export |

## What changes if these proposals are accepted

The next gates can become concrete:

**DG-001 accepted**
→ finalize domain concepts  
→ finalize invariants  
→ define transaction semantics  
→ define balance semantics  
→ then implement the first domain slice with tests.

## Decision record

**State:** Open  
**Owner:** Khaled  
**Approval:** Not granted by this document.
