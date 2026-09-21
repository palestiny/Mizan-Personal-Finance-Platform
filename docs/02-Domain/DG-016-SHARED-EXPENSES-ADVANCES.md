# DG-016 — Shared Expenses / Advances

## Status
**Accepted — Product Owner approved 2026-09-21**

Implementation refinement: a reversal of a recoverable-creating operation is rejected when prior settlements would make the recoverable balance negative. This preserves the non-negative recoverable invariant; settlements must be reversed first.

## Problem
Mizan must represent cases where a person pays an amount that is partly their own expense and partly an amount recoverable from another person.

Examples:
- Paying a restaurant bill where part belongs to another person.
- Paying a shared purchase up front and expecting another person to reimburse their share.
- Paying an advance entirely on behalf of another person.

## Decision
Extend the existing Recoverable model instead of introducing a new mutable aggregate or generalized receivables engine.

### Authoritative model
The existing model remains authoritative:

**Operation → Account Effects + Recoverable Effects**

- Account balances derive only from Account Effects.
- Recoverable balances derive only from Recoverable Effects.
- Accepted Operations and Effects remain immutable.
- Reversal remains the correction mechanism.

### Operation semantics
1. **RecoverableExpense**
   - Represents an amount paid entirely on behalf of another person.
   - Account decreases by the full amount.
   - Recoverable increases by the full amount.
   - This also represents a 100% advance.

2. **SharedExpense**
   - Represents a payment containing both a personal portion and a recoverable portion.
   - Account decreases by the full payment amount.
   - Recoverable increases by the recoverable portion.
   - Therefore:
     **Personal portion = Total payment − Recoverable portion**
   - Recoverable portion must be strictly less than or equal to the total payment.
   - A zero recoverable portion is rejected; ordinary personal expense should be used instead.
   - The recoverable portion must be positive.
   - Counterparty name is required.

### Settlement
Existing Recoverable Settlement semantics remain unchanged:
- partial settlement is allowed;
- multiple settlements are allowed;
- settlement cannot exceed outstanding balance;
- settlement currency must match;
- settlement requires an Active account.

### Advances
No separate Advance entity is introduced.
A full advance is represented by RecoverableExpense, preserving one financial model.

### Identity
No Person/Contact entity is introduced.
Counterparty remains normalized display text until identity/merging/shared-space requirements justify a first-class entity.

### Correction
Reversal of a SharedExpense reverses both:
- its Account Effect;
- its Recoverable Effect.

If the recoverable portion has already been settled in any amount, reversing the SharedExpense directly is rejected because it would create a negative recoverable balance. The settlement reversal must happen first. The original operation remains immutable.

## Invariants
- Total account decrease equals the submitted total payment.
- Recoverable increase equals the submitted recoverable portion.
- Recoverable portion > 0.
- Recoverable portion <= total payment.
- Total payment and recoverable portion use the same currency.
- Counterparty name is required and normalized.
- Normal SharedExpense requires an Active account.
- Historical reversal remains allowed after account closure when recoverable invariants remain valid.
- Reversal cannot make a recoverable balance negative.
- Idempotent retry returns the original accepted SharedExpense when command semantics match.
- Conflicting reuse of an idempotency key is rejected.

## Explicit non-goals
- No first-class Person/Contact.
- No group expense splitting among arbitrary participants.
- No percentage-based allocation engine.
- No mutable settlement/paid flags.
- No waiver/write-off.
- No generalized Accounts Receivable subsystem.

## Trade-offs
### Chosen: extend Recoverables with SharedExpense
**Pros**
- Reuses authoritative Recoverable Effects.
- Keeps balance and rebuild semantics unchanged.
- Makes shared payment semantics explicit without adding a second financial truth model.
- Full advances require no new concept.

**Cons**
- Recoverable remains a broader concept and may later need richer identity/allocation support.
- More operation types increase the application command surface.

### Rejected for now: separate Advance/SharedExpense aggregate
Rejected because it would duplicate state and introduce relationships that are not yet required by the validated MVP scenarios.

## Verification target
Add domain/application/API coverage for:
- shared expense creation;
- account balance after shared expense;
- recoverable balance after shared expense;
- partial/full settlement;
- invalid recoverable portion;
- currency mismatch;
- reversal;
- reversal after partial settlement is rejected without state corruption;
- idempotent retry/conflict.
