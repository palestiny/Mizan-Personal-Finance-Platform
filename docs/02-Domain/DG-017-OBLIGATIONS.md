# DG-017 — Obligations / Expected Future Payments

**Status:** Accepted — Product Owner approved 2026-09-21

## Decision

Mizan introduces a first-class Obligation state for expected future payments.

### Rules

1. An obligation is planning/expectation state, not an actual financial event.
2. A Planned obligation does not create Account Effects and never changes account balance.
3. An obligation has explicit amount, currency, description, and due date.
4. Lifecycle:
   - Planned → Settled
   - Planned → Cancelled
   - Settled and Cancelled are terminal.
5. An obligation may not be settled or cancelled twice.
6. Actual payment remains a separate FinancialOperation and must be recorded independently.
7. The MVP does not automatically link an obligation to a payment operation.
8. No recurring-obligation engine, installments, reminders, prediction engine, or generalized liability/accounting engine is introduced.
9. No account is required to create, settle, or cancel an obligation because obligation state itself is not an account movement.

## Trade-off

A first-class state gives Mizan authoritative planning data for future forecasting and alerts while preserving the core invariant that account balances contain only actual financial Effects.

Automatic payment linkage is deferred to avoid coupling planned expectations to financial truth before the product has evidence for the correct matching semantics.
