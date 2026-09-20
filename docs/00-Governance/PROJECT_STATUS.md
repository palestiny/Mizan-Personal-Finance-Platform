# Project Status

## Current phase
**M2 — Trustworthy Financial Core**

## Current state
**Status: M2 explicit reversal implementation verified in GitHub CI; the next M2 capability is not yet opened behind a design gate.**

**Production implementation:** domain foundation + EF Core/PostgreSQL persistence boundary + thin API path + explicit reversal capability are implemented and runtime-verified.

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
| M1 thin vertical slice | **Implemented and verified** |
| M2 explicit reversal | **Implemented and runtime-verified** |
| M2 account lifecycle | **Implemented; runtime verification pending CI** |

## Accepted domain foundation
**Operation + Effect** is the authoritative financial model.

Evidence and Proposal are surrounding concepts. Evidence is not financial truth. Proposal is not financial truth. Only accepted Operations create authoritative Effects. Accepted Operations and Effects are immutable. Corrections/Reversals are represented by new Operations and Effects. MVP operation types are Income, PersonalExpense, and OwnedAccountTransfer. Account classifications are Cash, Bank, and Wallet. Currency is explicit with a single-currency MVP.

## M1 implementation foundation
The repository contains the accepted modular-monolith project boundaries:
- `src/Mizan.Domain`
- `src/Mizan.Application`
- `src/Mizan.Infrastructure`
- `src/Mizan.Api`
- `tests/Mizan.Domain.Tests`
- `tests/Mizan.Application.Tests`
- `tests/Mizan.Api.Tests`

The implementation target is .NET 8. EF Core + Npgsql persistence is wired behind the accepted persistence boundary.

## Verification status
The GitHub Actions workflow `.github/workflows/m1-tests.yml` executes restore, build, committed EF migrations against PostgreSQL, and the verification suite. The latest documented verification confirms the explicit reversal slice is GREEN.

## Verified M2 reversal capability
- First-class immutable Reversal Operation linked by `OriginalOperationId`
- Exact inverse Effects with preserved effect ordering
- Original Operation/Effects remain immutable
- Reversal cannot target another Reversal
- One Reversal per original Operation enforced by PostgreSQL uniqueness
- Idempotent retry and conflicting-key behavior
- API and PostgreSQL migration coverage
- Domain, Application, and API verification
- Concurrent duplicate protection
- Latest documented verification: GitHub Actions run #128

## Verified M2 account lifecycle capability
- Account lifecycle uses Active / Closed states.
- New accounts are Active.
- Close and Reopen are explicit application commands.
- Normal Income, PersonalExpense, and OwnedAccountTransfer operations are blocked on Closed accounts.
- Balance/history/explanation remain readable for Closed accounts.
- Reversal remains allowed against accepted historical operations after account closure.
- Lifecycle status is persisted through an explicit EF migration.

## Important boundaries
- No provider-specific financial domain models.
- No generalized accounting engine.
- No household/shared foundation in MVP.
- No multi-currency implementation in MVP.
- No full offline synchronization implementation before the thin vertical slice demonstrates the need.
- No advanced autonomous financial actions without explicit authorization/policy/auditability.
- Pricing and packaging remain validation questions.
- No separate Correction primitive is introduced at this stage; a future correction workflow may compose Reversal + Replacement at the application level.

## Next action
Verify the Account Lifecycle implementation through GitHub CI. If GREEN, record CP-013 as completed and continue M2 only when the next concrete capability requires a design gate. Do not open another M2 architecture/design gate speculatively. Identify the next concrete M2 capability from the accepted roadmap, and open a design gate only if that capability introduces a material product, domain, or architecture decision. Then follow the normal Understand → Map → Design → Trade-offs → Decide → Document → Test → Implement → Review → Verify flow.

## Verification rule
A document is not treated as approved merely because it exists. Gate status and decision records must reflect explicit Product Owner acceptance.
