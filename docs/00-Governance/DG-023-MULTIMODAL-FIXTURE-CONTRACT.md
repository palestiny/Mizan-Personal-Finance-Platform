# DG-023 Multimodal Benchmark Fixture Contract

## Status

Design-stage contract for benchmark input materialization. It does not select a provider and does not enable provider execution.

## Problem

DG-023 now has three benchmark channels:

- text
- receipt/image
- voice/audio

The text benchmark currently stores its input inline, while receipt and voice manifests reference external synthetic fixtures. Provider selection evidence must use one reproducible execution model without coupling the benchmark to a provider SDK.

## Contract

Each benchmark case resolves to a **Benchmark Input Package** before provider execution.

A package contains:

- `case_id`
- `channel`
- `locale`
- `input_kind`
- `input_reference`
- `input_sha256` when materialized
- `dataset_version`
- `fixture_version`
- `synthetic=true` unless an explicit privacy decision permits otherwise

### Input kinds

| Channel | Input kind | Source |
|---|---|---|
| text | inline_text | case catalog |
| receipt | image_file | verified synthetic fixture |
| voice | audio_file | verified synthetic fixture |

The benchmark runner must consume the resolved package rather than knowing how a provider stores or transmits the input.

## Resolution rules

1. Text input is resolved directly from the catalog.
2. Receipt and voice `fixture_ref` values must resolve to an existing verified fixture.
3. Missing fixtures are a benchmark preparation failure, not a provider failure.
4. A fixture's SHA-256 is recorded when execution evidence is produced.
5. Fixture resolution must not mutate financial state.
6. Provider adapters receive only the resolved benchmark input and reproducible configuration.
7. Provider-specific request objects remain inside the provider adapter.

## Fixture validation

Before execution, each non-text fixture must pass:

- reference exists;
- expected file type is supported;
- file is decodable;
- fixture ID matches the manifest;
- content hash is recorded;
- synthetic/privacy classification is known;
- no prohibited secrets or unnecessary personal data are present.

For audio, also record duration and sample-rate/channel metadata where available.

For images, also record dimensions and format where available.

## Failure taxonomy

Keep preparation failures distinct from provider failures:

- `fixture_missing`
- `fixture_unreadable`
- `fixture_mismatch`
- `fixture_invalid_format`
- `fixture_privacy_violation`

These failures must prevent provider execution for that case. They must not be interpreted as provider quality failures.

## Result contract alignment

The existing DG-023 result contract remains authoritative for provider observations. The resolved input package adds reproducibility metadata; it does not add provider-specific semantics.

A benchmark result must therefore be traceable:

`case_id → dataset_version → fixture_ref → fixture_sha256 → provider/model/config → normalized observation → evaluation`

## Safety boundary

Fixture generation, fixture resolution, and benchmark execution are strictly non-authoritative.

They must never:

- create Proposals;
- execute Financial Commands;
- create Operations or Effects;
- resolve semantic references to internal AccountIds;
- treat provider confidence as confirmation.

## M3 scope

M3 will initially use one provider adapter per selected production path. The fixture contract is provider-neutral and must remain valid if later capability-specific adapters are introduced.

No runtime multi-provider fallback is introduced by this contract.
