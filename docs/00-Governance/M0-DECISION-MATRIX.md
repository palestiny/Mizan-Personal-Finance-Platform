# M0 — Decision Matrix

## Purpose

This matrix is the control surface for the M0 decision gates. It makes the dependency order explicit without silently converting proposals into approved decisions.

**Rule:** An item is not approved because a proposal or document exists. It becomes approved only after the Product Owner/Decision Owner explicitly accepts the decision and the corresponding gate/checkpoint is updated.

## Decision status

| Gate | Decision | Current state | Depends on | Blocks |
|---|---|---|---|---|
| DG-001 | Product Scope | **Open — Product Owner decision required** | Product direction | DG-002, DG-003, DG-004, DG-005 |
| DG-002 | Financial Domain Model | Not started | DG-001 | DG-003, DG-004, DG-005 |
| DG-003 | Financial Invariants | Not started | DG-002 | DG-004, DG-005 |
| DG-004 | Transaction Model | Not started | DG-003 | DG-005 and implementation |
| DG-005 | Balance Model | Not started | DG-004 | Persistence/offline implementation decisions |
| DG-006 | Offline-First Strategy | Not started | DG-005 + release requirement | DG-007, DG-008 |
| DG-007 | Persistence Architecture | Not started | Domain + balance + offline decisions | Implementation foundation |
| DG-008 | Synchronization | Not started | Persistence + offline strategy | Multi-device/server implementation |
| DG-009 | Mobile Technology | Not started | Product constraints + architecture constraints | Mobile implementation |
| DG-010 | Security & Privacy | Not started | Product/data boundaries | Production implementation |
| DG-011 | Backend Architecture | Not started | Domain + persistence + sync requirements | Backend implementation |
| DG-012 | Production & Observability | Not started | Architecture + operational requirements | Production readiness |

## DG-001 pending decisions

These are the decisions currently exposed by the Product Scope gate:

| ID | Decision | Current proposal | Status |
|---|---|---|---|
| D1 | Initial user boundary | Individual personal finance | Open |
| D2 | MVP financial scope | Complete personal-finance core | Open |
| D3 | Account scope | Accounts required for the core financial loop; exact domain representation deferred to DG-002 | Open |
| D4 | Currency scope | Not selected | Open |
| D5 | Offline requirement | Candidate capability; release requirement not yet selected | Open |
| D6 | History correction policy | Not selected | Open |
| D7 | Data portability | Not selected | Open |

## Decision discipline

For every significant decision:

1. State the problem and constraints.
2. Identify realistic alternatives.
3. Record relevant trade-offs and consequences.
4. Make the decision explicitly.
5. Record rejected alternatives when useful.
6. Define revisit conditions when the decision may legitimately change.
7. Update the relevant gate and checkpoint.
8. Only then treat downstream work as unblocked.

## Current execution rule

Until DG-001 is explicitly closed:

- Do not implement production financial domain code.
- Do not finalize the financial data model.
- Do not finalize transaction or balance semantics.
- Do not select a persistence/mobile/backend stack as an approved architecture.
- Preparatory analysis may continue only when it does not silently decide a blocked product/domain decision.

## Next controlled step

**Close DG-001.**

Once DG-001 is accepted, proceed to **DG-002 — Financial Domain Model**, using the accepted product boundary as an input rather than re-deciding it implicitly.
