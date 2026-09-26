# DG-021 — M3 Capture Provider Adapter Boundary

Status: Proposed — Product Owner decision required
Phase: M3 — Frictionless Capture
Date: 2026-09-26

## Decision required

Define the boundary for external capture providers (LLM, OCR, speech-to-text, and similar adapters) without weakening the financial authority boundary established by DG-018, DG-019, and DG-020.

The provider may improve interpretation quality, but it must never become a financial authority.

## Problem

DG-020 established:

Raw Capture → Capture Interpreter → Vendor-neutral Interpretation → Context Resolution + Validation → Proposal → User Confirmation → Existing Financial Command → Operation + Effect

The current implementation uses a deterministic interpreter. The next capability is to allow external providers to supply interpretation candidates while preserving:
- no provider-owned authoritative AccountIds;
- no provider-created Proposal confirmation;
- no provider-created FinancialOperation or FinancialEffect;
- deterministic application-owned context resolution;
- explicit missing and ambiguity handling;
- safe failure when provider output is unavailable, malformed, contradictory, or uncertain.

## Constraints

1. Domain remains unaware of AI/OCR/speech providers and their SDKs.
2. Proposal persistence remains non-authoritative.
3. Existing Financial Command remains the only authoritative execution/idempotency boundary.
4. Provider output is untrusted input and must be validated before entering the existing interpretation pipeline.
5. Semantic account references may cross the provider boundary; internal AccountIds may not.
6. A provider cannot resolve an account merely by naming or guessing one.
7. User confirmation remains mandatory for provider-derived financial proposals.
8. Provider failure must never create authoritative financial state.
9. The same vendor-neutral interpretation contract should support text, OCR, and voice-derived capture without coupling the financial core to media/provider details.

## Options

### Option A — Provider returns Proposal directly

Provider → Proposal → Confirmation → Financial Command

Advantages: small orchestration surface and a fast AI-first prototype.

Costs / risks: couples provider behavior to persistence/application workflow, encourages provider-specific financial semantics, weakens validation/failure boundaries, and makes future providers more likely to bypass DG-020.

Assessment: Reject for M3.

### Option B — Provider adapter returns validated vendor-neutral interpretation

Raw/normalized capture → Provider Adapter → validated CaptureInterpretation candidate → deterministic Context Resolution + Validation → Proposal → Confirmation → Financial Command

Advantages: preserves DG-020 unchanged, keeps providers replaceable, keeps validation/account resolution application-owned, supports deterministic and external interpreters, and permits fail-closed provider handling.

Costs: requires an adapter contract, explicit diagnostics/validation semantics, and provider-specific infrastructure implementations.

Assessment: Recommended.

### Option C — Provider returns only extracted fields; a separate interpreter assembles semantics

Provider → raw extracted fields → deterministic interpreter → CaptureInterpretation → resolution → Proposal

Advantages: strong extraction/interpretation separation and useful for OCR/speech pipelines.

Costs: more orchestration and intermediate contracts; semantic providers may need information discarded or reinterpreted; risks creating two competing interpretation layers too early.

Assessment: Future extension where a channel genuinely requires it; not the mandatory M3 abstraction.

## Recommended boundary

Adopt Option B.

Define an application-owned provider-neutral adapter contract conceptually equivalent to ICaptureProviderAdapter.

Its output is a provider-neutral interpretation candidate plus diagnostics, not a Proposal or financial command.

Provider-specific implementations belong outside Domain and outside Proposal persistence, in Infrastructure/integration code.

The adapter may receive raw capture or a channel-specific normalized representation, but it must not receive or return authoritative internal financial identifiers as part of the provider contract.

## Validation boundary

Provider output is untrusted.

Before it reaches Proposal materialization:
1. Validate structural schema.
2. Validate supported operation type.
3. Validate amount representation and range.
4. Validate currency representation.
5. Validate date/time representation.
6. Validate semantic account references as references only.
7. Reject contradictory fields.
8. Preserve missing required fields explicitly.
9. Preserve provider diagnostics/uncertainty without converting them into financial authority.
10. Pass only validated vendor-neutral semantics to the existing deterministic context resolver.

A provider response that cannot be validated is not a partial financial success. It is an unusable interpretation and must not create authoritative financial state.

## Confidence and uncertainty

Provider confidence is diagnostic metadata, not permission to execute.

Low confidence, conflicting evidence, missing fields, or unresolved references must remain visible to the application/user and must not be converted into an automatic confirmation decision.

No numeric confidence threshold is accepted as an authority shortcut in this gate.

## Provider failure policy

Timeout, network failure, provider rejection, rate limit, malformed structured output, schema mismatch, or unavailable provider must fail closed.

Required invariant:

Provider failure => no Proposal confirmation => no FinancialOperation => no FinancialEffect

A retry may repeat interpretation, but retrying a provider call must not bypass Proposal confirmation or the existing financial idempotency boundary.

## OCR and voice

OCR and voice should converge on the same vendor-neutral financial interpretation boundary.

Channel-specific work may happen before interpretation: voice may require transcription; OCR may require text/document extraction; image/receipt processing may require evidence handling.

Those concerns are adapters around capture; they must not introduce a second financial authority path.

The financial pipeline remains:

Channel adapter → vendor-neutral interpretation → deterministic resolution/validation → Proposal → confirmation → Financial Command.

## Anti-hallucination / trust boundary

The system must treat provider output as untrusted claims.

Providers may propose: amount, currency, operation type, semantic account reference, destination reference, effective time, and missing/ambiguous information.

Providers may not assert: authoritative AccountId, authoritative OperationId, authoritative Effect, confirmed Proposal, or permission to execute.

Application-owned validation and resolution remain the source of authority for internal identity and execution.

## Non-goals

This gate does not select an AI vendor, model, prompts, agent frameworks, RAG, autonomous financial execution, automatic confirmation, confidence thresholds, account ranking, production OCR/voice vendor selection, or generalized workflow orchestration.

## Acceptance criteria

The gate is accepted only if the Product Owner explicitly approves:
1. Option B as the provider boundary.
2. Provider adapters returning vendor-neutral interpretation candidates, not Proposals or commands.
3. Provider-specific SDKs isolated from Domain and Proposal persistence.
4. Strict structural/domain validation before Proposal materialization.
5. Semantic account references only across the provider boundary.
6. Deterministic application-owned account resolution.
7. Provider failure and malformed output fail closed.
8. Confidence remains diagnostic and cannot authorize execution.
9. OCR/voice converge on the same financial interpretation boundary.
10. Existing Proposal and Financial Command boundaries remain unchanged.

## Implementation sequence after acceptance

1. Add provider-neutral adapter contract.
2. Add validation/result diagnostics contract.
3. Add deterministic fake/provider adapter for TDD.
4. Prove valid provider output creates only a Proposal.
5. Prove unknown/ambiguous references remain unresolved.
6. Prove malformed/invalid output cannot produce a confirmable Proposal.
7. Prove provider failure creates no authoritative financial state.
8. Prove provider output cannot inject internal AccountIds.
9. Add one integration adapter only after the boundary is GREEN.
10. Run full CI and record a new M3 checkpoint.

## Open questions for Product Owner

- Approve Option B?
- Should provider diagnostics be persisted with the Proposal, or only retained as transient interpretation metadata in the first implementation?
- Should provider failure create a persisted Draft/Failed Proposal, or return a capture error before Proposal creation?
- Is a provider-neutral adapter contract sufficient for the first OCR/voice slices, or should media preprocessing have a separate explicit gate later?