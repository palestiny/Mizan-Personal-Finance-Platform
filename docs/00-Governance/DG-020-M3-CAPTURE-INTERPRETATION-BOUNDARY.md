# DG-020 — M3 Capture Interpretation Boundary

Status: Proposed — awaiting Product Owner decision
Phase: M3 — Frictionless Capture
Date: 2026-09-25

## Problem

DG-018 and DG-019 establish Proposal-first capture:

User input → interpretation → Proposal → confirmation → existing financial command → Operation + Effect

The Proposal runtime slice is now verified. The next decision is the contract between raw capture input and the non-authoritative Proposal.

The boundary must allow natural-language interpretation to evolve or be replaced without moving financial authority into an AI/parser implementation.

## Constraints

1. A parser/interpreter must never create authoritative financial state.
2. The existing Proposal lifecycle and Financial Command remain authoritative boundaries.
3. Interpretation must be vendor-neutral and independently testable.
4. The interpreter must not depend on internal database identifiers that a language model may not know.
5. Ambiguity and missing information must remain explicit.
6. Contextual resolution (for example mapping "البنك" to an AccountId) must be deterministic and validated before a Proposal becomes confirmable.
7. Existing financial invariants, idempotency, atomicity, currency, account lifecycle, and reversal semantics remain unchanged.
8. The first slice must not introduce a generalized agent, workflow engine, RAG platform, or vendor SDK into the financial core.

## Proposed boundary

Recommended flow:

Raw Capture
→ Capture Interpreter
→ Vendor-neutral Interpretation
→ Context Resolution + Validation
→ Proposal
→ User Confirmation
→ Existing Financial Command
→ Operation + Effect

### Raw Capture

Contains user-provided evidence such as:
- original text;
- capture channel;
- optional locale/time-zone context supplied by the application.

Raw capture is evidence, not financial state.

### Capture Interpreter

Converts the raw input into a vendor-neutral semantic interpretation.

The interpreter may be:
- deterministic rules for the first test slice;
- an LLM adapter;
- another parser;
- a future OCR/voice adapter.

The financial Domain must not know which implementation produced the interpretation.

### Vendor-neutral Interpretation

The interpretation should describe intent and semantic values, not authoritative identifiers.

Examples:
- operation type: expense/income/owned-account-transfer;
- amount + currency;
- effective date/time if explicitly understood;
- account references such as "bank" or "wallet";
- unresolved fields;
- ambiguities/questions;
- diagnostic metadata.

Internal AccountId values should be resolved by application-owned context/resolution logic, not invented by the interpreter.

### Context Resolution + Validation

The application resolves semantic references against current user/application context and validates the candidate against the existing financial rules.

Examples:
- "البنك" → a unique active account owned by the user;
- "المحفظة" → a unique active destination account;
- missing account → explicit missing field;
- multiple matching accounts → explicit ambiguity.

No resolution result is authoritative financial state.

### Proposal

The resolved, validated interpretation is materialized into the existing persisted Proposal model.

The Proposal remains non-authoritative and follows DG-019.

## Alternatives

### Option A — Interpreter creates Proposal directly

Flow:
Raw Input → Interpreter → Proposal

Advantages:
- smallest apparent implementation;
- fewer application types.

Trade-offs:
- mixes semantic interpretation with application context resolution;
- encourages provider-specific assumptions inside Proposal creation;
- makes account/reference resolution harder to test independently.

Assessment: not preferred.

### Option B — Vendor-neutral interpretation + deterministic context resolution

Flow:
Raw Input → Interpreter → Interpretation → Context Resolution/Validation → Proposal

Advantages:
- keeps AI/parser replaceable;
- separates language understanding from application context;
- internal IDs remain application-owned;
- ambiguity and missing data have explicit representations;
- deterministic resolution can be tested without an AI provider;
- supports future text, OCR, and voice adapters without changing financial authority.

Trade-offs:
- introduces an additional boundary and a small amount of mapping code;
- requires explicit contracts for semantic references and resolution outcomes.

Assessment: recommended.

### Option C — Interpreter maps directly to Financial Command

Flow:
Raw Input → Interpreter → Financial Command

Advantages:
- shortest runtime path.

Trade-offs:
- violates the Proposal-first trust boundary;
- couples interpretation to financial execution;
- weakens reviewability and explainability.

Assessment: rejected by DG-018.

## Proposed decision

**Product Owner decision required: Option B — Vendor-neutral interpretation plus deterministic context resolution.**

If accepted, the implementation boundary should be:

- `ICaptureInterpreter`: converts raw capture into vendor-neutral interpretation;
- `CaptureInterpretation`: semantic candidate, unresolved fields, ambiguities, and diagnostics;
- application-owned context resolver/validator: resolves semantic references and validates them;
- existing `ProposalService`: persists the resulting Proposal;
- existing Financial Command path remains unchanged.

No AI SDK or provider-specific type should cross into the Domain or become required by Proposal persistence.

## First-slice acceptance criteria

1. A raw text input can be interpreted through a vendor-neutral contract.
2. The interpretation can represent an amount/currency, operation type, semantic account references, missing fields, and ambiguities.
3. Internal AccountIds are resolved outside the interpreter.
4. Unknown or ambiguous account references cannot silently become a selected account.
5. A valid resolved interpretation can produce the existing Proposal model without changing financial state.
6. Invalid/incomplete interpretations remain non-confirmable.
7. The same interpretation contract can be implemented by a deterministic test adapter without an AI provider.
8. Existing Proposal confirmation and Financial Command behavior remain unchanged.
9. No AI/parser dependency is introduced into the financial Domain.
10. Tests prove that interpreter failure, missing fields, and ambiguity cannot create financial Effects.

## Non-goals

- Selecting an AI vendor/model.
- Prompt engineering.
- OCR implementation.
- Voice implementation.
- Autonomous confirmation.
- New financial operation types.
- New accounting semantics.
- General-purpose agent/workflow orchestration.
- Account recommendation/ranking.

## Revisit conditions

Reopen this gate if:
- semantic interpretation proves insufficient for a new capture channel;
- context resolution requires materially different product semantics;
- ambiguity handling needs a richer user interaction model;
- evidence justifies a different interpreter architecture.
