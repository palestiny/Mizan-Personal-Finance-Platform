# Project Status

## Current phase

**M0 — Product & Domain Foundation**

## Current state

**Status:** In progress  
**Production implementation:** Not started

The project is aligning its product vision, commercial hypotheses, financial domain model, invariants, and delivery roadmap before implementation.

## Vision alignment review

The repository has been reviewed against the new direction:

- Personal Financial Operating System, not expense tracker
- Global by design; Egypt is validation context, not product boundary
- Commercially viable service with recurring value as a hypothesis to validate
- AI-native, but AI is not financial truth
- Evidence/proposals separated from authoritative financial state
- Economic meaning can exceed simple income/expense/transfer
- Trust, correctness, privacy, portability, and user control remain foundational

The current product vision and strategy are aligned with this direction.

## Current design state

| Area | State |
|---|---|
| Product vision | Established |
| Product strategy | Established as hypotheses |
| Product scope | Open |
| Product strategy gate (DG-013) | Open |
| Domain model | Draft candidate |
| Financial invariants | Draft candidate |
| Transaction model | Draft candidate |
| Balance model | Not started |
| Production code | Not started |

## Important consistency findings

1. The earlier simple Financial Record model was too narrow for the new vision; the candidate model is now Operation + Effect, with Evidence/Proposal around it.
2. The new model does not require implementing recoverables, shared expenses, forecasting, automation, or integrations in the MVP.
3. The original roadmap attempted to resolve too many architecture decisions before a vertical slice existed. It has been right-sized so architecture gates are opened just in time.
4. M0 now ends at the minimum trustworthy financial model rather than attempting to settle the entire production architecture.

## Current controlled sequence

DG-001 + DG-013
→ DG-002
→ DG-003
→ DG-004
→ DG-005
→ thin vertical slice
→ architecture decisions only where the slice requires them
→ trustworthy core
→ validated capture
→ understanding
→ intelligence
→ action
→ expansion

## Not yet approved

- Final MVP scope
- Final financial domain model
- Final transaction/effect semantics
- Balance semantics
- Currency/precision policy
- Offline release requirement
- Persistence technology
- Synchronization strategy
- Mobile/backend architecture
- Security/privacy implementation details
- Monetization/packaging/pricing

## Next action

Complete the remaining review/reconciliation of the M0 candidate documents, then proceed through the open design gates without implementing production financial code before approval.

## Verification rule

A document existing in GitHub does not mean the decision is approved. Completion requires the corresponding gate decision, evidence, and checkpoint.
