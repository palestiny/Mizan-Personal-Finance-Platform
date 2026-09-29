# Receipt Benchmark Fixture Plan

## Status

Synthetic fixture plan for DG-023. The 20 manifest references now resolve to repository SVG fixtures; these establish deterministic fixture identity and resolver coverage but are not yet representative raster OCR/vision evidence.

## Fixture rule

Every `fixture_ref` must resolve to a verified synthetic image before receipt provider execution. The current SVG fixtures satisfy repository fixture resolution, but a provider-execution gate still requires representative raster-image fixtures and verification.

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

## Current limitation

The repository currently contains 20 synthetic SVG receipt fixtures bound by `fixture_ref`. They are intentionally treated as structured visual fixtures rather than production-realistic OCR evidence. Provider execution remains blocked until rasterized fixtures are materialized, verified, and shown to be suitable for the selected provider path.
