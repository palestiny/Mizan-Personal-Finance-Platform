# DG-023 Voice Feasibility Environment

## Status

Preparation-only. This document does not select a production provider and does not authorize DG-023 provider benchmarking.

## Purpose

Define the minimum isolated environment required to execute the three-case VoiceTut-TTS fixture-feasibility harness.

The environment is deliberately separate from the Mizan .NET runtime and from any production capture provider.

## Candidate under evaluation

- Generator: VoiceTut-TTS
- Package target: `voicetut-tts==0.1.1`
- Model: `mohammedaly22/VoiceTut-TTS`
- Model revision: `2988105848781c1645f32d8fb3c1ef3c6ea51eeb`
- Speaker: `Mohamed`
- Required Python baseline: 3.10
- Synthetic built-in speaker only
- No reference audio / voice cloning

The generator is only being evaluated as an independent benchmark-fixture generator. It is not being selected as Mizan's production capture provider.

## Why the environment is isolated

VoiceTut-TTS documents a Python 3.10 source environment and a CUDA-enabled PyTorch installation, followed by the OmniVoice backbone and VoiceTut-TTS package. The exact CUDA/PyTorch combination must be selected for the actual execution host rather than guessed in repository metadata.

The current VoiceTut-TTS documentation recommends installing PyTorch for the host CUDA version, installing OmniVoice from its Git repository, and then installing VoiceTut-TTS. The repository does not currently record a complete lockfile for the feasibility environment.

## Preparation procedure

Use a fresh Python 3.10 environment on a machine with an NVIDIA GPU.

Install the host-compatible PyTorch build first, following the current PyTorch installation guidance.

Then install:

```powershell
python -m pip install git+https://github.com/k2-fsa/OmniVoice.git
python -m pip install voicetut-tts==0.1.1
```

Verify:

```powershell
python --version
python -c "import torch; print(torch.__version__); print(torch.cuda.is_available())"
python -c "import voicetut_tts; print(voicetut_tts.__version__)"
```

The expected package version is exactly `0.1.1`.

Do not proceed when:

- Python is not 3.10;
- `torch.cuda.is_available()` is false for a GPU execution attempt;
- VoiceTut-TTS is not exactly version 0.1.1;
- the installed OmniVoice dependency is incompatible;
- the environment cannot load the pinned model revision.

## Reproducibility capture

Before generating fixtures, capture the actual environment that produced the evidence:

```powershell
python -m pip freeze > artifacts/benchmarks/capture/voice/environment-pip-freeze.txt
python -c "import platform,sys,torch; print(platform.platform()); print(sys.version); print(torch.__version__); print(torch.version.cuda); print(torch.cuda.get_device_name(0) if torch.cuda.is_available() else 'NO_CUDA_DEVICE')"
```

The captured `pip-freeze` is execution evidence, not a dependency declaration for the Mizan application.

Do not convert guessed versions into permanent pins before a successful feasibility execution has produced the real dependency graph.

## Execution

After environment verification:

```powershell
pwsh ./tools/benchmarks/capture/materialize-voice-feasibility.ps1 `
  -RepositoryRoot . `
  -ModelRevision 2988105848781c1645f32d8fb3c1ef3c6ea51eeb `
  -ExpectedPackageVersion 0.1.1 `
  -Speaker Mohamed
```

The harness generates only:

- `VOICE-001.wav`
- `VOICE-002.wav`
- `VOICE-004.wav`
- `feasibility-evidence.json`

It must not invoke Mizan financial runtime components.

## Acceptance

Environment preparation is not a feasibility PASS.

A feasibility PASS requires:

1. all three WAVs generated successfully;
2. all WAVs decode;
3. case identity is preserved;
4. model revision and package version are recorded;
5. hashes are recorded;
6. synthetic speaker provenance is recorded;
7. spoken content is independently verified against the manifest;
8. no real-person reference voice was used.

Only after those checks pass may the remaining nine voice fixtures be materialized.

## Current status

**Environment not executed.**

No voice feasibility PASS is claimed.
