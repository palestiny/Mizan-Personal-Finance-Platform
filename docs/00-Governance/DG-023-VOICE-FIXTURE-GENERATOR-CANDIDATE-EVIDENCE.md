# DG-023 Voice Fixture Generator Candidate Evidence

**Status:** evidence collection only — no production capture provider selected.

**As-of:** 2026-09-28

## Purpose

This document evaluates **fixture-generation candidates only** for the DG-023 synthetic voice benchmark.

A fixture generator is not a Mizan capture provider and must not be used as evidence for production-provider quality.

## Candidate evidence

| Candidate | Egyptian Arabic claim | Code-switching | License stated | Main verification concern |
|---|---|---|---|---|
| VoiceTut-TTS | Yes | Yes | Apache-2.0 | Reproduce locally and verify exact checkpoint/revision, generated content, audio metadata, and provenance. |
| oddadmix/chatterbox-egyptian-v0 | Yes | Not the primary documented focus | MIT | Reproduce locally and verify checkpoint/revision, dialect fidelity, and provenance. |
| AliAbdallah/egyptian-arabic-tts-chatterbox | Yes | Base model is multilingual | Apache-2.0 | Verify checkpoint/revision, generation reproducibility, and provenance. |
| itshamdi404/Egy_Arabic_Qwen3-TTS-12Hz-1.7B-Base | Yes | Model card describes Egyptian Arabic specialization | Apache-2.0 | Verify runtime requirements, checkpoint revision, and generated-content fidelity. |

## Important constraint

The selected generator must be independent from any production capture provider being benchmarked.

For example, if a candidate capture provider also offers TTS, its TTS service must not generate the benchmark audio used to evaluate that provider. Otherwise the benchmark can introduce provider-specific pronunciation, preprocessing, codec, or model-family advantages.

## Required verification before use

For any candidate:

1. Pin an immutable model revision/commit.
2. Record model-card URL, license, and provenance.
3. Verify license compatibility with storing synthetic benchmark artifacts in the repository.
4. Generate the exact 12 manifest transcripts.
5. Verify the spoken content independently from the generator.
6. Verify WAV decodability, duration, sample rate, channels, and SHA-256.
7. Record generator version/configuration as provenance only.
8. Confirm Egyptian-Arabic cases are actually Egyptian Arabic; do not relabel another Arabic dialect.
9. Apply only the degradation declared by the benchmark case.
10. Keep the generator out of provider-quality metrics.

## Current decision

No generator is selected yet.

The next action is a **small fixture-generation feasibility test**, not a production-provider selection:

- generate 2-3 representative ar-EG cases;
- verify content and audio metadata;
- verify reproducibility;
- review license/provenance;
- only then materialize the complete 12-case voice fixture set.

## Sources

- VoiceTut-TTS: https://huggingface.co/mohammedaly22/VoiceTut-TTS
- VoiceTut-TTS repository: https://github.com/MohammedAly22/VoiceTuT-TTS
- Chatterbox Egyptian v0: https://huggingface.co/oddadmix/chatterbox-egyptian-v0
- AliAbdallah Egyptian Arabic TTS: https://huggingface.co/AliAbdallah/egyptian-arabic-tts-chatterbox
- Egyptian Arabic Qwen3-TTS: https://huggingface.co/itshamdi404/Egy_Arabic_Qwen3-TTS-12Hz-1.7B-Base
