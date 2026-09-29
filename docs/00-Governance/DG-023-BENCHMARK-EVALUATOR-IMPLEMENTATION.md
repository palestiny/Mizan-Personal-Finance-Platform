# DG-023 Benchmark Evaluator Implementation

**Status:** Implemented on the DG-023 proposal branch. Provider execution is not enabled.

## Purpose

Provide a provider-independent evaluator for DG-023 benchmark observations before any production provider is selected.

The evaluator compares a normalized `CaptureInterpretation` against human-authored expected semantics and produces separate measurements instead of a single weighted score.

## Scope

Implemented under `tests/Mizan.Application.Tests/Benchmarks/`:

- expected benchmark semantics
- transient provider observation model
- per-case evaluation
- semantic exactness checks
- structured-output validity
- missing-field preservation
- ambiguity preservation
- contradiction rejection
- unsafe-authority and false-confirmation signals
- metric aggregation with case IDs

## Safety boundary

The evaluator only consumes normalized interpretation data and transient evaluation signals.

It has no reference to:

- Proposal creation
- Financial Command execution
- financial persistence
- AccountIds
- OperationIds
- Effects

Benchmark evaluation therefore remains read-only with respect to financial state.

## Metric rule

Metrics remain separate and retain the case IDs that passed each measurement. The implementation intentionally does not calculate a single provider score or ranking.

## Next implementation step

Add a thin catalog/result runner that:

1. loads the versioned synthetic benchmark cases;
2. invokes a provider adapter supplied by the experiment;
3. records normalized output and validation metadata;
4. measures latency/failure categories;
5. evaluates each case with this evaluator;
6. emits reproducible result artifacts without retaining raw provider payloads by default.

A real provider adapter must not be introduced until the DG-023 Product Owner decision is closed.
