# DG-023 Multimodal Fixture Materialization Runbook

## Status

Preparation runbook only. It does not select a production provider and does not authorize provider execution.

## Objective

Turn the DG-023 receipt and voice manifests into reproducible benchmark inputs without making the benchmark dependent on the provider being evaluated.

The benchmark must distinguish:

1. fixture generation — creation of synthetic input;
2. fixture verification — proving the generated artifact matches the manifest case;
3. provider execution — sending the verified artifact to a candidate provider;
4. evaluation — comparing the normalized provider observation with pre-authored ground truth.

A generated artifact is not benchmark evidence until steps 1-2 pass.

## Independence rule

A benchmark input generator must not require the production provider under evaluation.

For voice, do not generate a candidate provider's benchmark audio with that same provider's TTS service. Doing so can introduce provider-specific pronunciation, prosody, codec, or preprocessing advantages.

For receipt images, do not use a candidate provider's vision/OCR output to generate or repair the benchmark image.

The preferred generation path is local/open tooling with a reviewed license, or a separately governed synthetic-artifact source.

## Receipt materialization

The repository currently contains 20 synthetic SVG fixtures. They establish deterministic fixture identity and resolver behavior, but they are not yet representative raster OCR/vision evidence.

The next materialization step is:

1. rasterize each SVG into PNG using the repository materialization script and an explicitly verified `rsvg-convert` version;
2. preserve the manifest case ID;
3. apply only the degradation specified by the case;
4. verify dimensions, MIME/format, decodability, and SHA-256;
5. compare the rendered image against the intended synthetic receipt content;
6. record the tool/version and fixture hash;
7. update fixture_ref only if the repository path changes.

The rasterization tool is a benchmark-preparation dependency, not an application runtime dependency. The repository now contains `tools/benchmarks/capture/materialize-receipts.ps1`, which requires an explicitly supplied `rsvg-convert` version, rasterizes the 20 SVG sources to PNG, records SHA-256 and tool provenance, and writes preparation evidence under `artifacts/`. It does not claim provider-execution evidence by itself and does not invent degradation beyond what is declared by the manifest.

### Receipt acceptance

A receipt fixture is executable evidence only when all are true:

- the referenced raster file exists;
- the resolver accepts it;
- the file decodes successfully;
- the fixture ID matches the manifest;
- SHA-256 is recorded;
- dimensions and format are recorded;
- synthetic/privacy checks pass;
- the image visibly preserves the intended case characteristics;
- the provider benchmark runner can load the exact same bytes.

SVG fixtures remain retained as source fixtures where useful; they do not substitute for raster evidence.

## Voice materialization

The voice manifest contains 12 cases and currently has no executable audio fixtures.

The next materialization step is:

1. select an independent synthetic voice-generation method;
2. review its license and provenance;
3. generate each transcript as an audio fixture;
4. apply only the degradation declared by the case;
5. verify playable/decodable WAV output;
6. record sample rate, channel count, duration, and SHA-256;
7. verify the spoken content against the case transcript;
8. link the exact artifact from fixture_ref;
9. keep generator metadata outside the vendor-neutral expected semantics.

### Egyptian Arabic requirement

Cases labelled ar-EG must use an Egyptian-Arabic-capable generation method. A Jordanian Arabic voice, Persian voice, or another Arabic dialect must not be relabeled as Egyptian evidence.

The current local espeak environment does not provide an acceptable Arabic voice for this benchmark. Therefore it must not be used to fabricate the missing Arabic fixtures.

An independent open Egyptian-Arabic TTS model may be evaluated as a fixture-generation candidate, subject to license/provenance review and reproducibility checks. The generator is not a production capture provider and its output must not be scored as the generator's own provider result.

### Voice acceptance

A voice fixture is executable evidence only when all are true:

- the referenced WAV exists;
- the resolver accepts it;
- the file decodes successfully;
- the fixture ID matches the manifest;
- spoken content is verified;
- duration/sample-rate/channel metadata is recorded;
- SHA-256 is recorded;
- synthetic/privacy checks pass;
- degradation matches the manifest;
- the benchmark runner can load the exact same bytes.

## Evidence record

For every materialized fixture, preserve:

case_id -> dataset_version -> fixture_ref -> fixture_sha256 -> generator/tool/version -> fixture_verification -> provider/model/config -> normalized_observation -> evaluation

Generator metadata is diagnostic provenance. It must not become provider-specific semantic input.

## Failure taxonomy

Materialization failures remain separate from provider failures:

- fixture_generation_failed
- fixture_missing
- fixture_unreadable
- fixture_mismatch
- fixture_invalid_format
- fixture_content_mismatch
- fixture_privacy_violation
- fixture_provenance_unverified

A case with any preparation failure is not sent to a provider and is not counted in provider-quality metrics.

## Execution gate

DG-023 provider execution remains blocked until:

- 20 receipt raster fixtures pass the receipt acceptance checks;
- 12 voice fixtures pass the voice acceptance checks;
- the fixture resolver can resolve all executable cases;
- benchmark preparation produces reproducible hashes;
- the Product Owner has decided whether to proceed with provider benchmarking.

No fixture-preparation work creates Proposals, Operations, Effects, or Financial Commands.
