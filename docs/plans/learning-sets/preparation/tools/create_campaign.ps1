param(
    [ValidateSet('next', 'status', 'finish', 'resume', 'progress', 'release', 'flag')][string]$Action = 'next',
    [string]$WorkRoot,
    [string]$AgentId,
    [ValidateSet('translations', 'orchestration', 'software', 'all')][string]$Pool = 'all',
    [string]$ClaimToken,
    [string]$FlagId,
    [string]$Domain,
    [string]$Topic,
    [string]$Profile,
    [ValidateSet('needs_rework', 'needs_assessment', 'resolved')][string]$QualityStatus,
    [string]$Reason,
    [string[]]$Evidence,
    [string]$Allocation,
    [string]$Request,
    [string]$InputPath,
    [string]$FamilyMaterial,
    [string]$Model = 'unknown',
    [string]$PythonExecutable
)

# The script location owns the plan. No checkout or runtime scanning is used.
$ErrorActionPreference = 'Stop'
$env:PYTHONUTF8 = '1'
$env:PYTHONIOENCODING = 'utf-8'
$utf8 = New-Object System.Text.UTF8Encoding($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$runner = Join-Path $PSScriptRoot 'campaign.py'
if (-not (Test-Path -LiteralPath $runner -PathType Leaf)) {
    throw "Missing assigned runner: $runner. Restore this preparation file; do not search another checkout."
}
$arguments = @($runner, $Action)
$pythonPrefix = @()
if ($Action -ne 'status') {
    if (-not $AgentId) { throw 'Supply -AgentId with a unique ID for this Kiro session. Reuse it only for the same saved session.' }
    $arguments += @('--agent-id', $AgentId)
}
if ($Action -in @('finish', 'progress', 'release') -and -not $ClaimToken) {
    throw 'Use the exact -ClaimToken from the saved coordination-commands.md. No automatic takeover is allowed.'
}
if ($ClaimToken) {
    if ($Action -notin @('finish', 'resume', 'progress', 'release')) { throw '-ClaimToken applies to a saved request action.' }
    $arguments += @('--claim-token', $ClaimToken)
}
if ($Action -in @('finish', 'resume', 'progress', 'release')) {
    if ($WorkRoot -or $Allocation) {
        throw 'finish/resume/progress/release use the exact saved request. -WorkRoot and -Allocation apply only to next/status/flag as documented.'
    }
    if (-not $Request -or -not (Test-Path -LiteralPath $Request -PathType Leaf)) {
        throw 'An existing, exact -Request path is required. No new generation started.'
    }
    $saved = Get-Content -LiteralPath $Request -Raw -Encoding UTF8 | ConvertFrom-Json
    if (-not $PythonExecutable -and $saved.paths.python_executable) {
        $PythonExecutable = [string]$saved.paths.python_executable
    }
    $arguments += @('--request', $Request)
    if ($Action -eq 'finish') {
        if ($InputPath) { $arguments += @('--input', $InputPath) }
        if ($FamilyMaterial) { $arguments += @('--family-material', $FamilyMaterial) }
        $arguments += @('--model', $Model)
    }
} else {
    if (-not $WorkRoot) { $WorkRoot = 'C:/Projects/llm_mvp_core/learning-sets' }
    $arguments += @('--work-root', $WorkRoot)
    if ($Action -eq 'next') { $arguments += @('--pool', $Pool) }
    if ($Action -eq 'flag') {
        if (-not $FlagId -or -not $Domain -or -not $QualityStatus -or -not $Reason -or -not $Evidence) {
            throw 'flag requires -FlagId, -Domain, -QualityStatus, -Reason and one or more exact -Evidence references.'
        }
        $arguments += @('--flag-id', $FlagId, '--domain', $Domain, '--quality-status', $QualityStatus, '--reason', $Reason)
        if ($Topic) { $arguments += @('--topic', $Topic) }
        if ($Profile) { $arguments += @('--profile', $Profile) }
        foreach ($reference in $Evidence) { $arguments += @('--evidence', $reference) }
    }
    if ($Allocation) {
        if ($Action -ne 'next') { throw '-Allocation is valid only for next.' }
        $arguments += @('--allocation', $Allocation)
    }
}
if (-not $PythonExecutable) {
    # Fixed Windows launcher. If unavailable, supply the known executable explicitly.
    $PythonExecutable = 'py'
    $pythonPrefix = @('-3')
}
try {
    & $PythonExecutable @pythonPrefix @arguments
    exit $LASTEXITCODE
} catch {
    throw "Could not invoke the assigned Python executable '$PythonExecutable'. Supply -PythonExecutable with its known absolute path; no runtime search or generation was performed. $($_.Exception.Message)"
}

