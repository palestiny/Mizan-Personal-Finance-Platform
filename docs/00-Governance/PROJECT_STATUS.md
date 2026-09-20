# Project Status

## Current phase
**M0 — Product & Domain Foundation**

## Current state
**Status:** In progress  
**Production implementation:** Not started

## Accepted product and strategy gates
- **DG-001 Product Scope:** Accepted
- **DG-013 Product Strategy:** Accepted

The accepted direction is Mizan as a globally applicable Personal Financial Operating System, with Egypt/personal use as the initial validation context. The MVP is a complete personal-finance core, while advanced intelligence and automation remain staged capabilities.

## Current design state

| Area | State |
|---|---|
| Product vision | Established |
| Product strategy | **Accepted strategic direction / hypotheses remain validation-driven** |
| Product scope | **Accepted** |
| DG-013 strategy gate | **Accepted** |
| Domain model | Draft candidate |
| Financial invariants | Draft candidate |
| Transaction model | Draft candidate |
| Balance model | Not started |
| Production code | Not started |

## Accepted controlled sequence
**DG-002 → DG-003 → DG-004 → DG-005 → thin vertical slice → architecture decisions only where the slice requires them → trustworthy core → validated capture → understanding → intelligence → action → expansion**

## Important boundaries
- No provider-specific financial domain models.
- No generalized accounting engine.
- No household/shared foundation in MVP.
- No multi-currency implementation in MVP.
- No full offline synchronization implementation before the thin vertical slice demonstrates the need.
- No advanced autonomous financial actions without explicit authorization/policy/auditability.
- Pricing and packaging remain validation questions.

## Next action
Proceed to **DG-002 Domain Model Decision Gate**. No production financial implementation should begin until the required M0 domain gates are accepted.

## Verification rule
A document is not treated as approved merely because it exists. Gate status and decision records must reflect explicit Product Owner acceptance.
