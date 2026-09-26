# DG-022 — Real Capture Provider Integration Boundary

**Status:** Proposed — awaiting Product Owner decision
**Date:** 2026-09-26
**Phase:** M3 — Frictionless Capture

## Problem
Mizan now has a provider-neutral capture adapter boundary, deterministic context resolution, and Proposal-first capture. The next step is allowing a real external AI/OCR/voice provider without making the provider a financial authority or coupling financial core code to a vendor SDK.

## Options

### Option A — Direct provider integration in Application
Application services call a vendor SDK directly.

Pros: lowest initial effort.
Cons: vendor SDK leaks into application orchestration; replacement and testing become harder; operational/privacy concerns mix with financial logic.

### Option B — Infrastructure adapter behind the existing provider-neutral contract — Recommended
Application ICaptureProviderAdapter <- Infrastructure RealProviderAdapter -> External Provider.

The Application sees only ICaptureProviderAdapter and CaptureProviderResult. Provider SDKs, HTTP clients, authentication, serialization, retries, timeouts, and provider-specific error mapping remain in Infrastructure.

Pros: preserves DG-021 trust boundary, localizes provider replacement, enables deterministic fakes, keeps vendor data out of Domain/Proposal persistence, and makes operational controls explicit.
Cons: more initial structure and integration testing.

### Option C — Multi-provider abstraction/fallback from day one
Build routing and automatic fallback across multiple vendors immediately.

Pros: potential resilience and vendor flexibility.
Cons: adds routing, consistency, cost, observability, and privacy complexity before evidence requires it.

## Recommendation
Adopt Option B. Do not implement automatic multi-provider fallback yet.

## Integration invariants
1. External provider output is untrusted.
2. Provider cannot return or assign authoritative internal AccountId, OperationId, or Effect.
3. Provider cannot confirm a Proposal or execute a Financial Command.
4. Provider SDKs and credentials stay outside Domain and Proposal persistence.
5. Application validates provider output before deterministic context resolution.
6. Provider-specific schemas map into CaptureInterpretation only through the adapter.
7. Invalid/malformed/contradictory output fails closed.
8. Timeout, cancellation, rate limit, authentication failure, outage, and schema mismatch fail closed before Proposal creation.
9. Retries are bounded and cannot create financial side effects.
10. Logs must not expose raw financial captures or provider credentials by default.
11. Provider response payloads are not persisted as authoritative financial data.
12. Confidence is diagnostic only.
13. Existing Proposal and Financial Command boundaries remain unchanged.

## Operational defaults
- Explicit timeout per provider call.
- No unbounded retry.
- Retry only transient failures, with a small bounded policy.
- Do not retry validation/schema failures.
- Cancellation propagates.
- Credentials come from infrastructure configuration/secrets.
- Provider-specific errors map to stable application-level capture failure categories.
- Raw provider payload retention is deferred to a separate privacy/retention gate.
- Observability records provider/channel, latency, outcome category, and correlation metadata without raw capture content by default.

## Vendor selection criteria
Vendor choice is deferred. When evaluated, compare structured-output reliability, Arabic/English quality, OCR/voice suitability, API/SDK stability, latency, availability, pricing, data retention/training controls, privacy/security posture, rate limits, and replacement ease.

## Non-goals
- Selecting a vendor in this gate.
- Prompt engineering.
- Autonomous confirmation.
- Financial decision-making by AI.
- Multi-provider routing/fallback.
- Agent/RAG/workflow orchestration.
- Persisting raw provider payloads.
- New financial semantics.

## Acceptance criteria
- A real provider can be added without changing Domain financial semantics.
- Provider-specific SDK types do not cross into Domain or Proposal persistence.
- Provider failure cannot create a Proposal.
- Invalid provider output cannot create a confirmable Proposal.
- Provider retries cannot create financial effects.
- Logs and diagnostics have explicit redaction boundaries.
- Existing deterministic provider tests remain valid.
