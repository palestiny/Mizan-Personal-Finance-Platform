# DG-023 Benchmark Result Contract

**Status:** Design-only contract. No provider execution is implied.

## Per-case result

Each execution record should contain:

- benchmark_case_id
- provider_id
- model_id
- provider_configuration_id
- started_at
- duration_ms
- succeeded
- normalized_interpretation
- validation_result
- failure_category
- raw_payload_retained: false by default
- evaluator_notes

## Derived measurements

Measurements are calculated from per-case records, never manually entered:

- operation_type_exact
- amount_exact
- currency_exact
- effective_time_exact
- account_reference_exact
- destination_account_reference_exact
- missing_fields_correct
- ambiguities_correct
- contradiction_rejected
- structured_output_valid
- unsafe_authority_attempt
- false_confirmation_attempt
- timeout
- provider_error

Each measurement keeps case IDs for auditability.

## Provider isolation

The benchmark runner may know provider configuration, but benchmark cases and expected semantics must not depend on provider-specific fields.

Provider adapters must normalize into the existing vendor-neutral CaptureInterpretation contract before evaluation.

## Safety

Benchmark execution is read-only with respect to Mizan financial state.

A benchmark adapter must not call Proposal creation, Financial Command execution, or any authoritative persistence path.

## Reproducibility

A result set must identify:

- provider
- model/version where exposed
- adapter version/commit
- configuration identifier
- benchmark dataset version
- execution timestamp
- environment where latency was measured

Results without this metadata are incomplete evidence.

## Privacy

Raw provider responses and submitted captures are not retained by default. Store normalized benchmark results and failure metadata unless a separate privacy decision permits raw retention.
