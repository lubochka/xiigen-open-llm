# Preparation only. No build, Tokenize, Train, Verify, test, or runtime invocation.
$ErrorActionPreference = 'Stop'
$e1Root = 'C:/Projects/llm_mvp_core'
$e1Sources = "$e1Root/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission"
$e1Runner = "$e1Root/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION"
$e1Output = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/results/N60-E1-VERIFIER-ADMISSION-01'
$e1ManifestPath = Join-Path $PSScriptRoot 'N60-E1-source-manifest.json'
$e1TrainPath = Join-Path $PSScriptRoot 'N60-E1-TRAIN.jsonl'
$e1ExamPath = Join-Path $PSScriptRoot 'N60-E1-EXAM.json'

foreach ($e1Destination in @($e1Sources, $e1Runner, $e1Output)) {
    if (Test-Path -LiteralPath $e1Destination) {
        throw "EXISTING_FIXED_DESTINATION: $e1Destination. Preserve it; no overwrite, retry ID, or alternate project."
    }
}
foreach ($e1Input in @($e1ManifestPath, $e1TrainPath, $e1ExamPath)) {
    if (-not (Test-Path -LiteralPath $e1Input -PathType Leaf)) {
        throw "REQUIRED_INPUT_MISSING: $e1Input"
    }
}
$e1Manifest = Get-Content -LiteralPath $e1ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($e1Manifest.source_commit -ne '62bcc8f2368840e22d2ea98ef16d9bd526ad9a39' -or $e1Manifest.files.Count -ne 11) {
    throw 'UNEXPECTED_SOURCE_MANIFEST: pinned source commit and 11 files are required.'
}
$e1Verified = @()
foreach ($e1Entry in $e1Manifest.files) {
    $e1InputFile = Join-Path $e1Root $e1Entry.path
    if (-not (Test-Path -LiteralPath $e1InputFile -PathType Leaf)) {
        throw "SOURCE_MISSING: $e1InputFile"
    }
    $e1ActualHash = (Get-FileHash -LiteralPath $e1InputFile -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($e1ActualHash -ne $e1Entry.sha256) {
        throw "SOURCE_VERSION_DIFFERENT: $($e1Entry.path); expected $($e1Entry.sha256); actual $e1ActualHash. No source replacement or new hash is authorized by this preparation script."
    }
    $e1Verified += [pscustomobject]@{ path = $e1Entry.path; sha256 = $e1ActualHash; bytes = (Get-Item -LiteralPath $e1InputFile).Length }
}

$null = New-Item -ItemType Directory -Path "$e1Sources/reference"
$null = New-Item -ItemType Directory -Path "$e1Sources/fixtures"
foreach ($e1Entry in $e1Manifest.files) {
    $e1CopyPath = Join-Path "$e1Sources/reference" $e1Entry.path
    $null = New-Item -ItemType Directory -Path (Split-Path -Parent $e1CopyPath) -Force
    [System.IO.File]::WriteAllBytes($e1CopyPath, [System.IO.File]::ReadAllBytes((Join-Path $e1Root $e1Entry.path)))
}
Copy-Item -LiteralPath $e1ManifestPath -Destination "$e1Sources/reference/source-manifest.json"
Copy-Item -LiteralPath $e1TrainPath -Destination "$e1Sources/fixtures/N60-E1-TRAIN.jsonl"
Copy-Item -LiteralPath $e1ExamPath -Destination "$e1Sources/fixtures/N60-E1-EXAM.json"
$e1Receipt = [pscustomobject]@{
    status = 'REFERENCE_BYTES_PREPARED_HOST_NOT_IMPLEMENTED'
    source_commit = $e1Manifest.source_commit
    native_calls = 0
    build_calls = 0
    files = $e1Verified
    train_sha256 = (Get-FileHash -LiteralPath $e1TrainPath -Algorithm SHA256).Hash.ToLowerInvariant()
    exam_sha256 = (Get-FileHash -LiteralPath $e1ExamPath -Algorithm SHA256).Hash.ToLowerInvariant()
    next_work_order = 'N60-E1-native-host-preparation.md steps 2-10; do not change project or consumer'
}
$e1ReceiptText = $e1Receipt | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText("$e1Sources/reference/preparation-receipt.json", $e1ReceiptText, [System.Text.UTF8Encoding]::new($false))
Write-Output 'Reference source bytes prepared. Host implementation, compilation, and experiment execution remain unperformed.'
