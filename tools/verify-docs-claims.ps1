<#
.SYNOPSIS
    verify-docs-claims.ps1 - filesystem-only verification of the public-facing
    documentation claims for the LlmFromScratch repository.

.DESCRIPTION
    Guards the front layer (README.md, docs/FAQ/*.md, docs/SEO-GITHUB-METADATA.md)
    against silent drift as the repo is prepared for its AGPL-3.0-only publication.

    Five groups of checks, all offline and read-only:
      1. ANCHORS   - every "NN.Something/Path/File.cs[:line]" reference in README.md
                     and docs/FAQ/*.md resolves to a real file (and, when a line is
                     given, the file is at least that long).
      2. COUNTS    - the numbered-subproject count and FAQ topic count claimed in
                     README.md match what is actually on disk; docs/FAQ holds
                     exactly 15 topics + README.md + why-agpl.md = 17 markdown files.
      3. FORBIDDEN - none of the banned marketing/overclaim terms appear in the
                     scanned docs (the "Deliberately excluded topics" section of
                     SEO-GITHUB-METADATA.md is exempt: it documents WHY they are
                     excluded).
      4. FRESHNESS - README.md and every docs/FAQ/*.md carry a
                     "Last updated: YYYY-MM-DD" line (older than 90 days -> warning).
      5. GEO SHAPE - (warnings only) the README "What is LlmFromScratch?" direct
                     answer is 40-60 words and the block after it is 134-167 words.

    Errors fail the run (exit 1). Warnings never fail the run.
    PowerShell 5.1 compatible. No network access. No writes outside stdout.

.OUTPUTS
    A human-readable summary table, then "PASS" or "FAIL: <n> errors".
    Exit code 0 on PASS, 1 on FAIL.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path

$errors   = New-Object 'System.Collections.Generic.List[string]'
$warnings = New-Object 'System.Collections.Generic.List[string]'
$rows     = New-Object 'System.Collections.Generic.List[object]'

function Add-Row {
    param([string]$Group, [string]$Check, [string]$Result, [string]$Detail)
    $rows.Add([pscustomobject]@{ Group = $Group; Check = $Check; Result = $Result; Detail = $Detail })
}

function Read-Text {
    param([string]$FullPath)
    return [System.IO.File]::ReadAllText($FullPath, [System.Text.Encoding]::UTF8)
}

function Read-Lines {
    param([string]$FullPath)
    return [System.IO.File]::ReadAllLines($FullPath, [System.Text.Encoding]::UTF8)
}

function Count-Words {
    param([string]$Text)
    $clean = $Text -replace '`', ' '
    $tokens = @($clean -split '\s+' | Where-Object { $_ -match '\w' })
    return $tokens.Count
}

# -- Target file set ---------------------------------------------------------

$readmePath = Join-Path $repoRoot 'README.md'
$faqDir     = Join-Path $repoRoot 'docs/FAQ'
$seoPath    = Join-Path $repoRoot 'docs/SEO-GITHUB-METADATA.md'

if (-not (Test-Path -LiteralPath $readmePath)) {
    $errors.Add('SETUP: README.md not found at repo root')
    Add-Row 'SETUP' 'README.md present' 'FAIL' 'README.md not found'
}

$faqFiles = @()
if (Test-Path -LiteralPath $faqDir) {
    $faqFiles = @(Get-ChildItem -LiteralPath $faqDir -Filter '*.md' -File | Sort-Object Name)
} else {
    $errors.Add('SETUP: docs/FAQ directory not found')
    Add-Row 'SETUP' 'docs/FAQ present' 'FAIL' 'docs/FAQ not found'
}

# -- 1. ANCHORS --------------------------------------------------------------

$anchorRegex = '(?<path>[0-9]{2}\.[A-Za-z0-9._-]+(?:/[A-Za-z0-9._-]+)+\.cs)(?::(?<line>[0-9]+))?'
$anchorSources = @()
if (Test-Path -LiteralPath $readmePath) { $anchorSources += $readmePath }
$anchorSources += ($faqFiles | ForEach-Object { $_.FullName })

$anchorTotal = 0
$anchorSeen = New-Object 'System.Collections.Generic.HashSet[string]'
$brokenAnchors = New-Object 'System.Collections.Generic.List[string]'

foreach ($src in $anchorSources) {
    $rel = $src.Substring($repoRoot.Length).TrimStart('\', '/')
    $text = Read-Text $src
    $matches = [regex]::Matches($text, $anchorRegex)
    foreach ($m in $matches) {
        $anchorPath = $m.Groups['path'].Value
        $lineVal = $m.Groups['line'].Value
        $anchorLabel = $anchorPath
        if ($lineVal) { $anchorLabel = "$anchorPath`:$lineVal" }

        $dedupeKey = "$rel|$anchorLabel"
        if (-not $anchorSeen.Add($dedupeKey)) { continue }
        $anchorTotal++

        $targetRel = $anchorPath -replace '/', [System.IO.Path]::DirectorySeparatorChar
        $targetFull = Join-Path $repoRoot $targetRel

        if (-not (Test-Path -LiteralPath $targetFull -PathType Leaf)) {
            $brokenAnchors.Add("$anchorLabel (in $rel) -> file missing")
            continue
        }
        if ($lineVal) {
            $needed = [int]$lineVal
            $actual = (Read-Lines $targetFull).Count
            if ($needed -gt $actual) {
                $brokenAnchors.Add("$anchorLabel (in $rel) -> only $actual lines")
            }
        }
    }
}

if ($brokenAnchors.Count -eq 0) {
    Add-Row 'ANCHORS' 'code file:line references resolve' 'PASS' "$anchorTotal anchors, 0 broken"
} else {
    foreach ($b in $brokenAnchors) { $errors.Add("ANCHOR: $b") }
    Add-Row 'ANCHORS' 'code file:line references resolve' 'FAIL' "$anchorTotal anchors, $($brokenAnchors.Count) broken"
}

# -- 2. COUNTS ---------------------------------------------------------------

$readmeText = ''
if (Test-Path -LiteralPath $readmePath) { $readmeText = Read-Text $readmePath }

# 2a. numbered subproject directories
$subDirs = @(Get-ChildItem -LiteralPath $repoRoot -Directory | Where-Object { $_.Name -match '^[0-9]{2}\.' })
$subCount = $subDirs.Count
$mSub = [regex]::Match($readmeText, '([0-9]+)\s+numbered subproject')
if ($mSub.Success) {
    $claimedSub = [int]$mSub.Groups[1].Value
    if ($claimedSub -eq $subCount) {
        Add-Row 'COUNTS' 'numbered subproject dirs' 'PASS' "README claims $claimedSub, on disk $subCount"
    } else {
        $errors.Add("COUNT: README claims $claimedSub numbered subprojects but $subCount exist on disk")
        Add-Row 'COUNTS' 'numbered subproject dirs' 'FAIL' "README claims $claimedSub, on disk $subCount"
    }
} else {
    $errors.Add('COUNT: could not find a "<N> numbered subproject" claim in README.md')
    Add-Row 'COUNTS' 'numbered subproject dirs' 'FAIL' "claim not found; on disk $subCount"
}

# 2b. FAQ topic count (A*/B*/C* files) vs README "<N> topics"
$topicFiles = @($faqFiles | Where-Object { $_.Name -match '^[ABC][0-9]' })
$topicCount = $topicFiles.Count
$mTopics = [regex]::Match($readmeText, '([0-9]+)\s+topics')
if ($mTopics.Success) {
    $claimedTopics = [int]$mTopics.Groups[1].Value
    if ($claimedTopics -eq $topicCount) {
        Add-Row 'COUNTS' 'FAQ topic count' 'PASS' "README claims $claimedTopics, on disk $topicCount"
    } else {
        $errors.Add("COUNT: README claims $claimedTopics FAQ topics but $topicCount A*/B*/C* files exist")
        Add-Row 'COUNTS' 'FAQ topic count' 'FAIL' "README claims $claimedTopics, on disk $topicCount"
    }
} else {
    $errors.Add('COUNT: could not find a "<N> topics" claim in README.md')
    Add-Row 'COUNTS' 'FAQ topic count' 'FAIL' "claim not found; on disk $topicCount"
}

# 2c. docs/FAQ file inventory: 15 topics + README.md + why-agpl.md = 17
$faqTotal = $faqFiles.Count
$hasFaqReadme = @($faqFiles | Where-Object { $_.Name -eq 'README.md' }).Count -eq 1
$hasWhyAgpl   = @($faqFiles | Where-Object { $_.Name -eq 'why-agpl.md' }).Count -eq 1
if ($topicCount -eq 15 -and $hasFaqReadme -and $hasWhyAgpl -and $faqTotal -eq 17) {
    Add-Row 'COUNTS' 'docs/FAQ inventory (15+README+why-agpl=17)' 'PASS' "$faqTotal md files, $topicCount topics"
} else {
    $errors.Add("COUNT: docs/FAQ inventory off - $faqTotal md files, $topicCount topics, README.md=$hasFaqReadme, why-agpl.md=$hasWhyAgpl (expected 17 / 15 / true / true)")
    Add-Row 'COUNTS' 'docs/FAQ inventory (15+README+why-agpl=17)' 'FAIL' "$faqTotal md files, $topicCount topics, README=$hasFaqReadme, why-agpl=$hasWhyAgpl"
}

# -- 3. FORBIDDEN TERMS ------------------------------------------------------

$forbiddenTerms = @(
    'open-source-llm',
    'production-ready',
    'battle-tested',
    'enterprise-grade',
    'universal trained LLM',
    'general-purpose LLM weights'
)

$forbiddenTargets = @()
if (Test-Path -LiteralPath $readmePath) { $forbiddenTargets += $readmePath }
$forbiddenTargets += ($faqFiles | ForEach-Object { $_.FullName })
if (Test-Path -LiteralPath $seoPath) { $forbiddenTargets += $seoPath }

$forbiddenHits = New-Object 'System.Collections.Generic.List[string]'
foreach ($src in $forbiddenTargets) {
    $rel = $src.Substring($repoRoot.Length).TrimStart('\', '/')
    $lines = Read-Lines $src

    # For the SEO metadata file, compute the exempt "Deliberately excluded topics"
    # line range (from that header to the next level-1/2 header, exclusive).
    $exemptStart = -1
    $exemptEnd = -1
    if ($src -eq $seoPath) {
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match '^\#{1,2}\s+Deliberately excluded topics') {
                $exemptStart = $i
                $exemptEnd = $lines.Count
                for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                    if ($lines[$j] -match '^\#{1,2}\s') { $exemptEnd = $j; break }
                }
                break
            }
        }
    }

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($exemptStart -ge 0 -and $i -gt $exemptStart -and $i -lt $exemptEnd) { continue }
        $lower = $lines[$i].ToLowerInvariant()
        foreach ($term in $forbiddenTerms) {
            if ($lower.Contains($term.ToLowerInvariant())) {
                $forbiddenHits.Add("$rel`:$($i + 1): '$term'")
            }
        }
    }
}

if ($forbiddenHits.Count -eq 0) {
    Add-Row 'FORBIDDEN' 'no banned marketing/overclaim terms' 'PASS' "$($forbiddenTargets.Count) files scanned"
} else {
    foreach ($h in $forbiddenHits) { $errors.Add("FORBIDDEN: $h") }
    Add-Row 'FORBIDDEN' 'no banned marketing/overclaim terms' 'FAIL' "$($forbiddenHits.Count) hit(s)"
}

# -- 4. FRESHNESS ------------------------------------------------------------

$freshnessTargets = @()
if (Test-Path -LiteralPath $readmePath) { $freshnessTargets += $readmePath }
$freshnessTargets += ($faqFiles | ForEach-Object { $_.FullName })

$today = (Get-Date).Date
$missingDates = New-Object 'System.Collections.Generic.List[string]'
$staleDates = New-Object 'System.Collections.Generic.List[string]'

foreach ($src in $freshnessTargets) {
    $rel = $src.Substring($repoRoot.Length).TrimStart('\', '/')
    $text = Read-Text $src
    $mDate = [regex]::Match($text, 'Last updated:\s*([0-9]{4}-[0-9]{2}-[0-9]{2})')
    if (-not $mDate.Success) {
        $missingDates.Add($rel)
        continue
    }
    [datetime]$parsed = [datetime]::MinValue
    $ok = [System.DateTime]::TryParseExact(
        [string]$mDate.Groups[1].Value, [string]'yyyy-MM-dd',
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::None, [ref]$parsed)
    if ($ok -and ($today - $parsed.Date).Days -gt 90) {
        $staleDates.Add("$rel ($($mDate.Groups[1].Value), $(($today - $parsed.Date).Days) days)")
    }
}

if ($missingDates.Count -eq 0) {
    Add-Row 'FRESHNESS' 'Last updated line present' 'PASS' "$($freshnessTargets.Count) files"
} else {
    foreach ($d in $missingDates) { $errors.Add("FRESHNESS: missing 'Last updated:' line in $d") }
    Add-Row 'FRESHNESS' 'Last updated line present' 'FAIL' "$($missingDates.Count) file(s) missing a date"
}
if ($staleDates.Count -gt 0) {
    foreach ($s in $staleDates) { $warnings.Add("FRESHNESS: stale (>90 days) $s") }
    Add-Row 'FRESHNESS' 'dates within 90 days' 'WARN' "$($staleDates.Count) file(s) older than 90 days"
}

# -- 5. GEO SHAPE (warnings only) --------------------------------------------

if (Test-Path -LiteralPath $readmePath) {
    $readmeLines = Read-Lines $readmePath
    $secStart = -1
    for ($i = 0; $i -lt $readmeLines.Count; $i++) {
        if ($readmeLines[$i] -match '^\#\#\s+What is LlmFromScratch\?') { $secStart = $i; break }
    }
    if ($secStart -lt 0) {
        $warnings.Add('GEO: "## What is LlmFromScratch?" section not found in README.md')
        Add-Row 'GEO' 'What-is section present' 'WARN' 'section heading not found'
    } else {
        $secEnd = $readmeLines.Count
        for ($j = $secStart + 1; $j -lt $readmeLines.Count; $j++) {
            if ($readmeLines[$j] -match '^\#\#\s') { $secEnd = $j; break }
        }
        $paras = New-Object 'System.Collections.Generic.List[string]'
        $cur = New-Object 'System.Collections.Generic.List[string]'
        for ($j = $secStart + 1; $j -lt $secEnd; $j++) {
            if ($readmeLines[$j].Trim() -eq '') {
                if ($cur.Count -gt 0) { $paras.Add(($cur -join ' ')); $cur.Clear() }
            } else {
                $cur.Add($readmeLines[$j].Trim())
            }
        }
        if ($cur.Count -gt 0) { $paras.Add(($cur -join ' ')) }

        if ($paras.Count -ge 1) {
            $w1 = Count-Words $paras[0]
            if ($w1 -ge 40 -and $w1 -le 60) {
                Add-Row 'GEO' 'direct answer 40-60 words' 'PASS' "$w1 words"
            } else {
                $warnings.Add("GEO: direct answer is $w1 words (target 40-60)")
                Add-Row 'GEO' 'direct answer 40-60 words' 'WARN' "$w1 words (target 40-60)"
            }
        } else {
            $warnings.Add('GEO: no direct-answer paragraph under the What-is heading')
            Add-Row 'GEO' 'direct answer 40-60 words' 'WARN' 'paragraph not found'
        }

        if ($paras.Count -ge 2) {
            $w2 = Count-Words $paras[1]
            if ($w2 -ge 134 -and $w2 -le 167) {
                Add-Row 'GEO' 'self-contained block 134-167 words' 'PASS' "$w2 words"
            } else {
                $warnings.Add("GEO: self-contained block is $w2 words (target 134-167)")
                Add-Row 'GEO' 'self-contained block 134-167 words' 'WARN' "$w2 words (target 134-167)"
            }
        } else {
            $warnings.Add('GEO: no self-contained block paragraph under the What-is heading')
            Add-Row 'GEO' 'self-contained block 134-167 words' 'WARN' 'paragraph not found'
        }
    }
}

# -- Report ------------------------------------------------------------------

Write-Output '=== verify-docs-claims: LlmFromScratch documentation claim check ==='
Write-Output "repo root: $repoRoot"
Write-Output ''
Write-Output (($rows | Format-Table -AutoSize -Property Group, Check, Result, Detail | Out-String).TrimEnd())
Write-Output ''

if ($warnings.Count -gt 0) {
    Write-Output "Warnings ($($warnings.Count)):"
    foreach ($w in $warnings) { Write-Output "  - $w" }
    Write-Output ''
}

if ($errors.Count -gt 0) {
    Write-Output "Errors ($($errors.Count)):"
    foreach ($e in $errors) { Write-Output "  - $e" }
    Write-Output ''
    Write-Output "FAIL: $($errors.Count) errors"
    exit 1
}

Write-Output 'PASS'
exit 0
