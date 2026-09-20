# DG-014 — Account Lifecycle

**Status:** Accepted  
**Date:** 2026-09-21  
**Phase:** M2 — Trustworthy Financial Core

## Decision

Mizan uses a two-state account lifecycle:

- **Active** — balance-changing operations are allowed.
- **Closed** — normal new Income, PersonalExpense, and OwnedAccountTransfer operations are rejected.

Lifecycle transitions are:

**Active → Closed → Active**

Closing an account is a lifecycle state change only. It does not mutate, delete, or rewrite historical Operations or Effects.

## Accepted semantics

1. Every newly created account starts **Active**.
2. An Active account may be closed.
3. A Closed account may be reopened.
4. Closing an already Closed account is invalid.
5. Reopening an already Active account is invalid.
6. Normal balance-changing operations cannot target a Closed account.
7. Existing balance, history, and explanation reads remain available for Closed accounts.
8. A Reversal remains allowed against an accepted historical operation even when its account is Closed. Reversal is a financial correction of authoritative history, not a normal new transaction against the account.
9. Accounts are not deleted by lifecycle operations. Historical financial truth remains retained.
10. No Suspended state or separate Archive concept is introduced at this stage.

## Alternatives and trade-offs

### Option A — Active / Closed
Accepted.

- Smallest state model that expresses the required lifecycle.
- Clear operational rule for whether normal transactions are allowed.
- Reopening avoids treating a lifecycle state as an irreversible financial fact.
- Keeps historical financial truth independent from account lifecycle.

### Option B — Active / Suspended / Closed
Not selected.

- Adds a distinct operational state and additional transition rules.
- No current M2 requirement justifies different transaction semantics for Suspended.
- Can be introduced later if a concrete product requirement needs it.

### Option C — Archive
Not selected as the lifecycle model.

- Archiving can be a presentation/read concern, but it does not by itself define financial transaction semantics.
- It would leave the core question of whether new operations are allowed unresolved.

## Consequences

- Account persistence stores lifecycle status.
- Close/Reopen are application commands over the Account aggregate.
- Normal operation acceptance validates that every affected account is Active.
- Reversal intentionally bypasses this normal-new-operation restriction so historical correction remains possible.
- Balance/history/explanation queries continue to work for Closed accounts.
- There is no account deletion behavior in this gate.

## Verification contract

The implementation must prove:

- new accounts are Active;
- close/reopen transitions work;
- duplicate transition attempts fail;
- Closed accounts reject Income, Expense, and Transfer;
- Closed account balance/history remain readable;
- Reversal remains possible for an accepted operation after closure;
- lifecycle status survives PostgreSQL persistence;
- no delete endpoint or delete semantics are introduced.
