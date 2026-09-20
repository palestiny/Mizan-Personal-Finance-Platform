# Mizan Checkpoints

## CP-000 — Repository Bootstrap
**Date:** 2026-09-20  
**Phase:** M0 — Product & Domain Foundation  
**Status:** Completed

### Objective
Establish the minimum Git repository foundation required to begin controlled project development.

### Verification
- Git history established
- Current phase documented
- Governance and design-gate map established

## CP-001 — Product & Strategy Decision Gates
**Date:** 2026-09-20  
**Phase:** M0 — Product & Domain Foundation  
**Status:** Completed

### Objective
Accept the product scope and product strategy direction before locking the financial domain model.

### Accepted gates
- DG-001 Product Scope — Accepted
- DG-013 Product Strategy — Accepted

### Verification
- Product boundary explicitly accepted
- Global product direction established
- Egypt/personal use recorded as validation context only
- AI authority boundary established
- Commercial direction recorded as validation-driven
- GitHub decision records updated
- Issues #2 and #5 closed as completed

## CP-002 — Domain Foundation
**Date:** 2026-09-20  
**Phase:** M0 — Product & Domain Foundation  
**Status:** In progress

### Objective
Accept the financial domain model before defining the complete invariant and transaction rule set.

### Accepted gate
- DG-002 Domain Model — Accepted

### Verification
- Operation + Effect selected as authoritative financial model
- Evidence and Proposal boundaries defined
- Immutable accepted operations/effects defined
- Correction/reversal semantics defined
- MVP operation/account/currency boundaries defined
- AI/evidence cannot directly mutate authoritative financial state

### Next checkpoint
**CP-003 — Financial Invariants Decision Gate (DG-003)**

## CP-003 — Financial Invariants
**Date:** 2026-09-20  
**Phase:** M0 — Product & Domain Foundation  
**Status:** Completed

### Objective
Accept the mandatory financial invariants that govern authoritative financial truth before transaction implementation.

### Accepted gate
- DG-003 Financial Invariants — Accepted

### Verification
- Authoritative truth is the accepted Effect set
- Money uses exact non-floating representation via the approved Money abstraction
- Accepted operations/effects are atomic and immutable
- Transfers conserve value unless explicit fees/adjustments are represented
- Balances are deterministic, explainable, derived, and rebuildable
- Effective and recorded time are distinguished
- Corrections are explicit and do not silently mutate history
- Currency behavior is explicit; no implicit conversion
- Retries are idempotent and invalid operations cannot mutate truth
- Recovery/rebuild preserves financial results
- Issue #4 closed as completed

### Next checkpoint
**CP-004 — Transaction Model Decision Gate (DG-004)**


## CP-004 — Transaction Model
**Date:** 2026-09-20  
**Phase:** M0 — Product & Domain Foundation  
**Status:** Completed

### Objective
Accept transaction boundaries and lifecycle semantics before defining the balance model.

### Accepted gate
- DG-004 Transaction Model — Accepted

### Verification
- Operation Command is the transaction boundary
- Commands are separate from immutable accepted Operations
- Validation precedes effect derivation and atomic commit
- Idempotency applies to retryable balance-changing commands
- Correction/Reversal creates new Operations/Effects
- effective_at and recorded_at are retained
- Accepted results are explanation-ready
- Issue #6 closed as completed

### Next checkpoint
**CP-005 — Balance Model Decision Gate (DG-005)**
