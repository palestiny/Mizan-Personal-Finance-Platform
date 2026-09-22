# Project Status — Mizan

## Current phase

**M2 — Trustworthy Financial Core — COMPLETE**

Mizan's authoritative financial model is **Operation + Effect**. Only accepted immutable Operations create authoritative Effects. Reversals and corrections are represented by new Operations/Effects.

## Verified capabilities

| Capability | State |
|---|---|
| M1 thin vertical slice | Implemented and verified |
| M2 explicit reversal | Implemented, merged, and runtime-verified |
| M2 account lifecycle | Implemented, merged, and runtime-verified |
| M2 recoverables / reimbursements | Implemented, merged, and runtime-verified |
| M2 shared expenses / advances | Implemented, merged, and runtime-verified |
| M2 obligations / expected future payments | Implemented, merged, and runtime-verified |

### M2 Shared Expenses / Advances

- Shared Expense records the total payment as an account decrease and the recoverable portion as a Recoverable Effect.
- The personal portion is derived as Total - Recoverable; no separate accounting engine is introduced.
- Recoverable Expense continues to represent a fully recoverable advance.
- Recoverable settlement semantics remain unchanged, including partial settlement and currency matching.
- Reversal preserves both account and recoverable-effect integrity and rejects a reversal that would make the recoverable balance negative; settlements must be reversed first.
- Idempotent retry semantics are preserved for Shared Expense commands.
- No Person/Contact, group-splitting engine, waiver/write-off, or generalized receivables engine is introduced.

## M2 Obligations / Expected Future Payments

- Obligations have first-class Planned / Settled / Cancelled state.
- Planned obligations create no Account Effects and do not change account balances.
- Actual payment remains a separate FinancialOperation; automatic payment linkage is intentionally deferred.
- PostgreSQL persistence, lifecycle API, domain/API tests, and committed migration application are verified.
- No recurring engine, installments, reminders, prediction engine, or generalized liability/accounting engine is introduced.

## Verification

GitHub Actions runs **#158** and **#160** completed successfully after the recoverable settlement vs. original reversal concurrency verification.

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
- Recoverable settlement vs. original-operation reversal concurrency protection
- Recoverable outstanding balance rebuildability from authoritative Recoverable Effects
- Shared Expense creation and account/recoverable effects
- Shared Expense idempotent retry
- invalid recoverable portion rejection
- settled-recoverable reversal protection
- settlement-before-reversal ordering

## M3 Proposal-first Capture — First Vertical Slice

- DG-018 Natural-Language Capture is accepted with Proposal-first confirmation.
- DG-019 Proposal Model & Lifecycle is accepted with persisted, non-authoritative Proposal state.
- First runtime slice is implemented and runtime-verified without an AI/LLM provider.
- Proposal lifecycle is persisted as Draft / ReadyForConfirmation / Confirmed with Rejected / Expired / Failed terminal states.
- Confirmation reuses the existing Financial Command idempotency boundary; Proposal confirmation cannot create Effects directly.
- Concurrent confirmation cannot create duplicate authoritative Operations.
- PostgreSQL migration, API lifecycle, confirmation path, retry semantics, rejection behavior, and concurrency verification are covered by automated tests.
- Exact expiration TTL remains deferred to UX/implementation evidence.
- Capture interpretation remains vendor-neutral; AI/OCR/voice/import adapters are not introduced in this slice.

## Verification

GitHub Actions run **#189** completed successfully after the M3 Proposal vertical slice implementation and verification.

Verified:
- Build
- Committed PostgreSQL EF migration application
- Domain tests
- Application Proposal lifecycle tests
- Existing financial/application test coverage
- Proposal create/read/prepare/reject/confirm API paths
- Proposal confirmation through existing financial command execution
- Same-Proposal confirmation retry semantics
- Concurrent confirmation with different idempotency keys
- Rejection and incomplete Proposal behavior
- No AI/LLM provider dependency

## Next action

M3 Proposal-first capture boundary is now runtime-verified. The next slice is the **capture interpreter adapter**: convert a user input into a vendor-neutral Proposal without allowing interpretation to mutate authoritative financial state. Any new material product/domain semantics must open an explicit design gate before implementation.
