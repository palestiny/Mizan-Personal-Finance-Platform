# DG-002 — Domain Model Decision Gate

## Status

**State:** Draft — decision-ready candidate; depends on DG-001 product-scope decisions  
**Phase:** M0 — Product & Domain Foundation

This gate defines the smallest domain model that can support the new Mizan vision without turning the product into a general accounting system.

## 1. Design objective

Mizan must separate:

Evidence/Input → Proposal/Interpretation → Accepted Financial Operation → Authoritative Financial Effects → Account/Balance/History

The model must preserve financial correctness while allowing future capture channels, AI, integrations, richer economic semantics, and controlled automation to evolve around the financial core.

## 2. Candidate domain concepts

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

**Rule:** evidence is not financial truth and cannot directly mutate authoritative financial state.

### Financial Proposal

A candidate interpretation produced from evidence or direct user input.

It may contain:
- proposed operation type;
- amount/currency;
- candidate accounts;
- category/counterparty;
- confidence/ambiguity;
- supporting evidence references.

**Rule:** a proposal has no authoritative financial effect until accepted by domain policy.

The proposal may remain an application-layer concept in the MVP; that is still an open decision.

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

An operation answers: What happened financially in business terms?

### Financial Effect

The explicit authoritative change applied to an account by an accepted operation.

Candidate fields:
- operation identity;
- account identity;
- amount;
- currency;
- effect direction/type;
- effective time/order;
- provenance;
- correction/reversal relationship where applicable.

An effect answers: Exactly what changed in authoritative account state?

The model must allow one accepted operation to produce multiple effects without becoming a generic double-entry accounting engine.

### Account

A user-owned financial container with:
- stable identity;
- user-visible name;
- account classification;
- currency;
- lifecycle state;
- opening/initial financial state.

Account classification must not become provider-specific financial behavior.

### Balance

The financial state of an account at a defined point in time.

Candidate rule:

Balance = opening/initial state + applicable authoritative financial effects

A materialized balance may exist for performance, but it remains derived/rebuildable state.

### History

An auditable view of accepted operations and their authoritative effects, ordered according to approved temporal semantics.

History must explain how an account reached a balance.

### Category / Counterparty

Descriptive dimensions. They may improve understanding and intelligence but must not themselves determine financial truth.

## 3. Candidate relationships

Evidence
→ Financial Proposal
→ domain validation/confirmation
→ Financial Operation
→ one or more Financial Effects
→ Account
→ Balance
→ History

A direct user entry may bypass a persisted proposal and create an accepted operation after domain validation.

## 4. Candidate MVP boundary

The domain model should support the approved MVP without implementing the entire future vision.

Candidate MVP concepts:
- Account
- Financial Operation
- Financial Effect
- Balance
- History
- income
- personal expense
- owned-account transfer

Evidence may be represented minimally as provenance. Proposal may remain an application-layer concept if that reduces MVP complexity without weakening the AI/domain boundary.

Recoverables, advances, shared expenses, obligations, and informational events should remain structurally possible but are not automatically MVP features.

## 5. What this model deliberately does NOT introduce

The model is not a general accounting engine.

Do not introduce yet:
- full double-entry accounting;
- investment portfolio accounting;
- bank-provider domain objects;
- autonomous financial-agent state;
- household permissions;
- budgets/goals;
- forecasting/scenario engines;
- AI as a domain authority.

Those capabilities can receive their own design gates when validated.

## 6. Candidate invariants to validate in DG-003

DG-003 must verify at minimum:

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

## 7. Open decisions

DG-002 still requires Product Owner approval for:

- A simple Financial Record model vs Operation + Effect vs full ledger;
- whether Evidence/Proposal are persisted domain concepts in MVP or application-layer concepts;
- whether Financial Operation is user-facing, internal, or both;
- which richer economic classifications are MVP behavior;
- account scope;
- currency scope;
- correction semantics.

## 8. Decision record

**Decision:** Open  
**Decision owner:** Khaled  
**Approval:** Not granted by this document.

The working candidate is Operation + Effect, with Evidence/Proposal surrounding the authoritative financial core.
