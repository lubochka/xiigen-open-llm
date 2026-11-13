param(
    [ValidateSet('prepare', 'finish', 'resume')][string]$Action = 'prepare',
    [string]$Stream,
    [string]$Run,
    [Nullable[int]]$Count,
    [ValidateSet('train', 'dev', 'exam')][string]$Split = 'train',
    [int]$Start = 0,
    [string]$Previous,
    [string]$FamilyPlan,
    [string]$WorkRoot,
    [string]$Request,
    [string]$InputPath,
    [string]$Model = 'unknown'
)

# Preparing a prompt never calls a model, reviews rows, or starts training.
# The script location is authoritative; invocation from llm_mvp_core is safe.
$ErrorActionPreference = 'Stop'
$env:PYTHONUTF8 = '1'
$env:PYTHONIOENCODING = 'utf-8'
$utf8 = New-Object System.Text.UTF8Encoding($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$batchScript = Join-Path $PSScriptRoot 'batches.py'
if (-not (Test-Path -LiteralPath $batchScript -PathType Leaf)) {
    throw "Missing preparation tool: $batchScript. Update only the preparation package; do not create placeholders."
}
$pythonCommand = Get-Command py -CommandType Application -ErrorAction SilentlyContinue
$pythonPrefix = @('-3')
if (-not $pythonCommand) {
    $pythonCommand = Get-Command python -CommandType Application -ErrorAction SilentlyContinue
    $pythonPrefix = @()
}
if (-not $pythonCommand) {
    throw 'Python 3 is not available as py -3 or python. No generation started. No alternate repository/path was searched.'
}
if ($Action -eq 'prepare') {
    if (-not $Stream -or -not $Run) { throw 'prepare requires -Stream and -Run; take both from execution-queue.json.' }
    if (-not $WorkRoot) {
        $pathsFile = Join-Path (Split-Path -Parent $PSScriptRoot) 'paths.json'
        $pathConfig = Get-Content -LiteralPath $pathsFile -Raw -Encoding UTF8 | ConvertFrom-Json
        $WorkRoot = [string]$pathConfig.learning_sets_root_windows
        if (-not $WorkRoot) { throw 'paths.json does not declare learning_sets_root_windows; no run started.' }
    }
    $batchArguments = @($batchScript, 'prepare', '--stream', $Stream, '--run', $Run,
        '--split', $Split, '--start', [string]$Start, '--work-root', $WorkRoot)
    if ($PSBoundParameters.ContainsKey('Count')) { $batchArguments += @('--count', [string]$Count) }
    if ($Previous) { $batchArguments += @('--previous', $Previous) }
    if ($FamilyPlan) { $batchArguments += @('--family-plan', $FamilyPlan) }
} else {
    if ($WorkRoot) { throw 'finish/resume preserve the root saved in -Request; -WorkRoot is only for new prepare.' }
    if (-not $Request) { throw "$Action requires the exact -Request path; do not search another repository." }
    $batchArguments = @($batchScript, $Action, '--request', $Request)
    if ($Action -eq 'finish') {
        if ($InputPath) { $batchArguments += @('--input', $InputPath) }
        $batchArguments += @('--model', $Model)
    }
}
& $pythonCommand.Source @pythonPrefix @batchArguments
exit $LASTEXITCODE
