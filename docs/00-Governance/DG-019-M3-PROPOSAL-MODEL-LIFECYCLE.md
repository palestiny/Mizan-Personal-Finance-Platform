# DG-019 — M3 Proposal Model & Lifecycle

Status: Proposed — awaiting Product Owner decision
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

## Proposed model
A Proposal is a short-lived capture artifact containing:
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

The financial domain should not depend on an LLM type, prompt, provider, or model-specific object.

## Proposed lifecycle

Draft → ReadyForConfirmation → Confirmed

Terminal non-success states:
- Rejected
- Expired
- Failed

Rules:
- Only ReadyForConfirmation may be confirmed.
- A proposal with unresolved required fields cannot become ReadyForConfirmation.
- Confirmation is one-time; repeated confirmation must be idempotent or safely rejected according to the approved command semantics.
- Rejected/Expired/Failed proposals cannot create financial state.
- Confirmed means the proposal was accepted for command execution; authoritative financial success still depends on normal command/domain validation and transaction rules.
- A failed financial command must not be represented as successful financial acceptance merely because the Proposal was confirmed.

## Confirmation semantics
The critical design choice is whether confirmation itself is the idempotency boundary.

### Option A — Proposal owns confirmation idempotency
The Proposal stores a stable confirmation/execution identity. Repeating confirmation of the same Proposal resolves to the same financial command result when semantics match.
Advantages: natural retry behavior and strong UX semantics.
Trade-offs: Proposal persistence becomes coupled to command execution outcomes and needs careful atomic coordination.

### Option B — Confirmation creates/reuses a command idempotency key
The client/application derives or supplies a stable idempotency key for the authoritative financial command. Proposal state records the relationship but the financial command remains the source of execution idempotency.
Advantages: preserves the existing command idempotency boundary and avoids making Proposal a second execution ledger.
Trade-offs: requires explicit mapping between Proposal confirmation and command idempotency.

Working proposal: Option B.

## Expiration
A Proposal should have an explicit expiration boundary so stale interpretations do not remain confirmable indefinitely. Expiration is a capture concern and does not mutate financial state.

The exact default TTL should be determined from UX needs and implementation constraints rather than hard-coded into the financial domain.

## Concurrency
Concurrent confirmation of the same Proposal must not produce duplicate financial Operations. The design must guarantee that only one authoritative command execution can succeed for a given confirmation identity, while a retry observes the existing result or a defined conflict.

## Persistence boundary
The Proposal belongs to the Application/Capture side of the modular monolith. The Domain financial model should not reference Proposal entities. A persistence adapter may store proposals separately from authoritative financial tables.

## Alternatives considered
### Keep Proposal entirely in memory
Smallest initial implementation, but weak for refresh/retry/concurrent confirmation and poor observability.

### Persist Proposal as a first-class capture artifact
Adds storage and lifecycle complexity, but provides inspectability, retries, expiration, auditability of interpretation, and safe confirmation semantics.

Working proposal: persist Proposal state, while keeping it explicitly outside authoritative financial state.

## First-slice non-goals
- Training or hosting an AI model.
- Vendor-specific AI abstractions in the financial Domain.
- Autonomous confirmation.
- OCR, voice, bank feeds, or notification ingestion.
- Editing accepted Operations through Proposal mutation.
- General workflow/agent orchestration.
- Generalized accounting semantics.

## Decision required
Approve or revise:
1. Proposal as a persisted non-authoritative capture artifact.
2. The proposed lifecycle and terminal states.
3. Option B for confirmation idempotency: command idempotency remains authoritative, with Proposal storing the confirmation-to-command relationship.
4. Explicit expiration without a domain-hard-coded TTL.
5. Concurrent confirmation must not create duplicate authoritative Operations.

If accepted, implementation can proceed through a small vendor-neutral Proposal service/API and tests, followed by a separate capture interpreter adapter.