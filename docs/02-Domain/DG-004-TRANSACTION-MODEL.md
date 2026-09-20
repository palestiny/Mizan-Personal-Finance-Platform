# DG-004 — Transaction Model Decision Gate

## Status
**State:** Accepted — Product Owner approved 2026-09-20
**Phase:** M0 — Product & Domain Foundation

This gate defines candidate transaction semantics without silently approving unresolved product policy.

## 1. Design problem

Mizan must not equate every payment with a personal expense.

The model must represent the difference between:
- an input/evidence item;
- a proposed interpretation;
- an accepted financial operation;
- the authoritative financial effects produced by that operation.

The authoritative financial state must be derived from accepted effects, not from AI interpretation or raw evidence.

## 2. Candidate conceptual layers

### Evidence
What Mizan received or observed: manual entry, text, receipt/image, voice transcription, notification, import, or future integration data.

Evidence is provenance/context, not financial truth.

### Financial Operation
A validated business operation accepted by the domain.

Examples:
- income;
- personal expense;
- transfer between owned accounts;
- payment made on behalf of another person;
- advance;
- reimbursement received;
- correction/reversal.

### Financial Effect
The explicit change applied to one account by an accepted operation.

This separation is a candidate because it lets one real-world event produce multiple effects without making the input itself authoritative.

## 3. Candidate economic-effect classifications

| Classification | Typical meaning | Balance effect |
|---|---|---|
| Personal expense | User bears the cost | Decrease |
| Income | User receives value | Increase |
| Owned-account transfer | Value moves between user's accounts | Source decrease + destination increase |
| Pass-through/payment on behalf | User pays but cost may be recoverable | Decrease now; recoverable relationship separately represented |
| Advance/shared expense | User temporarily bears value for a shared obligation | Decrease now; claim/settlement semantics may follow |
| Reimbursement received | Previously recoverable amount is returned | Increase; relationship to prior claim should be explicit |
| Informational/evidence-only | Relevant information with no accepted financial effect | No balance effect |

These classifications are candidates, not approved business policy.

## 4. Core candidate rules

1. Evidence cannot directly mutate authoritative financial state.
2. AI output cannot directly mutate authoritative financial state.
3. Every accepted balance-changing operation must produce explicit financial effects.
4. A balance-changing effect must identify its account, amount, currency, direction/effect, and operation provenance.
5. A transfer is one business operation with coordinated effects on source and destination.
6. No-balance information must remain representable without inventing a financial effect.
7. Recoverable/advance/shared semantics must not be collapsed into ordinary personal expense merely because money left the account.
8. Accepted effects must be auditable back to the accepted operation and, where available, supporting evidence.
9. Reprocessing the same accepted operation must not create duplicate financial effects.
10. Corrections must preserve explainability according to the product's approved correction policy.

## 5. Candidate transaction lifecycle

**Capture → Interpret → Validate → Confirm/Policy → Accept → Apply Effects → Explain**

Possible AI involvement:
- extraction;
- classification;
- ambiguity detection;
- proposal generation.

Authoritative domain responsibilities:
- validation;
- business-rule enforcement;
- acceptance;
- effect creation;
- balance impact;
- idempotency;
- correction semantics.

## 6. Accepted transaction decisions

1. Operation Command is the transaction boundary.
2. Command is separate from the persisted immutable Financial Operation.
3. Transaction lifecycle is internal; external contracts expose accepted/rejected outcomes rather than requiring clients to manage internal states.
4. Idempotency applies to balance-changing commands that may be retried.
5. Validation precedes effect derivation/validation and atomic commit.
6. Correction/Reversal uses dedicated commands and creates new Operations/Effects.
7. effective_at and recorded_at are retained.
8. Accepted results are explanation-ready, not merely an operation identifier.

## 7. Key unresolved decisions

DG-004 must not finalize until these are explicitly decided:

- whether Financial Operation is a first-class aggregate or an internal domain concept;
- exact classification vocabulary;
- whether recoverables/claims are in MVP or only represented as metadata/relationship;
- whether zero-effect operations are allowed;
- whether one operation may affect more than two accounts;
- fee/adjustment semantics;
- operation ordering/effective date versus recording date;
- idempotency boundary and key ownership;
- correction/reversal representation;
- whether pending/unconfirmed proposals have any financial effect (candidate: no).

## 8. Alternatives for the core representation

### A — Transaction-centric
Transaction is the primary aggregate and directly contains its effects.

**Pros:** intuitive; simpler initial API.
**Cons:** can become overloaded when evidence, claims, shared expenses, corrections, and future complex operations grow.

### B — Operation + Effect
A validated operation owns explicit financial effects.

**Pros:** clean separation between business meaning and balance mutation; supports richer semantics and auditability.
**Cons:** more concepts and implementation work.

### C — Ledger-first
Everything is modeled primarily as postings/entries.

**Pros:** strong accounting rigor and extensibility.
**Cons:** risks over-generalizing the product and exposing accounting complexity to a personal-finance UX.

**Working recommendation for further design:** investigate B as the candidate, while keeping the user-facing concept understandable as a transaction/financial event. No decision is approved by this document.

## 9. Gate dependency

DG-004 depends on:
- DG-001 Product Scope
- DG-002 Domain Model
- DG-003 Financial Invariants

DG-005 Balance Model depends on sufficiently resolved transaction/effect semantics.

## 10. Definition of Done

- Accepted transaction vocabulary is documented.
- Evidence/proposal/authoritative-state boundary is explicit.
- Financial-effect rules are testable.
- Correction, ordering, idempotency, zero-value, and fee semantics are resolved.
- Model supports the approved MVP without blocking the product strategy.
- Every rule has an identified verification path.

## 11. Decision record

**Decision:** Accepted
**Decision owner:** Khaled
**Approval date:** 2026-09-20
**Approval:** Product Owner approved the eight transaction-model recommendations listed above. Remaining future architecture details stay bounded to downstream gates.
