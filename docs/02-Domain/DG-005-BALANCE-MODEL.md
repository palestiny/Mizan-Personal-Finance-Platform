# DG-005 — Balance Model Decision Gate

## Status
**State:** Draft — decision-ready candidate
**Phase:** M0 — Product & Domain Foundation
**Decision owner:** Khaled

## Purpose
Define how Mizan computes, stores, rebuilds, orders, and explains account balances without allowing a materialized balance to become an independent source of financial truth.

## 1. Balance source of truth

**Option A — Recompute from authoritative Effects (recommended)**
Balance at time T = opening state + applicable authoritative Effects up to T.

**Option B — Stored balance as primary truth**
Fast reads, but creates a second financial truth and complicates correction/recovery.

**Option C — Effects + periodic snapshots**
Strong performance/rebuild story, but adds persistence complexity before scale requires it.

**Recommendation:** A as domain truth. Materialized balance may be introduced only as a derived optimization.

## 2. Sign convention

**Recommended:** Effects carry explicit direction/type while `Money` remains an absolute magnitude.
- Income: positive effect
- Personal expense: negative effect
- Transfer source: negative effect
- Transfer destination: positive effect

This avoids ambiguous negative amounts while keeping effect semantics explicit.

## 3. Opening / initial balance

**Recommended:** account opening state is explicit and immutable after acceptance.
If the initial balance needs correction, create an explicit correction operation rather than silently editing history.

## 4. Point-in-time balance

**Recommended:** balance is evaluated at a defined point in time using `effective_at` ordering.
A current balance is the balance at the current authoritative point.

## 5. Ordering

Recommended deterministic ordering key:
1. `effective_at`
2. deterministic persisted effect/order identity for ties
3. `recorded_at` retained for audit/context

The persistence-level tie-breaker must be stable and reproducible.

## 6. Materialized balance

**Recommended:** optional derived balance cache/read model.
Rules:
- never authoritative;
- rebuildable from Effects;
- update/reconciliation cannot create financial truth;
- divergence is detectable;
- deleting it must not lose financial truth.

## 7. Corrections and historical balances

A correction is a new Operation/Effect set.
Therefore:
- historical balances before the correction's `effective_at` remain unchanged;
- balances at/after the correction reflect the new Effects;
- explanation can traverse original → correction/reversal.

## 8. Balance explanation

Every balance should be explainable as:
`Opening State + ordered authoritative Effects = Balance`

Categories, evidence, AI summaries, and natural language may enrich the explanation but cannot replace the authoritative calculation.

## 9. Negative balances

**Recommended:** allow negative balances at the generic Account level unless an account policy explicitly forbids them.
The core model should not assume every real-world account behaves like cash.

## 10. Precision and rounding

Balance arithmetic uses the accepted Money representation and never rounds through floating point.
Rounding is introduced only by an explicit domain rule; MVP single-currency balance calculation does not need exchange-rate rounding.

## 11. Rebuild and recovery

**Recommended rebuild contract:**
`Account opening state + all valid authoritative Effects → deterministic balance/history state`

Rebuild must produce the same result as normal operation processing.

## 12. Multi-account / global balance

MVP account balances are authoritative per account.
A global total is a derived view and must not become a second source of truth.
Because MVP is single-currency, aggregation across accounts is permitted within the same user currency context.

## 13. Candidate verification matrix

| Concern | Verification |
|---|---|
| Determinism | same authoritative inputs produce same balance |
| Historical point-in-time | balance at T excludes later effective effects |
| Correction | correction changes only applicable historical points |
| Transfer | source/destination balances change atomically |
| Rebuild | rebuilt balances equal normal balances |
| Materialized cache | deleting/rebuilding cache preserves results |
| Explanation | every balance delta maps to authoritative Effects |
| Negative balance | policy is explicit and testable |
| Ordering | ties produce deterministic results |
| Global total | equals sum of authoritative account balances |

## 14. Open decisions for Product Owner

1. Source of truth: A/B/C
2. Effect sign representation: explicit direction + absolute amount vs signed amount
3. Opening balance: immutable initial state vs opening operation
4. Point-in-time semantics: `effective_at` only vs `effective_at` + explicit sequence
5. Materialized balance: allowed optimization vs required read model
6. Negative balance: allowed by default vs account-policy restricted
7. Global balance: derived view only vs persisted aggregate
8. Balance explanation: core domain result vs application/read-model concern

## 15. Gate dependencies

DG-005 depends on DG-004 and DG-003.
DG-005 is the final M0 domain gate before the thin vertical slice.

## 16. Decision record

**Decision:** Open
**Approval:** Not granted.
