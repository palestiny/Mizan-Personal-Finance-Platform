# DG-023 Voice Fixture Feasibility Test

**Status:** preparation gate — no provider selected.

## Objective

Determine whether an independent Egyptian-Arabic TTS model can reproducibly generate a small synthetic fixture sample for the DG-023 voice benchmark.

This is **not** a production-provider benchmark.

## Initial candidates

### VoiceTut-TTS

The model card describes Egyptian-Arabic TTS, Arabic/English code-switching, and Apache-2.0 licensing. It also exposes a pinned model revision in repository history that can be recorded for reproducibility. Official model-card evidence: https://huggingface.co/mohammedaly22/VoiceTut-TTS

### Chatterbox Egyptian v0

The model card describes Egyptian Arabic (Masri) synthesis and MIT licensing. It documents local inference and notes dialect/prosody limitations. Official model-card evidence: https://huggingface.co/oddadmix/chatterbox-egyptian-v0

## Feasibility sample

Generate only these representative cases initially:

1. Egyptian Arabic PersonalExpense with spoken decimal amount.
2. Egyptian Arabic Income with an explicit account reference.
3. Arabic/English code-switching transfer.

The exact text must come from the existing `voice-v1.json` manifest; no new financial semantics are introduced by this test.

## Verification

For each generated WAV:

- case ID matches the manifest;
- spoken content is independently verified;
- WAV is decodable;
- sample rate and channel count are recorded;
- duration is recorded;
- SHA-256 is recorded;
- generator model revision is pinned;
- generator package/runtime version is recorded;
- license/provenance are recorded;
- no real person's reference voice is used;
- generated audio is explicitly marked synthetic.

## Pass criteria

The feasibility test passes only if all three samples:

- contain the intended Egyptian-Arabic/code-switching content;
- are independently verifiable;
- are reproducibly generated from a pinned revision/configuration;
- satisfy the audio fixture contract.

A pass authorizes **materialization of the remaining benchmark fixtures only**. It does not authorize provider selection or provider execution.

## Current evidence

Capability claims are documented from the candidate model cards, but no local generation result has yet been recorded in this repository.

Therefore the voice benchmark remains **blocked**.

## Boundary

`Fixture generator → synthetic WAV` is preparation infrastructure.

It must remain outside:

- CaptureProviderAdapter
- CaptureInterpretation
- Proposal persistence
- Financial Command
- Operation / Effect

No generated fixture can mutate financial state.