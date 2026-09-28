# DG-023 — Provider Benchmark Specification

**Purpose:** turn provider selection into a reproducible technical experiment.

## Test record

Every benchmark case records:

- case ID
- channel: text / receipt / voice
- locale
- synthetic input
- expected semantic interpretation
- required fields
- intentionally missing fields, if any
- intentionally ambiguous fields, if any
- provider/model/configuration under test
- normalized provider interpretation
- validation result
- latency
- failure category
- evaluator notes

The authoritative expected interpretation is created before provider execution and is independent of provider output.

## Measurements

Report separately:

- operation-type correctness
- amount correctness
- currency correctness
- effective-time correctness
- semantic account-reference correctness
- missing-field detection
- ambiguity detection
- contradiction rejection
- structured-output validity
- latency
- availability/failure behavior
- estimated cost
- privacy/data-handling constraints
- Arabic-language behavior

Do not collapse these into one opaque score.

## Test classes

### Valid complete
Expected: correct semantics and no unexplained missing/ambiguous fields.

### Missing information
Expected: missing field is surfaced; no invented value; Proposal remains non-confirmable.

### Ambiguous information
Expected: ambiguity is surfaced; no account is silently selected.

### Contradictory information
Expected: contradiction is surfaced or rejected; no confident guess is promoted to authority.

### Malformed/adversarial
Inputs may attempt to force unsupported fields, authoritative IDs, confirmation, or execution.
Expected: authority claims are rejected/ignored and provider output cannot bypass validation.

### Channel degradation
Receipt images and voice samples include realistic quality degradation.
Expected: uncertain extraction is surfaced; failure is fail-closed; no fabricated financial semantics.

## Candidate classes

Evaluate at least:

1. general multimodal model with structured-output support
2. specialized document/receipt extraction service
3. speech-to-text or multimodal voice-capable service

This is a capability comparison, not a vendor commitment.

Current public documentation provides examples of both approaches: Gemini documents JSON-Schema-constrained structured output, while Azure Document Intelligence documents a GA receipt model returning structured receipt fields. These are evidence inputs, not a production selection. 

## Privacy rule

Benchmark data should be synthetic or explicitly permitted for external processing.

Do not place real account numbers, authentication secrets, payment credentials, or unnecessary personal financial information into provider benchmark cases.

## Exit condition

The benchmark is complete when:

- all required case classes have expected interpretations;
- each selected candidate has reproducible test configuration;
- results are recorded per capability;
- failures and false positives are reviewed;
- privacy/data-handling constraints are documented;
- cost/latency observations are recorded;
- a Product Owner decision can be made without relying on provider marketing claims.
