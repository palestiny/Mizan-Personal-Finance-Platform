# DG-015 — Recoverables / Reimbursements

## Status
**State:** Accepted — Product Owner approved 2026-09-21
**Phase:** M2 — Trustworthy Financial Core
**Decision owner:** Khaled

## Purpose
Represent money paid on behalf of another person as an outstanding recoverable claim without turning Mizan into a generalized receivables/accounting engine.

## Decision

### 1. Recoverable is authoritative financial state
A recoverable is not merely expense metadata. The outstanding claim is authoritative and rebuildable from immutable accepted Operations and their Recoverable Effects.

The existing financial model remains authoritative:

**Operation → Account Effects + Recoverable Effects**

Account balances continue to derive only from Account Effects. Recoverable balances derive from Recoverable Effects.

### 2. No Person/Customer domain entity yet
The counterparty is represented as a normalized display name on the recoverable-creation effect. A first-class Person/Contact aggregate is deferred until product evidence requires identity, merging, contact data, or shared spaces.

### 3. Operation types
M2 adds two operation types:
- RecoverableExpense — money leaves an owned account and creates a recoverable claim.
- RecoverableSettlement — money enters an owned account and reduces an existing recoverable claim.

Both are immutable accepted Operations. Reversal remains the correction mechanism.

### 4. Partial settlement
Multiple settlements are allowed. A settlement must be greater than zero and cannot exceed the current outstanding amount.

Outstanding = sum(Increase Recoverable Effects) − sum(Decrease Recoverable Effects).

### 5. Completion
No separate mutable Settled flag is stored as financial truth. A recoverable is outstanding while its derived balance is greater than zero and settled at zero.

### 6. Waiver / write-off
Waiver is explicitly deferred. It must not be represented by deleting or mutating a claim. If later required, it will be introduced as a new authoritative Operation/Effect with its own semantics and audit trail.

### 7. Account lifecycle interaction
Creating a recoverable expense and settling a recoverable are normal balance-changing operations and therefore require Active accounts. Reversal remains allowed for accepted historical operations after account closure, consistent with DG-014.

### 8. Currency
A recoverable has one explicit currency. Settlement currency must match the recoverable currency.

## MVP scope
Included:
- create recoverable through a recoverable expense;
- counterparty display name;
- outstanding balance;
- partial settlement;
- full settlement;
- idempotent retries/conflict handling;
- immutable history;
- reversal of recoverable expense and settlement;
- rebuildable recoverable balance.

Deferred:
- Person/Contact aggregate;
- due dates/reminders;
- interest/fees;
- automatic collection;
- waiver/write-off;
- multi-currency settlement;
- household/shared ownership;
- generalized accounts receivable.

## Invariants
1. Every Recoverable Effect belongs to an accepted Operation.
2. Recoverable Effect amounts are positive; direction carries meaning.
3. A recoverable expense creates exactly one positive Recoverable Effect.
4. A settlement creates exactly one negative Recoverable Effect.
5. Settlement cannot exceed outstanding balance.
6. Reversal creates exact inverse effects for both account and recoverable state.
7. Accepted Operations and Effects remain immutable.
8. Idempotency semantics match existing financial commands.
9. Recoverable state can be rebuilt deterministically from authoritative Recoverable Effects.

## Trade-offs
- Metadata only: simplest, but cannot reliably answer outstanding claims or support partial settlement.
- First-class recoverable effects: adds a bounded financial state model, but preserves auditability, rebuildability, and future extensibility without introducing a full accounting engine. **Selected.**
- Full receivables/accounting subsystem: richer, but disproportionate to the current personal-finance MVP. **Deferred.**

## Verification contract
The implementation is GREEN only when CI proves:
- domain creation/settlement/reversal invariants;
- partial and full settlement;
- over-settlement rejection;
- idempotency;
- account closure interaction;
- PostgreSQL migration application;
- API recoverable expense, settlement, outstanding balance, and reversal behavior.
