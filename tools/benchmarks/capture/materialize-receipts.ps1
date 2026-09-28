param(
    [string]$ManifestPath = "docs/00-Governance/benchmarks/capture/receipts-v1.json",
    [string]$RepositoryRoot = ".",
    [string]$OutputDirectory = "artifacts/benchmarks/capture/receipts",
    [Parameter(Mandatory = $true)]
    [string]$ExpectedRsvgVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Fail([string]$Message) {
    throw "DG-023 receipt materialization failed: $Message"
}

$command = Get-Command rsvg-convert -ErrorAction SilentlyContinue
if ($null -eq $command) {
    Fail "rsvg-convert was not found on PATH."
}

$versionOutput = (& $command.Source --version 2>&1 | Out-String).Trim()
if ([string]::IsNullOrWhiteSpace($versionOutput)) {
    Fail "Unable to determine rsvg-convert version."
}

if ($versionOutput -notmatch [regex]::Escape($ExpectedRsvgVersion)) {
    Fail "Expected rsvg-convert version '$ExpectedRsvgVersion', but detected '$versionOutput'."
}

$manifestFullPath = [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $ManifestPath))
if (-not (Test-Path -LiteralPath $manifestFullPath -PathType Leaf)) {
    Fail "Manifest not found: $manifestFullPath"
}

$manifest = Get-Content -LiteralPath $manifestFullPath -Raw | ConvertFrom-Json
if ($manifest.schema -ne "m3-capture-receipts-v1") {
    Fail "Unexpected receipt manifest schema '$($manifest.schema)'."
}

$repositoryRootFullPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
$outputFullPath = [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $OutputDirectory))
New-Item -ItemType Directory -Force -Path $outputFullPath | Out-Null

$records = @()
foreach ($case in $manifest.cases) {
    if ($case.channel -ne "receipt") {
        Fail "Case '$($case.id)' is not a receipt case."
    }

    $fixtureRef = [string]$case.fixture_ref
    if ([string]::IsNullOrWhiteSpace($fixtureRef)) {
        Fail "Case '$($case.id)' has no fixture_ref."
    }

    $sourcePath = [System.IO.Path]::GetFullPath((Join-Path $manifestFullPath.Substring(0, $manifestFullPath.LastIndexOf([System.IO.Path]::DirectorySeparatorChar)) $fixtureRef))
    if (-not $sourcePath.StartsWith($repositoryRootFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        Fail "Fixture path escapes repository root for case '$($case.id)'."
    }
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        Fail "Fixture missing for case '$($case.id)': $fixtureRef"
    }

    $outputPath = Join-Path $outputFullPath ("{0}.png" -f $case.id)
    & $command.Source --format png --output $outputPath $sourcePath
    if ($LASTEXITCODE -ne 0) {
        Fail "rsvg-convert failed for case '$($case.id)' with exit code $LASTEXITCODE."
    }

    if (-not (Test-Path -LiteralPath $outputPath -PathType Leaf)) {
        Fail "No raster output was produced for case '$($case.id)'."
    }

    $fileInfo = Get-Item -LiteralPath $outputPath
    if ($fileInfo.Length -le 0) {
        Fail "Raster output is empty for case '$($case.id)'."
    }

    $hash = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $relativeOutput = [System.IO.Path]::GetRelativePath($repositoryRootFullPath, $outputPath).Replace("\\", "/")

    $records += [ordered]@{
        case_id = [string]$case.id
        source_fixture_ref = $fixtureRef
        materialized_fixture_ref = $relativeOutput
        format = "png"
        sha256 = $hash
        size_bytes = [int64]$fileInfo.Length
        rasterizer = "rsvg-convert"
        rasterizer_version = $versionOutput
    }
}

$evidencePath = Join-Path $outputFullPath "materialization-evidence.json"
$evidence = [ordered]@{
    schema = "m3-capture-receipt-materialization-v1"
    manifest = $ManifestPath.Replace("\\", "/")
    expected_rsvg_version = $ExpectedRsvgVersion
    detected_rsvg_version = $versionOutput
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    synthetic = $true
    cases = $records
}

$evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $evidencePath -Encoding UTF8
Write-Host "Materialized $($records.Count) receipt fixtures."
Write-Host "Evidence: $evidencePath"
