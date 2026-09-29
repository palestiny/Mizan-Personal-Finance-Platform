# DG-023 Voice Fixture Execution Harness

**Status:** preparation-only design artifact. No provider is selected or benchmark execution is authorized by this document.

## Purpose

Define a reproducible execution harness for the three-case voice feasibility sample before materializing the remaining DG-023 voice fixtures.

The harness generates synthetic WAV files from the existing `voice-v1.json` transcript hints, then validates and records evidence without entering the Mizan application runtime.

## Initial cases

- `VOICE-001` — Egyptian Arabic expense with spoken amount/date/account.
- `VOICE-002` — Egyptian Arabic income with spoken number/account.
- `VOICE-004` — Arabic/English mixed owned-account transfer.

The transcript text must be read from the benchmark manifest. The harness must not introduce new financial semantics.

## Generator boundary

The generator is external preparation infrastructure:

`voice-v1.json -> independent TTS generator -> synthetic WAV`

It must not reference:

- `CaptureProviderAdapter`
- `CaptureInterpretation`
- Proposal persistence
- Financial Command
- Operation / Effect

The generator must use a built-in synthetic speaker. Voice cloning and external reference audio are prohibited for this feasibility run.

## Reproducibility inputs

Every execution records:

- generator name;
- package version;
- model repository;
- immutable model revision/commit;
- generator configuration;
- speaker identifier;
- input case ID;
- exact transcript;
- generation timestamp;
- runtime/Python version;
- OS/runtime environment;
- output WAV SHA-256.

The model revision must be immutable. A moving `main` reference is not sufficient evidence.

## Output layout

Expected preparation output:

```
artifacts/benchmarks/capture/voice/
  VOICE-001.wav
  VOICE-002.wav
  VOICE-004.wav
  feasibility-evidence.json
```

The generated WAV files are synthetic benchmark fixtures and must remain outside application runtime assets.

## Evidence schema

Each case records:

- case_id
- fixture_ref
- input_sha256
- output_sha256
- generator
- generator_version
- model
- model_revision
- speaker
- runtime_version
- sample_rate
- channels
- duration_seconds
- generation_config
- synthetic
- transcript_verification
- validation_status
- failure_category

The report must contain no provider API keys, credentials, internal AccountIds, or unnecessary personal data.

## Validation

A generated case is accepted only when all are true:

1. WAV exists and is non-empty.
2. WAV is decodable.
3. Case ID matches the requested manifest case.
4. Spoken content matches the manifest transcript sufficiently for independent human verification.
5. Sample rate and channels are recorded.
6. Duration is recorded.
7. SHA-256 is recorded.
8. Model revision is pinned.
9. Generator/package versions are recorded.
10. Synthetic classification is explicit.
11. No real-person reference voice was used.

Failure of any condition blocks that fixture.

## Feasibility decision

### PASS

All three cases produce independently verifiable fixtures satisfying the contract.

A PASS authorizes only materialization of the remaining nine benchmark fixtures.

### FAIL

Any required case cannot be generated or verified reproducibly.

A FAIL does not alter Mizan architecture. The next action is to evaluate another independent fixture generator.

## Important distinction

This harness tests whether benchmark inputs can be prepared reliably. It does **not** measure:

- speech-to-text quality;
- financial semantic interpretation;
- provider selection;
- production latency;
- production cost;
- confirmation safety.

Those belong to the later DG-023 provider benchmark.

## Current evidence

No local execution result is currently recorded. Therefore this harness remains unexecuted and no feasibility PASS is claimed.


## Executable preparation script

The repository-side preparation entry point is:

`tools/benchmarks/capture/materialize-voice-feasibility.ps1`

The current feasibility candidate is VoiceTut-TTS package version `0.1.1`, with the model pinned to immutable revision `2988105848781c1645f32d8fb3c1ef3c6ea51eeb`. The model revision corresponds to the repository commit that introduced the current Egyptian model weights; the model card documents the local `VoiceTutTTS.synthesize(..., speaker=..., output=...)` API.

Example execution after the preparation environment is installed:

```powershell
pwsh ./tools/benchmarks/capture/materialize-voice-feasibility.ps1 `
  -RepositoryRoot . `
  -ModelRevision 2988105848781c1645f32d8fb3c1ef3c6ea51eeb `
  -ExpectedPackageVersion 0.1.1 `
  -Speaker Mohamed
```

This command is intentionally not part of CI and must not run against real user voice data. It produces only synthetic feasibility fixtures and evidence.

The script fails closed when Python or the pinned `voicetut-tts` package version is unavailable/mismatched. It records a SHA-256 of the exact benchmark transcript and the generated WAV, plus runtime/model/package metadata.

**Execution status:** not executed yet. No feasibility PASS is claimed.
