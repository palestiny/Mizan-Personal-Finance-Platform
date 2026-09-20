# Project Status — Mizan

## Current phase

**M2 — Trustworthy Financial Core**

Mizan's authoritative financial model is **Operation + Effect**. Only accepted immutable Operations create authoritative Effects. Reversals and corrections are represented by new Operations/Effects.

## Verified capabilities

| Capability | State |
|---|---|
| M1 thin vertical slice | Implemented and verified |
| M2 explicit reversal | Implemented, merged, and runtime-verified |
| M2 account lifecycle | Implemented, merged, and runtime-verified |
| M2 recoverables / reimbursements | Implemented, merged, and runtime-verified |

### M2 Recoverables / Reimbursements

- Recoverables are authoritative financial state represented by immutable Recoverable Effects.
- A Recoverable Expense decreases an owned account balance and creates an outstanding recoverable claim.
- Recoverable Settlement increases the owned account balance and reduces the claim.
- Partial settlement is supported.
- Settlement cannot exceed outstanding recoverable balance.
- Recoverable currency is explicit and settlement currency must match.
- Reversal remains the correction mechanism.
- Counterparty identity is intentionally deferred to a first-class Person/Contact entity.
- Waiver/write-off is deferred.
- Concurrent over-settlement is protected through serializable transaction handling and verification coverage.

## Verification

GitHub Actions run **#140** completed successfully after the final concurrency-safe recoverable implementation.

Verified in that run:
- Build
- PostgreSQL-backed EF migration application
- Domain tests
- Application tests
- existing financial/idempotency/concurrency/balance/reversal tests
- Account Lifecycle API scenarios
- Recoverable partial settlement
- Recoverable concurrent over-settlement protection
- Recoverable over-settlement rejection
- Recoverable expense reversal

## Next action

M2 Recoverables is GREEN and merged. Continue M2 by identifying the next concrete capability from the accepted roadmap. Open a new design gate only if the next capability introduces a material product, domain, or architecture decision.
