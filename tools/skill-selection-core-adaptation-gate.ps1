param(
    [Parameter(Mandatory = $true)]
    [string]$ServerLibraryRoot,

    [Parameter(Mandatory = $true)]
    [string]$ModelManifest,

    [Parameter(Mandatory = $true)]
    [string]$TrainingManifest,

    [switch]$RequireCommonAdaptiveSplit,

    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot

function Add-Gap {
    param(
        [System.Collections.ArrayList]$Gaps,
        [string]$Field,
        [string]$Problem,
        [string]$Path = ""
    )

    [void]$Gaps.Add([pscustomobject]@{
        field = $Field
        problem = $Problem
        path = $Path
    })
}

function Read-JsonFile {
    param(
        [string]$Path,
        [System.Collections.ArrayList]$Gaps,
        [string]$Field
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Gap -Gaps $Gaps -Field $Field -Problem "Required JSON file is missing." -Path $Path
        return $null
    }

    try {
        return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    }
    catch {
        Add-Gap -Gaps $Gaps -Field $Field -Problem "Required JSON file is not parseable: $($_.Exception.Message)" -Path $Path
        return $null
    }
}

function Get-StringValue {
    param(
        [object]$Object,
        [string]$Name
    )

    if ($null -eq $Object) {
        return ""
    }

    $prop = $Object.PSObject.Properties[$Name]
    if ($null -eq $prop -or $null -eq $prop.Value) {
        return ""
    }

    return [string]$prop.Value
}

function Test-RequiredProperty {
    param(
        [object]$Object,
        [string]$Name,
        [System.Collections.ArrayList]$Gaps,
        [string]$Field
    )

    $value = Get-StringValue -Object $Object -Name $Name
    if ([string]::IsNullOrWhiteSpace($value)) {
        Add-Gap -Gaps $Gaps -Field $Field -Problem "Required property '$Name' is missing or empty."
        return $false
    }

    return $true
}

function Test-RelativeSafePath {
    param(
        [string]$Value,
        [System.Collections.ArrayList]$Gaps,
        [string]$Field
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }

    if ([System.IO.Path]::IsPathRooted($Value)) {
        Add-Gap -Gaps $Gaps -Field $Field -Problem "Path must be repo-relative, not absolute." -Path $Value
        return $false
    }

    if ($Value -match "^[A-Za-z]:\\") {
        Add-Gap -Gaps $Gaps -Field $Field -Problem "Path contains a Windows absolute path." -Path $Value
        return $false
    }

    return $true
}

function Join-ManifestPath {
    param(
        [string]$ManifestPath,
        [string]$RelativePath
    )

    $base = Split-Path -Parent $ManifestPath
    return Join-Path $base $RelativePath
}

function Test-UnderPath {
    param(
        [string]$Path,
        [string]$Root
    )

    try {
        $resolvedPath = [System.IO.Path]::GetFullPath($Path)
        $resolvedRoot = [System.IO.Path]::GetFullPath($Root)
        return $resolvedPath.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)
    }
    catch {
        return $false
    }
}

$gaps = New-Object System.Collections.ArrayList

$serverLibraryRootExists = Test-Path -LiteralPath $ServerLibraryRoot
$serverLibraryManifestPath = Join-Path $ServerLibraryRoot "server-library-manifest.json"
$serverLibraryManifestExists = Test-Path -LiteralPath $serverLibraryManifestPath

if (-not $serverLibraryRootExists) {
    Add-Gap -Gaps $gaps -Field "server_library_root" -Problem "ServerLibraryRoot does not exist." -Path $ServerLibraryRoot
}

if (-not $serverLibraryManifestExists) {
    Add-Gap -Gaps $gaps -Field "server_library_manifest" -Problem "server-library-manifest.json is missing." -Path $serverLibraryManifestPath
}

$model = Read-JsonFile -Path $ModelManifest -Gaps $gaps -Field "model_manifest"
$training = Read-JsonFile -Path $TrainingManifest -Gaps $gaps -Field "training_manifest"
$serverLibrary = Read-JsonFile -Path $serverLibraryManifestPath -Gaps $gaps -Field "server_library_manifest"

$modelManifestExists = Test-Path -LiteralPath $ModelManifest
$trainingManifestExists = Test-Path -LiteralPath $TrainingManifest

$checkpointPath = $null
$runnerReportPath = $null
$checkpointExists = $false
$runnerReportExists = $false
$modelRepoOwned = $false

if ($null -ne $model) {
    foreach ($name in @("model_id", "domain", "checkpoint_filename", "runner_report_ref", "training_data_ref", "accuracy_at_export", "schema_version")) {
        [void](Test-RequiredProperty -Object $model -Name $name -Gaps $gaps -Field "model_manifest")
    }

    $checkpointFile = Get-StringValue -Object $model -Name "checkpoint_filename"
    if (Test-RelativeSafePath -Value $checkpointFile -Gaps $gaps -Field "checkpoint_filename") {
        $checkpointPath = Join-ManifestPath -ManifestPath $ModelManifest -RelativePath $checkpointFile
        $checkpointExists = Test-Path -LiteralPath $checkpointPath
        if (-not $checkpointExists) {
            Add-Gap -Gaps $gaps -Field "checkpoint_exists" -Problem "Checkpoint file named by model manifest is missing." -Path $checkpointPath
        }
    }

    $runnerReportRef = Get-StringValue -Object $model -Name "runner_report_filename"
    if ([string]::IsNullOrWhiteSpace($runnerReportRef)) {
        $runnerReportRef = Get-StringValue -Object $model -Name "runner_report_ref"
    }

    if (Test-RelativeSafePath -Value $runnerReportRef -Gaps $gaps -Field "runner_report_ref") {
        $runnerReportPath = Join-ManifestPath -ManifestPath $ModelManifest -RelativePath $runnerReportRef
        $runnerReportExists = Test-Path -LiteralPath $runnerReportPath
        if (-not $runnerReportExists) {
            Add-Gap -Gaps $gaps -Field "runner_report_exists" -Problem "Runner report file named by model manifest is missing." -Path $runnerReportPath
        }
    }

    $modelId = Get-StringValue -Object $model -Name "model_id"
    $domain = Get-StringValue -Object $model -Name "domain"
    if ($modelId -eq "skill-selection-v1" -and $domain -eq "SkillSelectionDomain" -and (Test-UnderPath -Path $ModelManifest -Root (Join-Path $RepoRoot "models\skill-selection"))) {
        $modelRepoOwned = $true
    }
    else {
        Add-Gap -Gaps $gaps -Field "model_repo_ownership" -Problem "Model manifest is not the expected repo-owned SkillSelection common asset." -Path $ModelManifest
    }

    $semanticQuality = $model.PSObject.Properties["semantic_quality_claimed"]
    if ($null -ne $semanticQuality -and [bool]$semanticQuality.Value) {
        Add-Gap -Gaps $gaps -Field "semantic_quality_claimed" -Problem "Gate must not accept semantic quality claims as common/adaptive split proof."
    }
}

$corpusSnapshotPath = $null
$corpusPointerPath = $null
$corpusSnapshotExists = $false
$corpusPointerExists = $false
$corpusHashMatches = $false
$trainingRepoOwned = $false
$safeCorpusCommitted = $false
$noPrivateTrainingFlags = $false

if ($null -ne $training) {
    foreach ($name in @("schema_version", "model_id", "domain", "corpus_sha256", "training_pair_count", "raw_private_corpus_committed", "private_path_recorded")) {
        [void](Test-RequiredProperty -Object $training -Name $name -Gaps $gaps -Field "training_manifest")
    }

    $snapshotFile = Get-StringValue -Object $training -Name "corpus_snapshot_filename"
    $pointerFile = Get-StringValue -Object $training -Name "corpus_pointer_filename"

    if (-not [string]::IsNullOrWhiteSpace($snapshotFile) -and (Test-RelativeSafePath -Value $snapshotFile -Gaps $gaps -Field "corpus_snapshot_filename")) {
        $corpusSnapshotPath = Join-ManifestPath -ManifestPath $TrainingManifest -RelativePath $snapshotFile
        $corpusSnapshotExists = Test-Path -LiteralPath $corpusSnapshotPath
    }

    if (-not [string]::IsNullOrWhiteSpace($pointerFile) -and (Test-RelativeSafePath -Value $pointerFile -Gaps $gaps -Field "corpus_pointer_filename")) {
        $corpusPointerPath = Join-ManifestPath -ManifestPath $TrainingManifest -RelativePath $pointerFile
        $corpusPointerExists = Test-Path -LiteralPath $corpusPointerPath
    }

    if (-not $corpusSnapshotExists -and -not $corpusPointerExists) {
        Add-Gap -Gaps $gaps -Field "corpus_snapshot_or_pointer" -Problem "Neither corpus snapshot nor corpus pointer exists."
    }

    if ($corpusSnapshotExists) {
        $expectedHash = Get-StringValue -Object $training -Name "corpus_sha256"
        $actualHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $corpusSnapshotPath).Hash.ToLowerInvariant()
        $corpusHashMatches = ($actualHash -eq $expectedHash.ToLowerInvariant())
        if (-not $corpusHashMatches) {
            Add-Gap -Gaps $gaps -Field "corpus_sha256" -Problem "Corpus snapshot SHA256 does not match training manifest." -Path $corpusSnapshotPath
        }
    }

    $rawPrivateCommitted = $false
    if ($null -ne $training.PSObject.Properties["raw_private_corpus_committed"]) {
        $rawPrivateCommitted = [bool]$training.raw_private_corpus_committed
    }
    $privatePathRecorded = $false
    if ($null -ne $training.PSObject.Properties["private_path_recorded"]) {
        $privatePathRecorded = [bool]$training.private_path_recorded
    }
    $safeSnapshot = $false
    if ($null -ne $training.PSObject.Properties["safe_corpus_snapshot_committed"]) {
        $safeSnapshot = [bool]$training.safe_corpus_snapshot_committed
    }

    $safeCorpusCommitted = $safeSnapshot
    $noPrivateTrainingFlags = (-not $rawPrivateCommitted -and -not $privatePathRecorded)

    if (-not $safeCorpusCommitted) {
        Add-Gap -Gaps $gaps -Field "safe_corpus_snapshot_committed" -Problem "Training manifest does not record a safe committed corpus snapshot."
    }
    if (-not $noPrivateTrainingFlags) {
        Add-Gap -Gaps $gaps -Field "private_corpus_flags" -Problem "Training manifest records private corpus or private path leakage."
    }

    $trainingRepoOwned = Test-UnderPath -Path $TrainingManifest -Root (Join-Path $RepoRoot "training-data\skill-selection")
    if (-not $trainingRepoOwned) {
        Add-Gap -Gaps $gaps -Field "training_repo_ownership" -Problem "Training manifest is not under the repo-owned skill-selection training-data root." -Path $TrainingManifest
    }
}

$commonAssetsRepoOwned = $false
$adaptiveStateSeparate = $false
$noPrivatePaths = ($gaps | Where-Object { $_.field -in @("checkpoint_filename", "runner_report_ref", "corpus_snapshot_filename", "corpus_pointer_filename", "private_corpus_flags") }).Count -eq 0
$serverLibraryNoExternalAi = $false
$serverLibraryNoPrivateCorpus = $false

if ($null -ne $serverLibrary) {
    if ($serverLibrary.PSObject.Properties["common_assets_repo_owned"]) {
        $commonAssetsRepoOwned = [bool]$serverLibrary.common_assets_repo_owned
    }
    if ($serverLibrary.PSObject.Properties["adaptive_state_separate_from_common_assets"]) {
        $adaptiveStateSeparate = [bool]$serverLibrary.adaptive_state_separate_from_common_assets
    }
    if ($serverLibrary.PSObject.Properties["external_ai_allowed"]) {
        $serverLibraryNoExternalAi = -not [bool]$serverLibrary.external_ai_allowed
    }
    if ($serverLibrary.PSObject.Properties["private_corpus_committed"]) {
        $serverLibraryNoPrivateCorpus = -not [bool]$serverLibrary.private_corpus_committed
    }

    if (-not $commonAssetsRepoOwned) {
        Add-Gap -Gaps $gaps -Field "common_assets_repo_owned" -Problem "Server library manifest does not record common_assets_repo_owned=true."
    }
    if (-not $adaptiveStateSeparate) {
        Add-Gap -Gaps $gaps -Field "adaptive_state_separate" -Problem "Server library manifest does not record adaptive state as separate from common assets."
    }
    if (-not $serverLibraryNoExternalAi) {
        Add-Gap -Gaps $gaps -Field "external_ai_policy" -Problem "Server library manifest must forbid external AI."
    }
    if (-not $serverLibraryNoPrivateCorpus) {
        Add-Gap -Gaps $gaps -Field "private_corpus_policy" -Problem "Server library manifest must record no private corpus committed."
    }

    $adaptiveRoot = Get-StringValue -Object $serverLibrary -Name "adaptive_state_root_example"
    if ($adaptiveRoot -match "models/" -or $adaptiveRoot -match "training-data/" -or $adaptiveRoot -match "skills/server-library" -or $adaptiveRoot -match "xiigen-smart-skills") {
        Add-Gap -Gaps $gaps -Field "adaptive_state_root_example" -Problem "Adaptive state example must not point inside common asset or host package roots." -Path $adaptiveRoot
        $adaptiveStateSeparate = $false
    }
}

if ($RequireCommonAdaptiveSplit) {
    if (-not $modelRepoOwned -or -not $trainingRepoOwned -or -not $commonAssetsRepoOwned -or -not $adaptiveStateSeparate) {
        Add-Gap -Gaps $gaps -Field "common_adaptive_split" -Problem "RequireCommonAdaptiveSplit failed: common assets and adaptive state separation are not both proven."
    }
}

$passed = (
    $serverLibraryRootExists -and
    $serverLibraryManifestExists -and
    $modelManifestExists -and
    $trainingManifestExists -and
    $checkpointExists -and
    $runnerReportExists -and
    ($corpusSnapshotExists -or $corpusPointerExists) -and
    $corpusHashMatches -and
    $noPrivatePaths -and
    $modelRepoOwned -and
    $trainingRepoOwned -and
    $commonAssetsRepoOwned -and
    $adaptiveStateSeparate -and
    $serverLibraryNoExternalAi -and
    $serverLibraryNoPrivateCorpus -and
    $gaps.Count -eq 0
)

$result = [pscustomobject]@{
    passed = $passed
    validation_only = $true
    common_adaptive_split_proven = $passed
    server_library_root_exists = $serverLibraryRootExists
    server_library_manifest_exists = $serverLibraryManifestExists
    model_manifest_exists = $modelManifestExists
    training_manifest_exists = $trainingManifestExists
    checkpoint_exists = $checkpointExists
    runner_report_exists = $runnerReportExists
    corpus_snapshot_exists = $corpusSnapshotExists
    corpus_pointer_exists = $corpusPointerExists
    corpus_hash_matches = $corpusHashMatches
    no_private_paths = $noPrivatePaths
    common_assets_repo_owned = ($modelRepoOwned -and $trainingRepoOwned -and $commonAssetsRepoOwned)
    adaptive_state_separate = $adaptiveStateSeparate
    semantic_quality_claimed = $false
    deterministic_static_masquerade_claimed = $false
    phase40_complete = $false
    gaps = @($gaps)
}

$json = $result | ConvertTo-Json -Depth 12
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outDir = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($outDir)) {
        New-Item -ItemType Directory -Force -Path $outDir | Out-Null
    }
    Set-Content -LiteralPath $OutputPath -Value $json -Encoding UTF8
}

Write-Output $json

if ($passed) {
    exit 0
}

exit 1
