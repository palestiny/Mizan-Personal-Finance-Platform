# DG-020 — M3 Capture Interpretation Boundary

Status: Accepted — Product Owner decision
Phase: M3 — Frictionless Capture
Date: 2026-09-25

## Decision

**Option B — Vendor-neutral interpretation plus deterministic context resolution.**

Accepted flow:

Raw Capture → Capture Interpreter → Vendor-neutral Interpretation → Context Resolution + Validation → Proposal → User Confirmation → Existing Financial Command → Operation + Effect

## Accepted boundary

- `ICaptureInterpreter` converts raw capture into vendor-neutral semantics.
- `CaptureInterpretation` contains semantic values, references, missing fields, and ambiguities; it does not contain authoritative internal AccountIds.
- Application-owned context resolution maps semantic references to internal account identifiers deterministically.
- Unknown or ambiguous references remain unresolved and cannot silently select an account.
- The existing Proposal lifecycle remains the non-authoritative review boundary.
- Existing Financial Command remains the authoritative execution/idempotency boundary.
- No AI provider or SDK is required by the Domain or Proposal persistence.

## First-slice implementation

Implemented with a deterministic interpreter and resolver:
- text capture;
- amount/currency extraction;
- operation classification;
- semantic account references;
- missing/ambiguous account handling;
- Proposal materialization;
- tests proving unresolved/ambiguous input remains Draft.

## Verification

PR #32 implemented the boundary and was squash-merged into `main` after CI verification.
PR #33 corrected the API Proposal status contract and date-sensitive API test data; CI run #194 passed and the PR was squash-merged.

## Non-goals

AI vendor selection, prompt engineering, OCR/voice implementation, autonomous confirmation, new financial semantics, generalized agents/workflows/RAG, and account ranking remain deferred.

## Revisit conditions

Reopen DG-020 only if a new capture channel or product requirement proves the current semantic interpretation/resolution contract insufficient.
