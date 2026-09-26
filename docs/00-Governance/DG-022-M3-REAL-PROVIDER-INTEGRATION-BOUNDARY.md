# DG-022 — Real Capture Provider Integration Boundary

**Status:** Accepted — Product Owner decision
**Date:** 2026-09-26
**Phase:** M3 — Frictionless Capture
**Decision:** Option B — Infrastructure adapter behind the provider-neutral contract

## Assessment
Accepted. A real external provider may be introduced only through the existing provider-neutral capture boundary. Provider-specific SDKs and operational concerns remain in Infrastructure.

## Binding decisions
- Provider output is untrusted.
- No authoritative AccountId, OperationId, Effect, Proposal confirmation, or Financial Command may originate from a provider.
- Provider-specific SDKs do not cross into Domain or Proposal persistence.
- Application validates provider output before deterministic context resolution.
- Invalid, malformed, contradictory, timed-out, unavailable, rate-limited, or otherwise failed provider calls fail closed before Proposal creation.
- Retries are bounded and limited to transient failures.
- Provider credentials remain in infrastructure configuration/secrets.
- Raw financial capture and raw provider payloads are not persisted/logged by default.
- Confidence remains diagnostic only.
- Automatic multi-provider fallback is deferred.
- Vendor selection is not decided by this gate and requires evidence-based evaluation.
- Existing Proposal and Financial Command boundaries remain unchanged.

## Implementation direction
Implement a real provider adapter in Infrastructure behind ICaptureProviderAdapter, with provider-specific request/response mapping, timeout/cancellation, bounded transient retry, stable failure mapping, redaction-safe diagnostics, and integration tests. Use a deterministic fake for application tests.

## Non-goals
Vendor selection, prompt engineering, autonomous confirmation, financial decision-making by AI, multi-provider routing/fallback, agent/RAG orchestration, and new financial semantics.

## Acceptance criteria
A real provider can be added without changing Domain financial semantics; provider failures and invalid outputs cannot create Proposals; retries cannot create financial effects; provider-specific SDK types remain isolated; sensitive data has explicit logging boundaries; existing deterministic tests remain valid.
