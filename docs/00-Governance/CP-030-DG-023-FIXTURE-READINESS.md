# CP-030 — DG-023 Fixture Readiness

**Status:** In progress  
**Gate:** DG-023 Capture Provider Selection & Evidence  
**Purpose:** convert the approved evidence method into executable, independently verifiable multimodal benchmark inputs.

## Completion contract

CP-030 is complete only when:

- 20 receipt raster fixtures are materialized and independently verified;
- 12 voice fixtures are materialized and independently verified;
- all executable fixture references resolve through the existing fixture contract;
- hashes, formats, dimensions/audio metadata, provenance, and synthetic classification are recorded;
- preparation failures are distinct from provider failures;
- no provider is selected by fixture generation;
- no Proposal, Financial Command, Operation, or Effect is created;
- the provider-neutral benchmark runner can consume the verified fixture packages.

## Completed before this checkpoint

- DG-023 approved and merged.
- Text benchmark corpus and evaluator/runner exist.
- Receipt SVG source fixtures and receipt materializer exist.
- Receipt preparation workflow exists with pinned raster tooling.
- Voice manifest, fixture contract, feasibility harness, and isolated environment specification exist.
- Voice fixture generation is explicitly independent of the production provider.

## Execution status

### Receipt

**Ready to execute:** yes.

The preparation workflow now supports both manual dispatch and push-based execution when its materialization inputs change. This makes the preparation step observable in GitHub Actions without changing application/runtime boundaries.

**Not yet verified:** successful workflow run and the resulting 20 raster artifacts.

### Voice

**Ready to execute:** preparation harness exists.

**Blocked on environment:** the repository specifies an isolated Python 3.10 + NVIDIA GPU environment for VoiceTut-TTS feasibility. The current execution surfaces available to this workflow do not provide that GPU environment.

No voice PASS is claimed until the three-case feasibility sample is actually generated and independently verified.

## Decision boundary

CP-030 does not select a production capture provider.

After fixture readiness is proven, provider execution may begin under the DG-023 benchmark contract. Production selection remains a separate DG-024 decision.

## Current gate status

**NOT CLOSED.**

The remaining evidence is execution evidence, not additional architecture design:
1. receipt workflow PASS + 20 accepted raster fixtures;
2. voice feasibility PASS + 12 accepted voice fixtures;
3. final fixture-resolution verification.

Until these are present, DG-023 provider benchmarking remains blocked.
