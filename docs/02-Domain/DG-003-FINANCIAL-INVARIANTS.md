# DG-003 — Financial Invariants Decision Gate

## Status

**State:** Accepted — Product Owner approved 2026-09-20  
**Phase:** M0 — Product & Domain Foundation

This gate converts the candidate Operation + Effect domain model into explicit, testable financial invariants. It does not approve unresolved product policy.

## 1. Purpose

Mizan's most important technical property is that authoritative financial state remains explainable, deterministic, recoverable, and correct.

The invariants apply regardless of UI, API, storage engine, cache, AI component, or synchronization mechanism.

## 2. Source-of-truth invariants

### I-001 — Authoritative financial effects

There is one authoritative representation of accepted financial effects.

Derived values such as balances, totals, reports, charts, search indexes, and AI insights must never become independent financial truth.

### I-002 — Proposal/evidence isolation

Evidence, AI output, and unaccepted proposals cannot directly mutate authoritative financial state.

Only an accepted financial operation may produce authoritative effects.

## 3. Monetary representation

### I-003 — No floating-point money

Authoritative monetary calculations must not use binary floating-point representation.

### I-004 — Valid monetary amount

Every accepted effect must satisfy explicit amount and precision rules.

At minimum:
- amount is present;
- amount is finite;
- amount is not NaN;
- amount conforms to approved precision;
- zero-value behavior is explicitly allowed or rejected.

## 4. Operation/effect invariants

### I-005 — Complete explicit effect set

Every accepted balance-changing operation has an explicit, complete set of financial effects.

No balance mutation may be inferred from UI labels, categories, descriptions, or AI output.

### I-006 — Effect ownership

Every authoritative effect belongs to exactly one accepted financial operation.

Orphan effects and effects shared by multiple operations are invalid.

### I-007 — Effect identity

Every effect identifies, at minimum:
- operation;
- account;
- amount;
- currency;
- financial direction/effect type;
- authoritative ordering/effective-time information.

### I-008 — Atomic effect set

An operation's complete effect set is accepted atomically.

A partially applied operation is invalid financial state.

### I-009 — Operation semantics

The effects produced by an accepted operation must satisfy that operation's approved business semantics.

For example:
- income increases the receiving account;
- personal expense decreases the spending account;
- owned-account transfer decreases the source and increases the destination.

## 5. Transfer invariants

### I-010 — Distinct transfer endpoints

A transfer must have distinct source and destination accounts unless an explicit future policy defines otherwise.

### I-011 — Transfer conservation

For same-currency transfers with no explicit fee/adjustment:

source decrease = destination increase

Fees, taxes, adjustments, and conversions must be represented explicitly rather than hidden inside transfer semantics.

### I-012 — Transfer atomicity

A transfer is one business operation. All required effects succeed or none do.

## 6. Balance invariants

### I-013 — Deterministic balance

For an account and defined point in time, the balance is reproducible from:
- valid opening/initial state;
- authoritative effects applicable to that point;
- approved currency semantics.

### I-014 — Explainable balance

Every displayed balance must be explainable through authoritative history.

Divergence between displayed/materialized state and authoritative state is a correctness failure.

### I-015 — Materialized balance is derived

A stored balance may improve performance, but it must be rebuildable and verifiable against authoritative effects.

## 7. Temporal and ordering invariants

### I-016 — Explicit financial time

The model must distinguish the time an event is financially effective from the time it was recorded when that distinction matters.

### I-017 — Deterministic ordering

When multiple effects share or interact around a time boundary, their authoritative ordering must be deterministic and reproducible.

The authoritative ordering policy is `effective_at`, with `recorded_at` retained for audit/context; deterministic tie-breaking must be defined by the transaction/persistence design.

## 8. History and correction

### I-018 — No silent historical mutation

Accepted financial effects must not be silently changed in a way that alters previously accepted financial truth.

### I-019 — Correction is explicit

A correction must preserve an explainable relationship between the original accepted operation and the resulting correction/reversal or approved edit.

### I-020 — Historical explainability

A reviewer must be able to determine:
- what was originally accepted;
- what changed;
- when/how the correction became effective;
- why the resulting balance changed.

## 9. Currency invariants

### I-021 — Currency is explicit

Every account and authoritative effect has unambiguous currency semantics.

### I-022 — No implicit cross-currency behavior

No cross-currency operation may use a hidden or assumed exchange rate.

If multi-currency is approved, rate source/provenance, effective time, precision, rounding, valuation, and transfer representation must be explicit.

## 10. Idempotency and retries

### I-023 — Safe retry

Retrying an already accepted operation must not create unintended duplicate effects.

### I-024 — Idempotency boundary

The system must define what constitutes the same operation/retry and where the idempotency key is authoritative.

The idempotency boundary is the accepted financial operation command/identity; retries of the same accepted operation must resolve to the same financial result without creating duplicate effects. Exact storage mechanics remain a DG-004/persistence design concern.

### I-025 — Deterministic rejection

The same invalid operation under the same authoritative state produces a consistent rejection category.

## 11. Validation boundaries

### I-026 — Domain validation is authoritative

UI/API/client validation may improve usability but cannot replace domain validation.

### I-027 — Validate before mutation

All applicable financial invariants are validated before authoritative effects are committed.

## 12. Failure and recovery

### I-028 — No partial financial commit

A failed operation must not leave partial financial effects.

### I-029 — Invalid operations do not change truth

Rejected operations do not mutate authoritative financial state.

### I-030 — Recoverable financial state

There is a defined path to reconstruct authoritative state after ordinary storage/cache failure.

### I-031 — Rebuild preserves results

Rebuilding derived state from authoritative effects produces the same accepted financial results.

## 13. Additional candidate policy points

- zero-value behavior is policy-driven by operation/effect semantics;
- fees/adjustments are explicit effects or explicit future operation semantics, never hidden;
- affected-account limits are not a financial invariant and remain transaction-model policy;
- `effective_at` and `recorded_at` are distinct, with deterministic ordering defined downstream;
- correction/reversal is explicit through new Operations/Effects;
- idempotency belongs to accepted operation identity, with persistence mechanics downstream;
- informational/no-effect operations are not part of the MVP balance-changing financial truth unless explicitly introduced;
- concurrency/conflict behavior is a downstream architecture policy for multi-device/offline scenarios.

## 14. Candidate verification matrix

| Category | Required verification |
|---|---|
| Money | exact representation, precision, invalid values, zero policy |
| Operation/effect | ownership, completeness, atomicity, no orphan effects |
| Income/expense | correct directional effect |
| Transfer | distinct accounts, conservation, atomicity |
| Balance | deterministic rebuild and explainability |
| Ordering | same input/state produces same result |
| Correction | explicit history and resulting balance |
| Currency | explicit currency and no implicit conversion |
| Retry | duplicate prevention |
| Failure | no partial commit |
| Recovery | rebuild consistency |
| Validation | domain rules independent of UI |
| Evidence/AI | cannot mutate authoritative truth |

## 15. Gate dependencies

DG-003 depends on:
DG-001 Product Scope → DG-002 Domain Model

DG-003 then informs:
DG-004 Transaction Model → DG-005 Balance Model

No production financial implementation should depend on unresolved invariant policy.

## 16. Decision record

**Decision:** Accepted  
**Decision owner:** Khaled  
**Approval date:** 2026-09-20  
**Approval:** Recommendations accepted. The Tier 1 financial invariants are mandatory; Tier 2 domain policies and Tier 3 future architecture policies remain explicitly bounded as described above.
