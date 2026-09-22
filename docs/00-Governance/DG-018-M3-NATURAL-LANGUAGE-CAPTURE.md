# DG-018 — M3 Natural-Language Capture Design Gate

Status: Proposed — awaiting Product Owner decision
Phase: M3 — Frictionless Capture
Date: 2026-09-22

## Problem

Mizan's M2 financial core is trustworthy, but entering a valid financial operation still requires structured input. M3 should reduce capture effort without weakening the authoritative financial model.

The first proposed capture capability is natural-language entry, for example:
- دفع 250 جنيه بنزين من البنك
- استلمت 8000 جنيه مرتب في البنك
- حولت 1000 من البنك للمحفظة

The capture layer must not directly create authoritative financial state from an interpretation.

## Constraints
1. Accepted financial state remains Operation + Effect.
2. Only validated and accepted Operations create authoritative Effects.
3. Natural language is ambiguous and may omit required information.
4. AI/interpretation must remain replaceable and non-authoritative.
5. Existing idempotency, atomicity, account lifecycle, currency, and reversal invariants must remain intact.
6. The design should not prematurely introduce a generalized AI platform, workflow engine, or offline synchronization model.

## Proposed boundary

User input → Capture interpretation → Proposal → User confirmation → Existing financial command → Operation + Effect

The proposal is non-authoritative until confirmation and normal domain/application validation succeed.

## Alternatives

### Option A — Direct interpretation into financial command
Flow: Input → parser/LLM → financial command → acceptance.
Advantages: lowest apparent latency and smallest UI/API flow.
Trade-offs: blurs interpretation and financial authority; bad interpretation can reach the financial command without explicit review; harder to explain what the system understood.
Assessment: not preferred for the first M3 slice because it weakens the trust boundary established in M2.

### Option B — Proposal-first confirmation
Flow: Input → interpretation → Proposal → user confirms → existing financial command → acceptance.
Advantages: preserves M2 as the single financial authority; makes ambiguity visible; allows AI/parser replacement; provides a place for confidence, missing fields, and user corrections.
Trade-offs: adds confirmation; requires a proposal lifecycle or equivalent short-lived representation; requires a clear contract between interpreted data and existing financial commands.
Assessment: recommended working proposal.

### Option C — Structured capture only, natural language later
Flow: improve structured entry first; defer natural-language interpretation.
Advantages: smallest immediate engineering scope and no interpretation uncertainty.
Trade-offs: delays the highest-friction reduction proposed by M3 and gives less evidence about natural-language value.
Assessment: viable if product validation prioritizes deterministic structured capture.

## Decision required

The Product Owner should choose A, B, or C. If B is selected, the next gate must define the Proposal model and lifecycle before implementation.

## Proposed M3 first-slice exit criteria
1. User input can be submitted without directly mutating financial state.
2. The system produces an inspectable proposal.
3. Missing or ambiguous required fields are explicit.
4. User confirmation invokes existing authoritative financial commands.
5. Rejected or expired proposals create no financial Effects.
6. Existing financial invariants and idempotency remain unchanged.
7. AI/parser failures cannot create financial state.
8. The capability is testable without requiring a specific AI vendor.

## Explicit non-goals
- Receipt OCR/image extraction.
- Voice capture.
- Bank/notification ingestion.
- Autonomous acceptance.
- Automatic financial actions.
- General-purpose agent/workflow engine.
- New accounting semantics.

## Revisit conditions
Reopen this gate if evidence shows confirmation creates unacceptable friction, ambiguity rates are high, or structured capture materially outperforms natural-language capture for the target workflow.