# DG-002 — Domain Model Decision Pack

## Purpose

Reconcile the product strategy with the financial-domain foundation before DG-002 is accepted.

This document is a decision aid. It does not approve an architecture.

## 1. Design pressure discovered

The product scope requires Mizan to distinguish:

- evidence/input;
- interpreted proposal;
- accepted financial operation;
- authoritative financial effects;
- account balance/history.

A simple **Financial Record = income/expense/transfer** model is sufficient for a basic expense tracker but is likely too narrow for the approved strategic direction if the product is to support reimbursements, advances, shared payments, evidence channels, AI proposals, corrections, and future automation.

## 2. Candidate layered domain model

### Evidence
An observed/input artifact with provenance.

Examples:
- manual input;
- text;
- receipt/image;
- voice transcription;
- notification;
- import;
- future integration payload.

**Rule:** evidence is not financial truth.

### Financial Proposal
A candidate interpretation produced from evidence or direct user input.

It may contain:
- proposed operation type;
- proposed amount/currency;
- candidate accounts;
- candidate counterparty/category;
- confidence/ambiguity;
- supporting evidence references.

**Rule:** a proposal has no authoritative financial effect until accepted by domain policy.

### Financial Operation
An accepted business-level financial event.

Candidate examples:
- income;
- personal expense;
- owned-account transfer;
- recoverable/payment-on-behalf;
- advance/shared expense;
- reimbursement;
- correction/reversal;
- informational operation.

### Financial Effect
The explicit authoritative change applied to an account.

Candidate fields:
- operation identity;
- account identity;
- amount;
- currency;
- effect direction/type;
- effective timestamp/order;
- provenance;
- correction/reversal relationship where applicable.

### Account
A user-owned financial container with:
- stable identity;
- display name;
- account classification;
- currency;
- lifecycle state;
- opening/initial financial state.

### Balance
A derived financial state calculated from the account opening state plus authoritative effects applicable to the requested point in time.

### History
An auditable view of accepted operations and their effects.

### Category / Counterparty
Descriptive dimensions that should not themselves determine financial truth.

## 3. Candidate relationship

**Evidence**
→ produces **Financial Proposal**
→ domain validation/confirmation
→ accepted **Financial Operation**
→ produces one or more **Financial Effects**
→ affects **Account**
→ derives **Balance**
→ appears in **History**

This allows AI and future integrations to evolve independently from the authoritative financial core.

## 4. Critical distinction: operation vs effect

A Financial Operation answers:

> What happened financially in business terms?

A Financial Effect answers:

> Exactly what changed in authoritative account state?

This separation supports cases where one operation has multiple effects without turning the system into a generic accounting engine.

## 5. Candidate alternatives

### A — Keep Financial Record as the central concept

**Pros**
- smallest conceptual model;
- simple implementation;
- fast MVP.

**Cons**
- becomes overloaded when evidence, proposals, recoverables, corrections and multi-effect operations are introduced;
- makes the boundary between business meaning and balance mutation less explicit.

### B — Operation + Effect, with Evidence/Proposal around it

**Pros**
- preserves financial correctness;
- supports AI proposals without giving AI authority;
- supports multi-effect operations;
- clearer auditability and future integrations;
- still avoids exposing accounting/ledger complexity to the user.

**Cons**
- more domain concepts;
- more implementation and test work.

### C — Full accounting ledger

**Pros**
- maximum generality and formal financial expressiveness.

**Cons**
- premature for a personal-finance product;
- higher complexity;
- risks making the product accounting-centric instead of user-centric.

## Working candidate

**B — Operation + Effect, with Evidence/Proposal as surrounding concepts.**

This is a design candidate only. It should be accepted or changed at the DG-002 gate.

## 6. MVP boundary

The candidate model does not require implementing every future concept.

MVP can keep:
- Evidence: minimal/manual provenance;
- Proposal: optional/directly accepted input path;
- Operations: income, personal expense, owned-account transfer;
- Effects: authoritative balance changes;
- Account, Balance, History.

Recoverable/shared/advance semantics can be represented only when the approved MVP scope requires them, but the model should not structurally prevent them.

## 7. Invariants impacted

If B is selected, DG-003 must explicitly verify:

1. Only accepted operations can create authoritative effects.
2. Every balance-changing operation has explicit effects.
3. Every effect belongs to exactly one accepted operation.
4. Effects identify account, amount, currency and financial direction.
5. An operation's complete effect set commits atomically.
6. Evidence/proposals cannot directly change balances.
7. Replaying an accepted operation cannot duplicate effects.
8. Balance rebuild uses authoritative effects, not proposals/evidence.

## 8. Decisions still requiring Product Owner approval

- Select A, B, or C.
- Confirm whether recoverable/shared/advance semantics are MVP behavior or future-ready only.
- Confirm whether Evidence and Proposal are persisted domain concepts in MVP or application-layer concepts.
- Confirm whether Financial Operation is the user-facing transaction concept, an internal aggregate, or both.

## Decision record

**State:** Open  
**Owner:** Khaled  
**Approval:** Not granted by this document.
