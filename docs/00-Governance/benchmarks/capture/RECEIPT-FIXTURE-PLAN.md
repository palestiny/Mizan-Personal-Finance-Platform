# Receipt Benchmark Fixture Plan

## Status

Design-stage synthetic fixture manifest for DG-023. The manifest defines expected receipt semantics but does not claim that image files exist yet.

## Fixture rule

Every `fixture_ref` must resolve to a synthetic image before receipt provider execution. A manifest entry alone is not evidence.

## Required coverage

- 20 receipt cases
- Arabic, English, mixed-language
- clean and degraded captures
- tax and fee fields
- missing and ambiguous totals/currency
- contradictory totals
- adversarial text
- date extraction

## Privacy

Fixtures must be synthetic. Do not use real receipts, account numbers, payment credentials, or unnecessary personal data.

## Evaluation boundary

Receipt extraction is normalized into the existing vendor-neutral capture interpretation boundary. Receipt-specific fields such as merchant, tax, fees, and line items are diagnostic/auxiliary evidence unless and until a separate financial semantics gate promotes them.

No fixture may create a Proposal, Operation, or Effect.

## Important limitation

The current repository commit defines the manifest and ground truth only. It does not claim the referenced image files exist. Provider execution remains blocked until the fixture files are actually created and verified.
