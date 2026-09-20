# M0 — Decision Matrix

## Purpose

This matrix is the control surface for M0. It distinguishes direction, proposals, and approved decisions and prevents preparatory design work from being mistaken for approval.

## Gate map

| Gate | Current state | Depends on | Blocks |
|---|---|---|---|
| DG-001 | **Accepted** | Product direction | DG-002, DG-003, DG-004, DG-005 |
| DG-013 | **Accepted** | Product direction + discovery evidence | Release strategy, AI sequencing, commercial validation |
| DG-002 | **Accepted** | DG-001 | DG-003, DG-004 |
| DG-003 | **Accepted** | DG-002 | DG-004, DG-005 |
| DG-004 | **Accepted** | DG-003 | DG-005 |
| DG-005 | **Accepted** | DG-004 | Thin vertical slice |
| DG-006 | **Not started** | Approved product requirement + DG-005 | Offline architecture if required |
| DG-007 | **Not started** | Domain/balance requirements + concrete runtime constraints | Persistence implementation |
| DG-008 | **Not started** | Persistence + offline requirement | Multi-device synchronization |
| DG-009 | **Not started** | Product constraints + architecture constraints | Mobile implementation decisions |
| DG-010 | **Not started** | Data boundaries + concrete runtime needs | Production security controls |
| DG-011 | **Not started** | Approved domain + persistence/runtime needs | Backend implementation |
| DG-012 | **Not started** | Runtime architecture + release needs | Production readiness |

## M0 completion rule

M0 is complete when the minimum product/domain foundation is approved and verified, not when every future architecture question has been decided.

The intended M0 closure path is:

DG-001 + DG-013 → DG-002 → DG-003 → DG-004 → DG-005

DG-006 through DG-012 are architecture gates, not prerequisites for proving the core domain.

## Decision discipline

For every significant decision:

1. State the problem and constraints.
2. Identify realistic alternatives.
3. Record trade-offs and consequences.
4. Make the decision explicitly.
5. Record rejected alternatives when useful.
6. Define revisit conditions when legitimate.
7. Update the relevant gate and checkpoint.
8. Only then treat downstream work as unblocked.

## Architecture gating rule

Do not force all of DG-006–DG-012 through M0 upfront.

Open a gate when:
- the product requirement is concrete;
- the decision materially constrains implementation;
- relevant alternatives can be evaluated with evidence.

This keeps the project small while preserving engineering quality.

## Current working proposals

| Area | Working proposal | Status |
|---|---|---|
| Initial user | Individual | Open |
| MVP financial core | Accounts + income + expense + transfer + balance + history | Open |
| Domain shape | Operation + Effect, with Evidence/Proposal around it | Open |
| Currency | Single-currency MVP candidate | Open |
| Offline | Architecture-ready; full offline behavior only if justified by release requirement | Open |
| Correction | Mixed/explicit correction history candidate | Open |
| Portability | Export in MVP candidate | Open |

None of these are approved until the relevant decision gate is closed.