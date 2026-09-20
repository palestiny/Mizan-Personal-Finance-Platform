# DG-002 — Domain Model Decision Gate

## Status
**State:** Accepted  
**Phase:** M0 — Product & Domain Foundation  
**Decision owner:** Khaled  
**Accepted:** 2026-09-20

## Design objective
Mizan separates:
Evidence/Input → Proposal/Interpretation → Accepted Financial Operation → Authoritative Financial Effects → Account/Balance/History

The model preserves financial correctness while allowing future capture channels, AI, integrations, richer economic semantics, and controlled automation to evolve around the financial core.

## Accepted domain concepts

### Evidence
An observed/input artifact with provenance. Evidence is a domain concept, but MVP representation remains lightweight. Evidence is not financial truth and cannot directly mutate authoritative financial state.

### Financial Proposal
A candidate interpretation produced from evidence or direct user input. It may contain proposed operation type, amount/currency, candidate accounts, category/counterparty, confidence/ambiguity, and supporting evidence references. A proposal has no authoritative financial effect until accepted by domain policy. Proposal is an architectural concept from the beginning, but full persisted proposal lifecycle is not required for the initial MVP path.

### Financial Operation
An accepted business-level financial event. MVP types are income, personal expense, and owned-account transfer. Future-ready semantics may later include recoverable/payment-on-behalf, advance/shared expense, reimbursement, informational operations, correction, and reversal.

An operation answers: What happened financially in business terms?

For the UX, an Operation may be presented as a transaction, income, expense, or transfer. The domain authority remains Financial Operation.

Accepted operations are immutable. Corrections and reversals are represented by new operations.

### Financial Effect
The explicit authoritative change applied to an account by an accepted operation. Effects contain, at minimum, operation identity, account identity, amount, currency, financial direction/type, effective time/order, provenance, and correction/reversal relationship where applicable.

An effect answers: Exactly what changed in authoritative account state?

One operation may produce one or more effects. The complete effect set is validated and committed atomically. Effects are immutable after acceptance.

### Account
A generic user-owned financial container with stable identity, user-visible name, account classification, currency, lifecycle state, and opening/initial financial state.

MVP classifications: Cash, Bank, Wallet. Provider-specific financial entities are not part of the core domain.

### Currency
Currency is explicit on financial concepts. MVP supports one currency per user context; multi-currency is future scope and requires its own design gate.

### Balance
Balance is the financial state of an account at a defined point in time.

Balance = opening/initial state + applicable authoritative financial effects

A materialized balance may exist for performance, but it remains derived/rebuildable state.

### History
An auditable view of accepted operations and their authoritative effects, ordered according to approved temporal semantics. History must explain how an account reached a balance.

### Category / Counterparty
Descriptive dimensions. They may improve understanding and intelligence but must not themselves determine financial truth.

## Accepted relationships
Evidence → Financial Proposal → domain validation/confirmation → Financial Operation → one or more Financial Effects → Account → Balance → History

A direct user entry may bypass a persisted proposal and create an accepted operation after domain validation.

AI and evidence never directly mutate authoritative financial state.

## Accepted MVP boundary
- Account
- Financial Operation
- Financial Effect
- Balance
- History
- Income
- Personal Expense
- Owned-Account Transfer
- Cash / Bank / Wallet classifications
- Explicit currency, single-currency MVP

Evidence is represented minimally as provenance where needed. Proposal is available as an architectural boundary but does not require full persistence/lifecycle in the first MVP path.

Recoverables, advances, shared expenses, obligations, reimbursement semantics, and informational events remain future-ready but are not MVP behavior.

## Explicit non-goals
The model is not a general accounting engine. Do not introduce yet:
- full double-entry accounting;
- investment portfolio accounting;
- bank-provider domain objects;
- autonomous financial-agent state;
- household permissions;
- budgets/goals;
- forecasting/scenario engines;
- AI as a domain authority.

Those capabilities require their own design gates when validated.

## Mandatory implications for DG-003
1. Only accepted operations can create authoritative effects.
2. Every balance-changing operation has a complete explicit effect set.
3. Every effect belongs to exactly one accepted operation.
4. Every effect identifies account, amount, currency, direction/type, and provenance.
5. An operation's complete effect set commits atomically.
6. Evidence/proposals cannot directly change balances.
7. Replaying an accepted operation cannot duplicate effects.
8. No orphan or cross-operation effects are possible.
9. Balance rebuild uses authoritative effects, not proposals/evidence.
10. Corrections preserve historical explainability.
11. Accepted effects are immutable.
12. Correction/reversal creates new operations/effects rather than mutating accepted history.

## Decision record
**Decision:** Accepted  
**Approval:** Product Owner accepted the recommended DG-002 decision bundle on 2026-09-20.

### Accepted decisions
1. **B — Operation + Effect** is the authoritative financial model.
2. Evidence is a domain concept, represented lightweight in MVP.
3. Proposal is an architectural concept; full persisted proposal lifecycle is not required for the initial MVP path.
4. Financial Operation is the authoritative business event and may be presented to users as a transaction/income/expense/transfer.
5. Accepted Operations are immutable.
6. Accepted Effects are immutable.
7. Corrections/Reversals are new Operations with new Effects.
8. MVP operation types are Income, PersonalExpense, and OwnedAccountTransfer.
9. Account is generic with Cash, Bank, and Wallet classifications in MVP.
10. Currency is explicit; MVP remains single-currency.
11. An Operation produces one or more Effects.
12. An Operation's complete Effect set commits atomically.
13. Financial acceptance is idempotent.
14. Evidence/AI cannot directly mutate authoritative financial state.
15. A full accounting Ledger is not introduced in M0/MVP.
16. Recoverable/Advance/Shared/Reimbursement and similar richer semantics are future-ready, not MVP behavior.
