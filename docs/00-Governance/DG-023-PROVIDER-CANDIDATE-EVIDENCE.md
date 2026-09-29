# DG-023 Provider Candidate Evidence Matrix

**Status:** evidence collection only — no provider selected.

**As-of:** 2026-09-28

This document records externally documented capabilities that are relevant to the DG-023 benchmark. It is not a scorecard and does not rank candidates.

## Candidate capability classes

| Candidate | Text | Image | Audio input | Structured output | Notes |
|---|---|---|---|---|---|
| OpenAI API | Yes | Yes | Yes | Yes | Current official API documentation exposes image/vision input, audio input/transcription, and structured outputs. Exact model/API pairing must be fixed before benchmark execution. |
| Google Gemini API | Yes | Yes | Yes | Yes | Official documentation exposes multimodal input including image/audio, structured JSON output, and audio understanding/transcription. Exact model/API pairing must be fixed before benchmark execution. |
| Azure AI / Foundry | Yes | Model-dependent | Model-dependent | Model-dependent | Azure Foundry exposes a broad model catalog, including multimodal models. Exact deployed model and capability contract must be selected before benchmarking. |

## Evidence captured

### OpenAI

Official documentation currently describes:
- image inputs for vision-capable models;
- audio workflows including file transcription and audio input in chat;
- structured outputs compatible with supported vision workflows.

Sources:
- https://developers.openai.com/api/docs/guides/images-vision
- https://developers.openai.com/api/docs/guides/audio
- https://developers.openai.com/api/docs/guides/audio-chat-completions
- https://openai.com/index/introducing-structured-outputs-in-the-api/

### Google Gemini

Official documentation currently describes:
- image/text multimodal inputs;
- audio understanding and transcription;
- structured JSON output;
- supported audio formats including WAV, MP3, FLAC, M4A and others.

Current pricing documentation also exposes separate text/image/audio input pricing by model and service tier. Pricing is time-sensitive and must be captured again at benchmark execution time.

Sources:
- https://ai.google.dev/gemini-api/docs/audio
- https://ai.google.dev/gemini-api/docs/structured-output
- https://ai.google.dev/gemini-api/docs/pricing

### Azure AI / Foundry

Microsoft documents Foundry as a model platform with multimodal model availability, including text/image and audio-capable model entries. Exact capabilities and pricing depend on the selected model/deployment and must therefore be treated as configuration evidence rather than a platform-wide guarantee.

Source:
- https://azure.microsoft.com/en-us/pricing/details/ai-foundry-models/microsoft/

## Benchmark implications

1. Capability documentation is **not** benchmark evidence.
2. No candidate is selected by this document.
3. Each candidate must be tested through the same vendor-neutral benchmark contract.
4. The benchmark must record the exact provider, model, API, configuration, timestamp, latency, failures, and normalized output.
5. Provider confidence cannot authorize Proposal confirmation.
6. Privacy/data-retention terms must be reviewed for the exact API/product configuration used.
7. Current pricing must be captured at execution time; this document must not be treated as a permanent price source.

## Current blocker

Voice fixtures are still unavailable in the repository. No provider execution should begin until the fixture contract is satisfied for the modalities being evaluated.
