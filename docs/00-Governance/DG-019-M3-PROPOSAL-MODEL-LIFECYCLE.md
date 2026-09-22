# DG-019 — M3 Proposal Model & Lifecycle

Status: Accepted — Option B: persisted Proposal with command-owned confirmation idempotency
Phase: M3 — Frictionless Capture
Date: 2026-09-22

## Problem
DG-018 accepted Proposal-first confirmation for natural-language capture. Before runtime implementation, Mizan needs a precise boundary for the non-authoritative Proposal so that interpretation can be inspected, corrected, confirmed, rejected, expired, and safely converted into an existing financial command.

## Constraints
1. A Proposal is never authoritative financial state.
2. Confirming a Proposal must invoke an existing authoritative financial command; Proposal confirmation must not create Effects directly.
3. A Proposal must preserve enough information to explain what the capture layer understood.
4. Missing and ambiguous required fields must be explicit rather than guessed into financial state.
5. A Proposal must be independently testable without a specific AI/LLM vendor.
6. Existing account lifecycle, currency, idempotency, atomicity, reversal, and immutable Operation/Effect rules remain unchanged.
7. A Proposal must not become a second financial ledger or a mutable copy of an accepted Operation.

## Accepted model
A Proposal is a persisted, short-lived capture artifact containing:
- ProposalId
- Original user input
- Proposed operation type
- Proposed operation fields needed by the target command
- Explicit missing fields
- Explicit ambiguities/questions
- Interpretation metadata sufficient for diagnostics, without making model/provider details part of the financial domain
- Status
- CreatedAt / ExpiresAt
- Confirmation/rejection timestamps when applicable
- The confirmation-to-command idempotency relationship when confirmed

The financial domain does not depend on an LLM type, prompt, provider, or model-specific object.

## Accepted lifecycle

Draft → ReadyForConfirmation → Confirmed

Terminal non-success states:
- Rejected
- Expired
- Failed

Rules:
- Only ReadyForConfirmation may be confirmed.
- A proposal with unresolved required fields cannot become ReadyForConfirmation.
- Confirmation is one-time; retries use the approved command idempotency semantics.
- Rejected/Expired/Failed proposals cannot create financial state.
- Confirmed means the proposal was accepted for command execution; it does not by itself mean the financial operation succeeded.
- A failed financial command must not be represented as successful financial acceptance merely because the Proposal was confirmed.

## Accepted confirmation semantics
**Option B — Confirmation creates/reuses a command idempotency key.**

The client/application derives or supplies a stable idempotency key for the authoritative financial command. Proposal state records the relationship, while the existing financial command remains the source of execution idempotency.

This preserves the existing idempotency boundary and avoids making Proposal a second execution ledger. The explicit trade-off is that Proposal confirmation must maintain a clear mapping to the command idempotency identity.

## Expiration
A Proposal has an explicit expiration boundary so stale interpretations do not remain confirmable indefinitely. Expiration is a capture concern and does not mutate financial state.

The exact default TTL is intentionally not hard-coded into the financial Domain; it will be determined by UX and implementation constraints.

## Concurrency
Concurrent confirmation of the same Proposal must not produce duplicate financial Operations. Only one authoritative command execution can succeed for a given confirmation identity; retries must observe the existing result or a defined conflict.

## Persistence boundary
The Proposal belongs to the Application/Capture side of the modular monolith. The financial Domain does not reference Proposal entities. Proposal persistence is separate from authoritative financial tables.

## Decision
**Product Owner decision: Accepted.**

The following are approved:
1. Proposal as a persisted non-authoritative capture artifact.
2. The proposed lifecycle and terminal states.
3. Option B: command idempotency remains authoritative, with Proposal storing the confirmation-to-command relationship.
4. Explicit expiration without a domain-hard-coded TTL.
5. Concurrent confirmation must not create duplicate authoritative Operations.

### Rejected/deferred alternatives
- Proposal-owned confirmation idempotency (Option A) is rejected for the first implementation because it would couple Proposal persistence more tightly to financial execution outcomes.
- In-memory-only Proposal storage is rejected because refresh/retry/concurrency/observability requirements justify persistence.
- Exact expiration TTL is deferred until UX/implementation evidence exists.

## First-slice non-goals
- Training or hosting an AI model.
- Vendor-specific AI abstractions in the financial Domain.
- Autonomous confirmation.
- OCR, voice, bank feeds, or notification ingestion.
- Editing accepted Operations through Proposal mutation.
- General workflow/agent orchestration.
- Generalized accounting semantics.

## Implementation boundary
Implementation may proceed through a small vendor-neutral Proposal service/API and tests, followed by a separate capture interpreter adapter.

The first runtime slice must prove the Proposal lifecycle and safe confirmation boundary without requiring a specific AI provider.
