param(
    [string]$ManifestPath = "docs/00-Governance/benchmarks/capture/receipts-v1.json",
    [string]$RepositoryRoot = ".",
    [string]$OutputDirectory = "artifacts/benchmarks/capture/receipts",
    [Parameter(Mandatory = $true)]
    [string]$ExpectedRsvgVersion,
    [Parameter(Mandatory = $true)]
    [string]$ExpectedMagickVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Fail([string]$Message) {
    throw "DG-023 receipt materialization failed: $Message"
}

$rsvg = Get-Command rsvg-convert -ErrorAction SilentlyContinue
if ($null -eq $rsvg) { Fail "rsvg-convert was not found on PATH." }

$rsvgVersion = (& $rsvg.Source --version 2>&1 | Out-String).Trim()
if ([string]::IsNullOrWhiteSpace($rsvgVersion)) { Fail "Unable to determine rsvg-convert version." }
if ($rsvgVersion -notmatch [regex]::Escape($ExpectedRsvgVersion)) {
    Fail "Expected rsvg-convert version '$ExpectedRsvgVersion', but detected '$rsvgVersion'."
}

$magick = Get-Command magick -ErrorAction SilentlyContinue
if ($null -eq $magick) { Fail "ImageMagick 'magick' was not found on PATH." }

$magickVersion = (& $magick.Source --version 2>&1 | Out-String).Trim()
if ([string]::IsNullOrWhiteSpace($magickVersion)) { Fail "Unable to determine ImageMagick version." }
if ($magickVersion -notmatch [regex]::Escape($ExpectedMagickVersion)) {
    Fail "Expected ImageMagick version '$ExpectedMagickVersion', but detected '$magickVersion'."
}

$repositoryRootFullPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
$manifestFullPath = [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $ManifestPath))
if (-not (Test-Path -LiteralPath $manifestFullPath -PathType Leaf)) {
    Fail "Manifest not found: $manifestFullPath"
}

$manifest = Get-Content -LiteralPath $manifestFullPath -Raw | ConvertFrom-Json
if ($manifest.schema_version -ne "m3-capture-receipt-benchmark-v1") {
    Fail "Unexpected receipt manifest schema '$($manifest.schema_version)'."
}

$manifestDirectory = Split-Path -Parent $manifestFullPath
$outputFullPath = [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $OutputDirectory))
New-Item -ItemType Directory -Force -Path $outputFullPath | Out-Null

function Apply-Degradation([string]$InputPath, [string]$OutputPath, [object[]]$Degradations) {
    $args = @($InputPath)

    foreach ($degradation in $Degradations) {
        switch ([string]$degradation) {
            "blur" {
                $args += @("-blur", "0x2.0")
            }
            "crop" {
                # Remove the top 12% so merchant/header content is partially or fully absent
                # while preserving the lower receipt content, including totals.
                $args += @("-gravity", "North", "-crop", "100%x88%+0+0", "+repage")
            }
            "skew" {
                $args += @("-background", "white", "-shear", "3x0", "+repage")
            }
            "glare" {
                # Deterministic bright elliptical highlight near the upper-middle receipt area.
                $args += @(
                    "-alpha", "on",
                    "-fill", "rgba(255,255,255,0.58)",
                    "-stroke", "none",
                    "-draw", "ellipse 55% 38% 24% 13% 0,360"
                )
            }
            "low_contrast" {
                $args += @("-brightness-contrast", "0x-45")
            }
            "contradictory_total" {
                # The contradiction is semantic content already present in the source fixture;
                # no image mutation is required.
            }
            default {
                Fail "Unsupported declared degradation '$degradation'."
            }
        }
    }

    $args += $OutputPath
    & $magick.Source @args
    if ($LASTEXITCODE -ne 0) {
        Fail "ImageMagick failed for '$OutputPath' with exit code $LASTEXITCODE."
    }
}

$records = @()
foreach ($case in $manifest.cases) {
    $fixtureRef = [string]$case.fixture_ref
    if ([string]::IsNullOrWhiteSpace($fixtureRef)) {
        Fail "Case '$($case.id)' has no fixture_ref."
    }

    $sourcePath = [System.IO.Path]::GetFullPath((Join-Path $manifestDirectory $fixtureRef))
    if (-not $sourcePath.StartsWith($repositoryRootFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        Fail "Fixture path escapes repository root for case '$($case.id)'."
    }
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        Fail "Fixture missing for case '$($case.id)': $fixtureRef"
    }

    $intermediatePath = Join-Path $outputFullPath ("{0}.source.png" -f $case.id)
    $outputPath = Join-Path $outputFullPath ("{0}.png" -f $case.id)

    & $rsvg.Source --format png --output $intermediatePath $sourcePath
    if ($LASTEXITCODE -ne 0) {
        Fail "rsvg-convert failed for case '$($case.id)' with exit code $LASTEXITCODE."
    }

    if (-not (Test-Path -LiteralPath $intermediatePath -PathType Leaf)) {
        Fail "No raster output was produced for case '$($case.id)'."
    }

    $degradations = @($case.degradation)
    Apply-Degradation -InputPath $intermediatePath -OutputPath $outputPath -Degradations $degradations

    Remove-Item -LiteralPath $intermediatePath -Force

    if (-not (Test-Path -LiteralPath $outputPath -PathType Leaf)) {
        Fail "No final raster output was produced for case '$($case.id)'."
    }

    $fileInfo = Get-Item -LiteralPath $outputPath
    if ($fileInfo.Length -le 0) {
        Fail "Final raster output is empty for case '$($case.id)'."
    }

    $hash = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $relativeOutput = [System.IO.Path]::GetRelativePath($repositoryRootFullPath, $outputPath).Replace("\", "/")

    $records += [ordered]@{
        case_id = [string]$case.id
        source_fixture_ref = $fixtureRef
        materialized_fixture_ref = $relativeOutput
        format = "png"
        sha256 = $hash
        size_bytes = [int64]$fileInfo.Length
        rasterizer = "rsvg-convert"
        rasterizer_version = $rsvgVersion
        degradation_tool = "ImageMagick"
        degradation_tool_version = $magickVersion
        applied_degradation = $degradations
        synthetic = $true
    }
}

$evidencePath = Join-Path $outputFullPath "materialization-evidence.json"
$evidence = [ordered]@{
    schema = "m3-capture-receipt-materialization-v2"
    manifest = $ManifestPath.Replace("\", "/")
    expected_rsvg_version = $ExpectedRsvgVersion
    detected_rsvg_version = $rsvgVersion
    expected_magick_version = $ExpectedMagickVersion
    detected_magick_version = $magickVersion
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    synthetic = $true
    note = "SVG fixtures are rasterized with pinned rsvg-convert and then transformed with pinned ImageMagick according to the manifest degradation list. Semantic contradiction is source content and is not image-generated."
    cases = $records
}

$evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $evidencePath -Encoding UTF8
Write-Host "Materialized $($records.Count) receipt fixtures."
Write-Host "Evidence: $evidencePath"
