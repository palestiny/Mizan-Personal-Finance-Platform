# DG-003 — Financial Invariants Decision Gate

## Status

**State:** Draft — candidate rules only; depends on DG-001 and DG-002 decisions  
**Phase:** M0 — Product & Domain Foundation

This gate converts the candidate domain model into explicit, testable financial invariants. It does not approve unresolved product policy.

## 1. Purpose

Mizan's most important technical property is that financial state remains explainable, deterministic, and correct.

The invariants in this document define rules that must remain true regardless of UI, API, storage engine, cache, AI component, or synchronization mechanism.

Production code must not be written against an unapproved interpretation of these rules.

## 2. Source-of-truth invariant

### I-001 — Authoritative financial history

The system must have one authoritative representation of accepted financial effects.

Derived values such as:

- cached balances
- dashboard totals
- reports
- charts
- AI insights
- search indexes

must never become an independent source of financial truth.

**Verification direction:** a derived balance can be rebuilt from authoritative financial state and produce the same result.

## 3. Monetary representation

### I-002 — No floating-point money

Financial amounts must not use binary floating-point representation for authoritative monetary calculations.

The representation must preserve exact monetary semantics appropriate to the selected currency model.

**Dependency:** DG-001 currency decision.

### I-003 — Valid monetary amount

Every accepted financial effect must satisfy an explicit amount rule.

At minimum:

- amount is present
- amount is finite
- amount is not NaN
- amount conforms to the selected precision
- zero-value operations are either explicitly allowed or explicitly rejected

The zero-value policy must be decided before transaction implementation.

## 4. Account-effect invariants

### I-004 — Every accepted financial operation has explicit effects

An accepted financial operation must make its financial effect on each affected account explicit.

The system must not infer a balance change from UI labels, categories, descriptions, or AI output.

### I-005 — Income effect

An income operation must increase the balance of its receiving account by its accepted monetary effect.

### I-006 — Expense effect

An expense operation must decrease the balance of its spending account by its accepted monetary effect.

The domain representation may use signed effects or another explicit representation; the invariant is the financial result, not the storage shape.

## 5. Transfer invariants

### I-007 — Distinct transfer endpoints

A transfer must have a distinct source account and destination account.

A self-transfer must be rejected unless a later product decision explicitly defines a legitimate use case.

### I-008 — Transfer conservation

A transfer must preserve value between its source and destination accounts.

For a same-currency transfer with no explicit fee or adjustment:

**source decrease = destination increase**

The transfer must not create or destroy value.

If fees, taxes, adjustments, or cross-currency conversion are introduced, their effects must be represented explicitly rather than hidden inside the transfer rule.

### I-009 — Atomic transfer

A transfer is one business operation.

Either all required financial effects are accepted, or none are accepted.

A partially applied transfer is invalid financial state.

## 6. Balance invariants

### I-010 — Deterministic balance

For an account and a defined point in time, the balance must be deterministically reproducible from:

- the account's valid initial/opening state
- authoritative financial effects applicable to that point in time
- the approved currency semantics

### I-011 — Explainable balance

Every balance must be explainable through the financial history that produced it.

If a displayed balance cannot be reconciled with authoritative financial state, the system must treat that as a correctness failure rather than silently repairing the number.

### I-012 — Materialized balance is derived state

A stored or cached balance may be used for performance, but it must be recoverable/rebuildable from authoritative state.

The implementation must define a verification path that can detect divergence.

## 7. Atomicity and failure

### I-013 — No partial financial commit

A financial operation that fails validation or persistence must not leave a partial financial effect.

This applies especially to:

- transfers
- corrections
- multi-record operations
- future synchronized operations

### I-014 — Invalid operations do not change financial truth

If a financial operation violates an invariant, it must be rejected without changing authoritative financial state.

## 8. History and correction

### I-015 — No silent historical mutation

An accepted financial event must not be changed in a way that silently changes previously accepted financial truth.

The exact correction mechanism remains unresolved until DG-001 decides between:

- direct edit
- compensating/reversal operation
- mixed policy

### I-016 — Correction must remain explainable

Whatever correction policy is approved, the resulting financial state must remain explainable.

A user/reviewer must be able to determine:

- what was originally accepted
- what correction occurred
- why the resulting balance changed

The implementation details belong to DG-004 after the product policy is decided.

## 9. Currency invariants

### I-017 — Currency is explicit

Every account and authoritative monetary effect must have unambiguous currency semantics.

No operation may rely on an implicit currency conversion.

### I-018 — No implicit cross-currency transfer

A cross-currency operation must not be accepted using a hidden or assumed exchange rate.

If multi-currency is approved, DG-004/DG-005 must define:

- exchange-rate source/provenance
- effective time
- precision/rounding
- valuation semantics
- treatment of rate changes
- transfer representation

## 10. Idempotency and retries

### I-019 — Safe retry semantics

Retrying an already accepted financial operation must not create an unintended duplicate financial effect.

The exact idempotency mechanism belongs to the transaction/persistence design, but the business invariant is mandatory.

### I-020 — Deterministic rejection

The same invalid operation under the same authoritative state should produce a consistent rejection reason/category.

This supports debugging, client behavior, automated testing, and safe retries.

## 11. Validation boundaries

### I-021 — Domain validation is authoritative

UI validation, API validation, and client-side checks may improve usability, but they must not replace domain-level financial validation.

The authoritative rules must remain enforceable independently of presentation.

### I-022 — Validation must precede financial mutation

An operation must pass all applicable business invariants before its authoritative financial state is changed.

## 12. Recovery and rebuild

### I-023 — Financial state must be recoverable

The system must have a defined path to reconstruct authoritative financial state after an ordinary storage/cache failure.

This does not yet choose a database or storage architecture.

### I-024 — Rebuild must preserve results

Rebuilding derived state from authoritative financial data must produce results consistent with the accepted financial state.

## 13. Candidate test matrix

Before production implementation, DG-003 should map each invariant to executable verification.

Minimum categories:

| Category | Required verification |
|---|---|
| Money | precision, invalid values, zero policy |
| Income | positive effect, invalid amount |
| Expense | negative financial effect, invalid amount |
| Transfer | distinct accounts, conservation, atomicity |
| Balance | deterministic calculation, explainability |
| Correction | approved policy, audit/explainability |
| Currency | explicit currency, conversion rules |
| Retry | duplicate prevention |
| Failure | rollback/no partial commit |
| Recovery | rebuild consistency |
| Validation | domain rules independent of UI |

## 14. Decisions still blocking finalization

DG-003 cannot be finalized until the following are explicit:

- DG-001 target user boundary
- DG-001 MVP scope
- DG-001 account scope
- DG-001 currency scope
- DG-001 offline requirement
- DG-001 correction policy
- DG-001 portability scope
- DG-002 final domain model

Additional decisions required during this gate:

- zero-value financial operation policy
- exact monetary precision policy
- treatment of fees/adjustments
- exact historical ordering semantics
- idempotency boundary/key semantics

These should be decided before production transaction implementation.

## 15. Gate sequence

**DG-001 Product Scope**
→ **DG-002 Domain Model**
→ **DG-003 Financial Invariants**
→ **DG-004 Transaction Model**
→ **DG-005 Balance Model**
→ production implementation

## 16. Decision record

**Decision:** Open  
**Decision owner:** Khaled  
**Rationale:** Candidate invariants prepared for review; final rules depend on accepted product/domain decisions.
