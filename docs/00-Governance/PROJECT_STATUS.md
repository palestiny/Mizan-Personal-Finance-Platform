# Project Status

## Current phase
**M1 — Thin Vertical Slice Implementation**

## Current state
**Status:** In progress  
**Production implementation:** RED-test suite established; minimal domain implementation is next.

## Accepted product and strategy gates
- **DG-001 Product Scope:** Accepted
- **DG-013 Product Strategy:** Accepted
- **DG-002 Domain Model:** Accepted
- **DG-003 Financial Invariants:** Accepted
- **DG-004 Transaction Model:** Accepted
- **DG-005 Balance Model:** Accepted
- **M1 Thin Vertical Slice:** Architecture decisions Accepted
- **M1 Runtime & Technology Foundation:** Accepted

The accepted direction is Mizan as a globally applicable Personal Financial Operating System, with Egypt/personal use as the initial validation context. The MVP is a complete personal-finance core, while advanced intelligence and automation remain staged capabilities.

## Current design state
| Area | State |
|---|---|
| Product vision | Established |
| Product strategy | Accepted strategic direction / hypotheses remain validation-driven |
| Product scope | Accepted |
| DG-013 strategy gate | Accepted |
| DG-002 domain model | **Accepted** |
| Financial invariants | **Accepted** |
| Transaction model | **Accepted** |
| Balance model | **Accepted** |
| M1 architecture | **Accepted** |
| Runtime technology | **Accepted** |
| Production structure | Established |
| RED tests | **Established** |
| Minimal domain implementation | Not started |

## Accepted domain foundation
**Operation + Effect** is the authoritative financial model.

Evidence and Proposal are surrounding concepts. Evidence is not financial truth. Proposal is not financial truth. Only accepted Operations create authoritative Effects. Accepted Operations and Effects are immutable. Corrections/Reversals are represented by new Operations and Effects. MVP operation types are Income, PersonalExpense, and OwnedAccountTransfer. MVP account classifications are Cash, Bank, and Wallet. Currency is explicit with a single-currency MVP.

## M1 implementation foundation
The repository now contains the accepted modular-monolith project boundaries:
- `src/Mizan.Domain`
- `src/Mizan.Application`
- `src/Mizan.Infrastructure`
- `src/Mizan.Api`
- `tests/Mizan.Domain.Tests`
- `tests/Mizan.Application.Tests`
- `tests/Mizan.Api.Tests`

The implementation target is .NET 8 for the first slice, preserving the accepted ASP.NET Core/.NET decision without introducing a new product architecture decision.

## RED-test coverage established
The RED suite specifies:
- exact minor-unit Money;
- Income and PersonalExpense effect direction;
- transfer two-effect conservation;
- transfer endpoint validation;
- accepted operation/effect immutability;
- effects-derived balance and rebuildability;
- application idempotency;
- atomic rejection/no partial effects;
- deterministic history and balance explanation;
- one API end-to-end financial path.

Issue #10 tracks the RED-test checkpoint.

## Verification status
The GitHub Actions workflow `.github/workflows/m1-tests.yml` was added to execute restore and test on the M1 branch. The GitHub connector has not yet reported a workflow run for the latest workflow commit, so no GREEN/RED runtime result is claimed here.

## Accepted controlled sequence
**M1 thin vertical slice implementation → trustworthy core → validated capture → understanding → intelligence → action → expansion**

## Important boundaries
- No provider-specific financial domain models.
- No generalized accounting engine.
- No household/shared foundation in MVP.
- No multi-currency implementation in MVP.
- No full offline synchronization implementation before the thin vertical slice demonstrates the need.
- No advanced autonomous financial actions without explicit authorization/policy/auditability.
- Pricing and packaging remain validation questions.

## Next action
Proceed with the **minimum domain implementation required to turn the established RED tests GREEN**, without adding architecture that the tests and accepted slice do not require.

## Verification rule
A document is not treated as approved merely because it exists. Gate status and decision records must reflect explicit Product Owner acceptance.
