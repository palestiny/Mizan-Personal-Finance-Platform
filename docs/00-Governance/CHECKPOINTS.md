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


## CP-005 — DG-005 Balance Model Accepted
- **Date:** 2026-09-20
- **State:** Completed
- **Gate:** DG-005 Balance Model
- **Accepted:** authoritative Effects are the source of truth; balance is derived; explicit Effect direction with absolute Money; immutable opening state; deterministic point-in-time ordering; materialized balance is derived only; negative balances allowed by default at Account level; global balance is derived; explanation is backed by authoritative state.
- **Next:** Thin vertical slice / implementation design gates as required.


## CP-006 — M1 Thin Vertical Slice Architecture Accepted
- **Date:** 2026-09-20
- **State:** Completed
- **Gate:** M1 Thin Vertical Slice
- **Accepted:** modular monolith; relational persistence abstraction; thin HTTP API; application command/query separation without full CQRS; atomic database transaction; domain + integration + API E2E tests; explicit idempotency key; effects-first balance implementation.
- **Next:** RED tests for the approved slice.


## CP-007 — M1 Runtime Technology Accepted
- **Date:** 2026-09-20
- **State:** Completed
- **Gate:** M1 Runtime & Technology Foundation
- **Accepted:** ASP.NET Core/.NET; PostgreSQL; REST/HTTP JSON; API-first thin slice without production UI; xUnit + FluentAssertions + ASP.NET integration testing; explicit Domain/Application/Infrastructure/API boundaries; EF Core; versioned EF Core migrations.
- **Issue:** #9 closed as completed.
- **Next:** RED tests for the approved thin vertical slice.


## CP-008 — M1 RED Test Foundation Established
- **Date:** 2026-09-20
- **State:** Completed
- **Phase:** M1 — Thin Vertical Slice Implementation
- **Issue:** #10
- **Objective:** Establish the executable test contract before implementing financial production behavior.
- **Established:** solution/project boundaries, Domain/Application/Infrastructure/API projects, domain/application/API test projects, core RED specifications, and CI workflow.
- **Coverage:** Money precision, operation/effect semantics, transfer conservation, immutability, derived/rebuildable balance, idempotency, atomicity, deterministic history/explanation, and one API financial path.
- **Important verification note:** The tests are intentionally ahead of the implementation. No GREEN result is claimed. The GitHub connector did not expose a workflow run for the latest workflow commit yet.
- **Next:** Minimum domain implementation → make RED tests GREEN → persistence transaction path → API path → rebuild/reconciliation verification.

## CP-009 — M1 PostgreSQL Persistence Boundary + API Path
- **Status:** Completed implementation checkpoint
- **Date:** 2026-09-20
- **Scope:** EF Core/PostgreSQL persistence boundary, application finance service/repository, atomic Operation + Effects + idempotency transaction path, thin financial API, PostgreSQL CI service configuration.
- **Verification status:** Implementation committed to GitHub; CI runtime result not yet verified because workflow execution has not yet been reported by the GitHub connector.
- **Known follow-up:** Generate and commit the first versioned EF Core migration; then verify CI and harden idempotency conflict/concurrency behavior.
- **Issue:** #11


## CP-010 — M1 Runtime Verification and Idempotency Hardening
- **Status:** Completed
- **Date:** 2026-09-21
- **Phase:** M1 — Thin Vertical Slice Implementation
- **Objective:** Verify the real PostgreSQL-backed vertical slice in GitHub CI and close the remaining idempotency semantic gap in the test harness.
- **Verified:** Build, committed EF Core migration application against PostgreSQL, full test suite, API financial flow, retry idempotency, conflicting-key behavior, concurrent duplicate protection, atomicity, deterministic history/explanation, and balance rebuildability.
- **CI:** Runs #105, #106, #107, and #108 completed successfully; #108 verifies the final cross-operation idempotency harness change.
- **Root-cause fixes completed:** compile/test API mismatches, UTC timestamp persistence normalization, duplicate test command models, and inconsistent in-memory idempotency semantics.
- **Migration:** First versioned EF Core migration is committed and successfully applied by CI.
- **Remaining M1 checkpoint:** documentation/acceptance closure only; no known failing runtime behavior remains in the verified slice.
- **Next:** close M1 as a verified thin technical foundation, then move to the next approved roadmap capability without introducing unneeded architecture.


## CP-011 — M1 Thin Vertical Slice Verified
- **Status:** Verified; closure pending Product Owner acceptance
- **Date:** 2026-09-21
- **Phase:** M1 — Thin Vertical Slice Implementation
- **Verification:** The approved slice is implemented and verified through domain, application, API, PostgreSQL, migration, idempotency, atomicity, deterministic history/explanation, and rebuildability tests.
- **CI evidence:** The latest verification run completed successfully after the final idempotency test-harness correction.
- **M1 Definition-of-Done:** All technical conditions in the accepted M1 design are now evidenced. No new architecture is required to close the slice.
- **Next:** Product Owner closure of M1, then open the next roadmap gate only when its concrete requirements justify it.
