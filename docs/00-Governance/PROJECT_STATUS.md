# Project Status

## Current phase
**M0 — Product & Domain Foundation**

## Current state
**Status:** In progress  
**Production implementation:** Not started

## Accepted product and strategy gates
- **DG-001 Product Scope:** Accepted
- **DG-013 Product Strategy:** Accepted
- **DG-002 Domain Model:** Accepted
- **DG-003 Financial Invariants:** Accepted
- **DG-004 Transaction Model:** Accepted
- **DG-005 Balance Model:** Accepted

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
| Production code | Not started |

## Accepted domain foundation
**Operation + Effect** is the authoritative financial model.

Evidence and Proposal are surrounding concepts. Evidence is not financial truth. Proposal is not financial truth. Only accepted Operations create authoritative Effects. Accepted Operations and Effects are immutable. Corrections/Reversals are represented by new Operations and Effects. MVP operation types are Income, PersonalExpense, and OwnedAccountTransfer. MVP account classifications are Cash, Bank, and Wallet. Currency is explicit with a single-currency MVP.

## Accepted controlled sequence
**Thin vertical slice → architecture decisions only where the slice requires them → trustworthy core → validated capture → understanding → intelligence → action → expansion**

## Important boundaries
- No provider-specific financial domain models.
- No generalized accounting engine.
- No household/shared foundation in MVP.
- No multi-currency implementation in MVP.
- No full offline synchronization implementation before the thin vertical slice demonstrates the need.
- No advanced autonomous financial actions without explicit authorization/policy/auditability.
- Pricing and packaging remain validation questions.

## Next action
Proceed to the **thin vertical slice**. M0 domain foundation gates DG-001, DG-002, DG-003, DG-004, and DG-005 are accepted. No production financial implementation should begin outside the controlled vertical-slice plan.

## Verification rule
A document is not treated as approved merely because it exists. Gate status and decision records must reflect explicit Product Owner acceptance.
