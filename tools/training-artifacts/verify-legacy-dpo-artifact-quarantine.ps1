[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [string]$QuarantineRoot = '',
    [switch]$Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ExpectedSchema = 'xiigen-legacy-dpo-artifact-quarantine-manifest-v1'
$ExpectedStatus = 'BASELINE_ONLY_COLD_START_FOREST'

function Normalize-RelativePath {
    param([string]$Path)

    return $Path.Replace([char]'\', [char]'/')
}

function Convert-ToRepoPath {
    param([string]$RelativePath)

    return Join-Path $RepoRoot ($RelativePath.Replace([char]'/', [char]'\'))
}

function Get-RelativePath {
    param([System.IO.FileInfo]$File)

    return Normalize-RelativePath -Path ([System.IO.Path]::GetRelativePath(
        $RepoRoot,
        $File.FullName))
}

function Get-NonEmptyLineCount {
    param([string]$Path)

    $count = 0
    foreach ($line in Get-Content -LiteralPath $Path -Encoding UTF8) {
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $count++
        }
    }

    return $count
}

function Get-CanonicalTextSha256 {
    param([string]$Path)

    $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $canonicalText = $text.Replace("`r`n", "`n").Replace("`r", "`n")
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($canonicalText)
    return [System.BitConverter]::ToString(
        [System.Security.Cryptography.SHA256]::HashData($bytes)
    ).Replace('-', '').ToLowerInvariant()
}

function Read-Jsonl {
    param(
        [string]$Path,
        [string]$HumanName,
        [System.Collections.Generic.List[string]]$Problems
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        $Problems.Add("Карантин не может быть проверен: отсутствует файл $HumanName ($Path).")
        return @()
    }

    $records = New-Object System.Collections.Generic.List[object]
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $Path -Encoding UTF8) {
        $lineNumber++
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        try {
            $records.Add(($line | ConvertFrom-Json))
        }
        catch {
            $Problems.Add("Карантин не может быть проверен: строка $lineNumber файла $HumanName не является JSON.")
        }
    }

    return $records.ToArray()
}

function Get-GitTrackedSet {
    $tracked = git -C $RepoRoot ls-files
    $set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($path in $tracked) {
        [void]$set.Add((Normalize-RelativePath -Path $path))
    }

    return $set
}

function Add-ProblemForUncoveredArtifact {
    param(
        [System.Collections.Generic.List[string]]$Problems,
        [string]$RelativePath,
        [bool]$GitTracked
    )

    $kind = if ($GitTracked) { 'отслеживаемый git' } else { 'локальный' }
    $Problems.Add(
        "Исторический артефакт присутствует в репозитории, но не записан в карантин: $RelativePath ($kind).")
}

function Assert-ClosedFlags {
    param(
        [object]$Record,
        [string]$RecordName,
        [System.Collections.Generic.List[string]]$Problems
    )

    if ($Record.artifact_status -ne $ExpectedStatus) {
        $Problems.Add("У записи карантина неверный статус хранения: $RecordName.")
    }

    if ($Record.training_allowed -ne $false) {
        $Problems.Add("Запись карантина ошибочно разрешает обучение: $RecordName.")
    }

    if ($Record.semantic_publication_allowed -ne $false) {
        $Problems.Add("Запись карантина ошибочно разрешает смысловую публикацию: $RecordName.")
    }

    if ([string]::IsNullOrWhiteSpace([string]$Record.human_reason)) {
        $Problems.Add("Запись карантина не объясняет человеку причину хранения: $RecordName.")
    }

    $rebuildRule = [string]$Record.rebuild_rule
    if ($rebuildRule -notmatch 'DpoTrainerPairAdmission' -or
        $rebuildRule -notmatch 'authorship' -or
        $rebuildRule -notmatch 'licens') {
        $Problems.Add(
            "Запись карантина не называет путь пересборки через допуск пары, авторство и лицензионную проверку: $RecordName.")
    }
}

function Test-FileRecord {
    param(
        [object]$Record,
        [string]$RelativePath,
        [System.Collections.Generic.HashSet[string]]$TrackedSet,
        [System.Collections.Generic.List[string]]$Problems
    )

    Assert-ClosedFlags -Record $Record -RecordName $RelativePath -Problems $Problems
    $absolutePath = Convert-ToRepoPath -RelativePath $RelativePath
    $recordIsTracked = [bool]$Record.git_tracked
    $actualIsTracked = $TrackedSet.Contains($RelativePath)

    if ($recordIsTracked -ne $actualIsTracked) {
        $Problems.Add("Запись карантина неверно помечает git-статус файла: $RelativePath.")
    }

    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        if ($recordIsTracked) {
            $Problems.Add("Отслеживаемый git артефакт записан в карантин, но отсутствует на диске: $RelativePath.")
        }

        return
    }

    # Jsonl quarantine entries are text artifacts. Their hash is normalized to LF
    # so the same git content verifies identically from Windows and Linux checkouts.
    $actualHash = Get-CanonicalTextSha256 -Path $absolutePath
    if ($actualHash -ne [string]$Record.sha256) {
        $Problems.Add("Хеш исторического артефакта изменился после записи карантина: $RelativePath.")
    }

    $actualRows = Get-NonEmptyLineCount -Path $absolutePath
    if ($actualRows -ne [int]$Record.non_empty_jsonl_rows) {
        $Problems.Add("Число непустых строк исторического артефакта изменилось после записи карантина: $RelativePath.")
    }
}

function Get-PresentThreeFieldDatasets {
    $root = Join-Path $RepoRoot 'experiments-v2\INTERNAL-TRAIN-READY-V191\results'
    if (-not (Test-Path -LiteralPath $root)) {
        return @()
    }

    return @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter 'dpo-pairs-normalized.jsonl' |
        ForEach-Object { Get-RelativePath -File $_ } |
        Sort-Object)
}

function Get-PresentSeedPairFiles {
    return @(Get-RepositoryFiles -Root $RepoRoot -SearchPattern 'dpo-pairs.jsonl' |
        Where-Object { (Normalize-RelativePath -Path $_) -like '*/dpo-runs/*' } |
        ForEach-Object { Normalize-RelativePath -Path ([System.IO.Path]::GetRelativePath($RepoRoot, $_)) } |
        Sort-Object)
}

function Test-IsRuntimeDirectory {
    param([string]$DirectoryPath)

    $trimmed = $DirectoryPath.TrimEnd([char]'\', [char]'/')
    $name = Split-Path -Leaf $trimmed
    $relative = Normalize-RelativePath -Path ([System.IO.Path]::GetRelativePath($RepoRoot, $trimmed))

    # Only repository-root runtime families are output from current runs. Nested
    # Results/TestResults/evidence folders under docs, artifacts, or training-data
    # remain visible so a real historical DPO file cannot disappear from manifest.
    if ((Test-IsRootRuntimeFamily -RelativePath $relative -RootName 'evidence') -or
        (Test-IsRootRuntimeFamily -RelativePath $relative -RootName 'Results') -or
        (Test-IsRootRuntimeFamily -RelativePath $relative -RootName 'TestResults')) {
        return $true
    }

    return $name -in @(
        '.git',
        '.tmp',
        '.clone',
        '.codex-test-temp',
        '.vs',
        'bin',
        'node_modules',
        'obj',
        'packages')
}

function Test-IsRootRuntimeFamily {
    param(
        [string]$RelativePath,
        [string]$RootName
    )

    return $RelativePath -eq $RootName -or
        $RelativePath.StartsWith("$RootName/", [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-SafeChildDirectories {
    param([string]$DirectoryPath)

    try {
        return @(Get-ChildItem -LiteralPath $DirectoryPath -Directory -Force -ErrorAction Stop)
    }
    catch [System.UnauthorizedAccessException], [System.IO.DirectoryNotFoundException], [System.IO.IOException] {
        Add-RepositoryScanProblem -DirectoryPath $DirectoryPath -Exception $_.Exception
        return @()
    }
}

function Get-SafeFiles {
    param(
        [string]$DirectoryPath,
        [string]$SearchPattern
    )

    try {
        return @(Get-ChildItem -LiteralPath $DirectoryPath -File -Force -Filter $SearchPattern -ErrorAction Stop)
    }
    catch [System.UnauthorizedAccessException], [System.IO.DirectoryNotFoundException], [System.IO.IOException] {
        Add-RepositoryScanProblem -DirectoryPath $DirectoryPath -Exception $_.Exception
        return @()
    }
}

function Add-RepositoryScanProblem {
    param(
        [string]$DirectoryPath,
        [System.Exception]$Exception
    )

    $relative = Normalize-RelativePath -Path ([System.IO.Path]::GetRelativePath($RepoRoot, $DirectoryPath))
    $Problems.Add(
        "Карантин не может проверить обычный каталог проекта: $relative. " +
        "Проверка остановлена, чтобы не скрыть незарегистрированный исторический DPO-файл. " +
        "Ошибка чтения: $($Exception.GetType().Name).")
}

function Get-RepositoryFiles {
    param(
        [string]$Root,
        [string]$SearchPattern
    )

    $pending = [System.Collections.Generic.Stack[string]]::new()
    $pending.Push([System.IO.Path]::GetFullPath($Root))

    while ($pending.Count -gt 0) {
        $current = $pending.Pop()
        if (Test-IsRuntimeDirectory -DirectoryPath $current) {
            continue
        }

        foreach ($file in Get-SafeFiles -DirectoryPath $current -SearchPattern $SearchPattern) {
            $file.FullName
        }

        foreach ($child in Get-SafeChildDirectories -DirectoryPath $current) {
            if (-not (Test-IsRuntimeDirectory -DirectoryPath $child.FullName)) {
                $pending.Push($child.FullName)
            }
        }
    }
}

if ([string]::IsNullOrWhiteSpace($QuarantineRoot)) {
    $QuarantineRoot = Join-Path $RepoRoot 'training-data\legacy-dpo-artifact-quarantine\2026-08-30'
}

$Problems = [System.Collections.Generic.List[string]]::new()
$manifestPath = Join-Path $QuarantineRoot 'manifest.json'
$legacyPath = Join-Path $QuarantineRoot 'legacy-three-field-datasets.jsonl'
$seedPath = Join-Path $QuarantineRoot 'seed-dpo-run-directories.jsonl'

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    $Problems.Add("Карантин не может быть проверен: отсутствует главный manifest.json.")
}

$manifest = $null
if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
    try {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        $Problems.Add("Карантин не может быть проверен: главный manifest.json не является JSON.")
    }
}

if ($manifest -ne $null) {
    if ($manifest.schema -ne $ExpectedSchema) {
        $Problems.Add("Главный манифест карантина имеет неверную схему.")
    }

    if ($manifest.status -ne $ExpectedStatus) {
        $Problems.Add("Главный манифест карантина имеет неверный статус хранения.")
    }

    if ($manifest.training_allowed -ne $false) {
        $Problems.Add("Главный манифест карантина ошибочно разрешает обучение.")
    }

    if ($manifest.semantic_publication_allowed -ne $false) {
        $Problems.Add("Главный манифест карантина ошибочно разрешает смысловую публикацию.")
    }

    if ($manifest.files_deleted -ne $false) {
        $Problems.Add("Главный манифест карантина заявляет удаление файлов.")
    }

    if ($manifest.historical_artifacts_rewritten -ne $false) {
        $Problems.Add("Главный манифест карантина заявляет переписывание исторических артефактов.")
    }
}

$trackedSet = Get-GitTrackedSet
$legacyRecords = Read-Jsonl -Path $legacyPath -HumanName 'legacy-three-field-datasets.jsonl' -Problems $Problems
$seedRecords = Read-Jsonl -Path $seedPath -HumanName 'seed-dpo-run-directories.jsonl' -Problems $Problems
$legacyByPath = @{}
$seedByPath = @{}

foreach ($record in $legacyRecords) {
    $relativePath = Normalize-RelativePath -Path ([string]$record.relative_path)
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        $Problems.Add("Запись карантина старого набора не содержит путь.")
        continue
    }

    if ($legacyByPath.ContainsKey($relativePath)) {
        $Problems.Add("Две записи карантина описывают один старый набор: $relativePath.")
        continue
    }

    $legacyByPath[$relativePath] = $record
    Test-FileRecord -Record $record -RelativePath $relativePath -TrackedSet $trackedSet -Problems $Problems
}

foreach ($record in $seedRecords) {
    $relativePath = Normalize-RelativePath -Path ([string]$record.pair_file)
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        $Problems.Add("Запись карантина посевного прогона не содержит путь к dpo-pairs.jsonl.")
        continue
    }

    if ($seedByPath.ContainsKey($relativePath)) {
        $Problems.Add("Две записи карантина описывают один посевный файл: $relativePath.")
        continue
    }

    $seedByPath[$relativePath] = $record
    Test-FileRecord -Record $record -RelativePath $relativePath -TrackedSet $trackedSet -Problems $Problems

    $heldoutPath = Normalize-RelativePath -Path ([string]$record.heldout_pair_file)
    if (-not [string]::IsNullOrWhiteSpace($heldoutPath)) {
        $heldoutAbsolutePath = Convert-ToRepoPath -RelativePath $heldoutPath
        if (Test-Path -LiteralPath $heldoutAbsolutePath -PathType Leaf) {
            $heldoutHash = Get-CanonicalTextSha256 -Path $heldoutAbsolutePath
            if ($heldoutHash -ne [string]$record.heldout_sha256) {
                $Problems.Add("Хеш отложенного файла посевного прогона изменился после записи карантина: $heldoutPath.")
            }

            $heldoutRows = Get-NonEmptyLineCount -Path $heldoutAbsolutePath
            if ($heldoutRows -ne [int]$record.heldout_non_empty_jsonl_rows) {
                $Problems.Add("Число непустых строк отложенного файла посевного прогона изменилось: $heldoutPath.")
            }
        }
        elseif ([bool]$record.heldout_git_tracked) {
            $Problems.Add("Отложенный git-файл посевного прогона записан в карантин, но отсутствует на диске: $heldoutPath.")
        }
    }
}

foreach ($relativePath in Get-PresentThreeFieldDatasets) {
    if (-not $legacyByPath.ContainsKey($relativePath)) {
        Add-ProblemForUncoveredArtifact -Problems $Problems -RelativePath $relativePath -GitTracked $trackedSet.Contains($relativePath)
    }
}

foreach ($relativePath in Get-PresentSeedPairFiles) {
    if (-not $seedByPath.ContainsKey($relativePath)) {
        Add-ProblemForUncoveredArtifact -Problems $Problems -RelativePath $relativePath -GitTracked $trackedSet.Contains($relativePath)
    }
}

if ($manifest -ne $null -and $manifest.scopes -ne $null) {
    $legacyScope = $manifest.scopes.legacy_three_field_datasets
    $seedScope = $manifest.scopes.seed_dpo_run_directories
    if ([int]$legacyScope.catalog_records -ne $legacyRecords.Count) {
        $Problems.Add("Главный манифест не совпадает с числом записей старых трехполевых наборов.")
    }

    if ([int]$seedScope.catalog_records -ne $seedRecords.Count) {
        $Problems.Add("Главный манифест не совпадает с числом записей посевных прогонов.")
    }
}

$result = [ordered]@{
    schema = 'xiigen-legacy-dpo-artifact-quarantine-verification-v1'
    quarantine_root = $QuarantineRoot
    legacy_three_field_records = $legacyRecords.Count
    seed_dpo_run_records = $seedRecords.Count
    problems = @($Problems)
    passed = $Problems.Count -eq 0
}

if ($Json) {
    $result | ConvertTo-Json -Depth 6
}
else {
    if ($Problems.Count -eq 0) {
        Write-Output "Карантин старых DPO-артефактов принят: обучение и смысловая публикация закрыты для $($legacyRecords.Count) старых наборов и $($seedRecords.Count) посевных прогонов."
    }
    else {
        Write-Output "Карантин старых DPO-артефактов не принят:"
        foreach ($problem in $Problems) {
            Write-Output "- $problem"
        }
    }
}

if ($Problems.Count -gt 0) {
    exit 1
}
