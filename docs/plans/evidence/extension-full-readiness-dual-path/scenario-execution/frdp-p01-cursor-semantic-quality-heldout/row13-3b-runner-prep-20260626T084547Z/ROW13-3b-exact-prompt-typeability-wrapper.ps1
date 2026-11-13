param(
    [switch] $Run,
    [string] $SourceRunner = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\ROW12-cursor-panel-visibility-runner.ps1",
    [string] $CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe",
    [string] $ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills",
    [string] $EvidenceBase = $PSScriptRoot,
    [int] $HostReadyTimeoutSec = 90,
    [int] $WindowReadyTimeoutSec = 45,
    [int] $PanelSettleSec = 4,
    [int] $PanelHoldMs = 120000,
    [int] $SubmitResultWaitMs = 20000,
    [int] $SubmitResultCdpTimeoutSec = 30,
    [switch] $KeepCursorOpen
)

$ErrorActionPreference = "Stop"

$ExactPrompt = "The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim."
$ExpectedPromptSha256 = "eca453b1fd9e7c925a21287bd52d3b8c907405efac387d7ebb2d8b6db2e60dd4"
$PatchedRunnerName = "ROW12-cursor-panel-visibility-runner.row13-3b-exact-prompt.ps1"
$PatchedRunnerPath = Join-Path $PSScriptRoot $PatchedRunnerName
$WrapperMetadataPath = Join-Path $PSScriptRoot "row13-3b-wrapper-prep-metadata.json"

function Get-Sha256Hex {
    param([AllowNull()] [string] $Value)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes([string] $Value)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

function ConvertTo-NativeArgument {
    param([AllowNull()] [string] $Value)

    if ($null -eq $Value) {
        return '""'
    }

    if ($Value -notmatch '[\s"]') {
        return $Value
    }

    $escaped = $Value.Replace('\', '\\').Replace('"', '\"')
    return '"' + $escaped + '"'
}

function Join-NativeArgumentList {
    param([string[]] $Arguments)
    return (($Arguments | ForEach-Object { ConvertTo-NativeArgument -Value $_ }) -join " ")
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value
    )

    $dir = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    $Value | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding UTF8
}

$actualPromptSha256 = Get-Sha256Hex -Value $ExactPrompt
if ($actualPromptSha256 -ne $ExpectedPromptSha256) {
    throw "Row13.3b exact prompt hash mismatch. Expected $ExpectedPromptSha256 but computed $actualPromptSha256."
}

if (-not (Test-Path -LiteralPath $SourceRunner)) {
    throw "Source Row12 runner not found: $SourceRunner"
}

$sourceText = Get-Content -Raw -LiteralPath $SourceRunner
$oldAssignment = '$typeabilityProbeText = if ($TypeabilityProofOnly.IsPresent) { "row12-ui-typeability-probe-$stamp" } else { $null }'
$promptLiteral = "'" + $ExactPrompt.Replace("'", "''") + "'"
$newAssignment = '$typeabilityProbeText = if ($TypeabilityProofOnly.IsPresent) { ' + $promptLiteral + ' } else { $null }'
$patchedText = $sourceText.Replace($oldAssignment, $newAssignment)
if ($patchedText -eq $sourceText) {
    throw "Could not find the Row12 TypeabilityProofOnly probe assignment to patch."
}

Set-Content -LiteralPath $PatchedRunnerPath -Value $patchedText -Encoding UTF8

$runnerArgs = @(
    "-ExecutionPolicy", "Bypass",
    "-File", $PatchedRunnerPath,
    "-CursorExe", $CursorExe,
    "-ExtensionRoot", $ExtensionRoot,
    "-EvidenceBase", $EvidenceBase,
    "-HostReadyTimeoutSec", [string] $HostReadyTimeoutSec,
    "-WindowReadyTimeoutSec", [string] $WindowReadyTimeoutSec,
    "-PanelSettleSec", [string] $PanelSettleSec,
    "-PanelHoldMs", [string] $PanelHoldMs,
    "-SubmitResultWaitMs", [string] $SubmitResultWaitMs,
    "-SubmitResultCdpTimeoutSec", [string] $SubmitResultCdpTimeoutSec,
    "-TypeabilityProofOnly"
)
if ($KeepCursorOpen.IsPresent) {
    $runnerArgs += "-KeepCursorOpen"
}

$wrapperCommand = "powershell -ExecutionPolicy Bypass -File " + (ConvertTo-NativeArgument -Value $PSCommandPath) + " -Run"
$runnerCommand = "powershell " + (Join-NativeArgumentList -Arguments $runnerArgs)

$metadata = [ordered] @{
    schema_version = "row13.3b.exact_prompt_wrapper_prep.v1"
    status = "ROW13_3B_RUNNER_PREP_PASS"
    created_utc = (Get-Date).ToUniversalTime().ToString("o")
    source_runner = $SourceRunner
    patched_runner = $PatchedRunnerPath
    patch_scope = "evidence-local copy only; production Row12 runner is not modified"
    exact_prompt_override = $true
    TypeabilityProofOnly = $true
    do_not_launch_Cursor = -not $Run.IsPresent
    exact_prompt = $ExactPrompt
    exact_prompt_sha256 = $actualPromptSha256
    next_Row13_3b_command = $wrapperCommand
    patched_runner_command = $runnerCommand
    max_expected_run_time = "about 5-6 minutes with PanelHoldMs=120000, HostReadyTimeoutSec=90, WindowReadyTimeoutSec=45, and CDP timeout 30s"
    cleanup_expectations = "Without -KeepCursorOpen, the copied Row12 runner closes launched Cursor processes in its finally cleanup. Evidence files remain under EvidenceBase. With -KeepCursorOpen, the operator must close the launched Cursor window manually."
    forbidden_now = @(
        "do not launch Cursor during prep",
        "do not click submit",
        "do not mutate checkpoints/locators/models",
        "do not edit production extension source",
        "do not perform git actions"
    )
}
Write-JsonFile -Path $WrapperMetadataPath -Value $metadata

if (-not $Run.IsPresent) {
    $metadata | ConvertTo-Json -Depth 10
    exit 0
}

& powershell @runnerArgs
exit $LASTEXITCODE
