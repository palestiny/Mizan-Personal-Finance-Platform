[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [Parameter(Mandatory = $true)]
    [string]$ModelRevision,

    [string]$Model = "mohammedaly22/VoiceTut-TTS",
    [string]$Speaker = "Mohamed",
    [string]$OutputDirectory = "artifacts/benchmarks/capture/voice"
)

$ErrorActionPreference = "Stop"

function Resolve-RepoPath([string]$Path) {
    $root = (Resolve-Path $RepositoryRoot).Path
    $candidate = [System.IO.Path]::GetFullPath((Join-Path $root $Path))
    if (-not $candidate.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path escapes repository root: $Path"
    }
    return $candidate
}

$manifestPath = Resolve-RepoPath "docs/00-Governance/benchmarks/capture/voice-v1.json"
$outDir = Resolve-RepoPath $OutputDirectory
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$manifest = Get-Content -Raw -Path $manifestPath | ConvertFrom-Json
if ($manifest.schema -ne "m3-capture-voice-benchmark-v1") {
    throw "Unsupported voice benchmark schema: $($manifest.schema)"
}

$caseIds = @("VOICE-001", "VOICE-002", "VOICE-004")
$cases = @($manifest.cases | Where-Object { $caseIds -contains $_.case_id })
if ($cases.Count -ne $caseIds.Count) {
    throw "Manifest does not contain all required feasibility cases."
}

$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    throw "Python is required for the VoiceTut-TTS feasibility harness."
}

$script = @'
import argparse, json, hashlib, os, platform, sys, wave
from datetime import datetime, timezone

def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--cases", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--model", required=True)
    p.add_argument("--revision", required=True)
    p.add_argument("--speaker", required=True)
    args = p.parse_args()

    try:
        from voicetut_tts import VoiceTutTTS
    except Exception as exc:
        raise RuntimeError(
            "voicetut-tts is not installed in this Python environment. "
            "Install the pinned preparation dependency before running this harness."
        ) from exc

    with open(args.cases, "r", encoding="utf-8") as f:
        cases = json.load(f)

    os.makedirs(args.output, exist_ok=True)
    generator = VoiceTutTTS.from_pretrained(args.model, revision=args.revision)

    evidence = {
        "schema": "m3-capture-voice-feasibility-evidence-v1",
        "generated_at_utc": datetime.now(timezone.utc).isoformat(),
        "generator": "VoiceTut-TTS",
        "model": args.model,
        "model_revision": args.revision,
        "speaker": args.speaker,
        "runtime": {
            "python": platform.python_version(),
            "platform": platform.platform()
        },
        "cases": []
    }

    for case in cases:
        case_id = case["case_id"]
        transcript = case["transcript"]
        output_path = os.path.join(args.output, case_id + ".wav")

        result = generator.generate(
            text=transcript,
            speaker=args.speaker
        )
        result.save(output_path)

        if not os.path.isfile(output_path) or os.path.getsize(output_path) == 0:
            raise RuntimeError(f"{case_id}: generated WAV is missing or empty")

        with wave.open(output_path, "rb") as wav:
            sample_rate = wav.getframerate()
            channels = wav.getnchannels()
            frames = wav.getnframes()
            duration = frames / float(sample_rate) if sample_rate else 0.0

        evidence["cases"].append({
            "case_id": case_id,
            "fixture_ref": "artifacts/benchmarks/capture/voice/" + case_id + ".wav",
            "output_sha256": sha256(output_path),
            "generator": "VoiceTut-TTS",
            "model": args.model,
            "model_revision": args.revision,
            "speaker": args.speaker,
            "runtime_version": platform.python_version(),
            "sample_rate": sample_rate,
            "channels": channels,
            "duration_seconds": duration,
            "generation_config": {"speaker": args.speaker},
            "synthetic": True,
            "transcript": transcript,
            "transcript_verification": "pending-independent-human-verification",
            "validation_status": "generated-awaiting-independent-verification",
            "failure_category": None
        })

    with open(os.path.join(args.output, "feasibility-evidence.json"), "w", encoding="utf-8") as f:
        json.dump(evidence, f, ensure_ascii=False, indent=2)

if __name__ == "__main__":
    main()
'@

$temp = Join-Path ([System.IO.Path]::GetTempPath()) ("mizan-voice-harness-" + [guid]::NewGuid().ToString("N") + ".py")
Set-Content -Path $temp -Value $script -Encoding UTF8
try {
    $caseJson = Join-Path ([System.IO.Path]::GetTempPath()) ("mizan-voice-cases-" + [guid]::NewGuid().ToString("N") + ".json")
    $cases | ConvertTo-Json -Depth 20 | Set-Content -Path $caseJson -Encoding UTF8
    & $python.Source $temp --cases $caseJson --output $outDir --model $Model --revision $ModelRevision --speaker $Speaker
    if ($LASTEXITCODE -ne 0) { throw "Voice fixture generator exited with code $LASTEXITCODE." }
}
finally {
    Remove-Item $temp -Force -ErrorAction SilentlyContinue
    if ($caseJson) { Remove-Item $caseJson -Force -ErrorAction SilentlyContinue }
}
