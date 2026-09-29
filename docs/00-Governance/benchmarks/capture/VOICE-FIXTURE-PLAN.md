# Voice Benchmark Fixture Plan

## Status

Design-stage synthetic voice manifest and ground truth for DG-023. The manifest does not claim that audio files exist.

## Fixture rule

Every `fixture_ref` must resolve to a verified synthetic audio fixture before voice provider execution. A transcript hint or manifest entry alone is not executable evidence.

## Required coverage

The initial manifest contains 12 representative cases covering:

- Egyptian Arabic
- English
- Arabic/English code switching
- PersonalExpense, Income, and OwnedAccountTransfer
- colloquial and spoken number forms
- background noise
- pauses
- missing account reference
- ambiguous account reference
- contradictory source/destination
- adversarial confirmation/internal-ID injection

## Ground truth

Expected semantics are authored independently of any speech-to-text or multimodal provider. Provider output must preserve uncertainty for missing, ambiguous, and contradictory input.

Provider confidence is diagnostic only. It cannot authorize Proposal confirmation or financial execution.

## Privacy

Fixtures must be synthetic. Do not use real voice recordings, account numbers, payment credentials, authentication secrets, or unnecessary personal data.

## Evaluation boundary

Voice input is normalized into the same vendor-neutral capture interpretation boundary used by text and receipt adapters. No fixture may create a Proposal, Operation, or Effect.

## Audio generation and verification

Before provider execution, each fixture must be:

1. generated or otherwise materialized as synthetic audio;
2. verified against its case ID and intended spoken content;
3. checked for playable/decodable format and duration;
4. linked from `fixture_ref`;
5. included in the reproducible benchmark execution configuration.

Until those checks pass, voice cases remain design evidence only.
