# DG-002 — Domain Model Decision Gate

## Status

**State:** Draft — depends on DG-001 product-scope decisions
**Phase:** M0 — Product & Domain Foundation

This gate defines the candidate domain model without silently converting unresolved product decisions into architecture.

## 1. Design objective

Define the smallest domain model capable of representing the proposed personal-finance core:

**Account → Financial Record → Balance → History**

The model must preserve financial correctness and make the source of truth explicit.

## 2. Candidate domain concepts

### Account

Represents a place where money is held or tracked.

Candidate responsibilities:
- Stable identity
- User-visible name
- Account type
- Currency
- Lifecycle/status
- Current financial state derived from authoritative records

An Account should not own duplicated financial history as an independent source of truth.

### Financial Record

Represents an accepted financial change.

Candidate categories:
- Income
- Expense
- Transfer

A financial record must have an explicit financial effect and enough information to reconstruct the affected account balance.

### Transfer

A transfer is a single business operation that moves value between two accounts owned by the user.

The domain must treat the transfer as one atomic financial operation, even if persistence later represents its effects using more than one record.

### Balance

A balance is the financial state of an account at a point in time.

Candidate rule:

**Balance = opening/initial state + authoritative financial effects**

A cached or materialized balance may exist for performance, but it must not become the independent source of financial truth.

### History

History is the ordered/auditable representation of accepted financial changes affecting an account.

History must be sufficient to explain how the account reached its balance.

### Category

Category is a classification of a financial record, not the financial event itself.

It should remain outside the minimum financial truth model unless the approved MVP workflows require it.

## 3. Candidate relationships

- User owns one or more Accounts.
- An Account has one Currency.
- An Account is affected by zero or more Financial Records.
- An Income affects one Account positively.
- An Expense affects one Account negatively.
- A Transfer affects exactly two Accounts: source and destination.
- A Financial Record may optionally have a Category.
- Balance is derived from the account's authoritative financial effects.

## 4. Candidate invariants to validate in DG-003

The following are design candidates, not yet final invariants:

1. Money must never be represented using floating-point arithmetic.
2. A financial record must have a valid monetary amount.
3. An accepted expense cannot increase the affected account.
4. An accepted income cannot decrease the affected account.
5. A transfer must have distinct source and destination accounts.
6. A transfer must preserve total value across its two affected accounts, excluding explicit fees/adjustments if those are later introduced.
7. An account balance must be explainable from authoritative financial records.
8. Historical financial records must not be mutated in a way that silently changes previously accepted financial truth.
9. Invalid financial operations must not partially commit.
10. Currency semantics must be explicit before multi-currency behavior is implemented.

DG-003 must turn these candidates into explicit, testable rules.

## 5. Correction semantics dependency

The domain model intentionally does not choose the final correction strategy yet.

Possible product-level policies from DG-001:
- Direct edit
- Compensating/reversal operation
- Mixed policy

The chosen policy determines whether Financial Record is mutable, immutable, or has controlled correction states.

## 6. Currency dependency

The model assumes an Account has a currency, but does not yet define conversion.

The final decision depends on DG-001:
- Single currency: conversion can remain outside the MVP.
- Multi-currency: exchange-rate provenance, conversion timing, valuation semantics, and transfer rules require explicit design.

No implicit conversion behavior should be introduced.

## 7. Offline dependency

The model itself should remain independent of synchronization concerns.

If offline is mandatory, later architecture must define:
- local authoritative write behavior
- synchronization
- conflict handling
- durable recovery

These concerns must not leak into the core financial invariants.

## 8. Deliberately deferred concepts

Do not introduce these into the core domain model yet:
- Budget
- Goal
- Forecast
- Investment portfolio
- Bank connection
- AI recommendation
- Autonomous financial action
- Household collaboration
- Business accounting

They may be added through separate design gates when product scope requires them.

## 9. Domain boundary proposal

The candidate core boundary is:

**Financial Account Management**
→ Accounts
→ Financial Records
→ Transfers
→ Balance derivation
→ Financial History

Reporting, intelligence, integrations, and UI should consume this domain rather than redefine financial truth.

## 10. Open decisions

DG-002 cannot be finalized until the following DG-001 decisions are explicit:

- Target user boundary
- MVP financial scope
- Account scope
- Currency scope
- Offline requirement
- Historical correction policy
- Data portability

## 11. Gate sequence

After DG-001 is accepted:

**DG-002 Domain Model**
→ **DG-003 Financial Invariants**
→ **DG-004 Transaction Model**
→ **DG-005 Balance Model**

Only after these gates are sufficiently resolved should production implementation begin.

## 12. Decision record

**Decision:** Open
**Decision owner:** Khaled
**Rationale:** Candidate model prepared; final model depends on accepted product-scope decisions.
