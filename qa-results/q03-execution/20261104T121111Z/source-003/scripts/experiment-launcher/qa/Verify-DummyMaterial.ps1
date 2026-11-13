param([Parameter(Mandatory)][string]$InputPath, [Parameter(Mandatory)][string]$OutputPath)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '../ExperimentLauncher.psm1') -Force
$identity = Get-ProcessIdentity $PID
$reason = 'ACCEPTED'; $code = 0; $inputIdentity = $null
try {
    $read = Read-LauncherFile $InputPath -Json
    $inputIdentity = $read.identity; $m = $read.value
    if ($m.schema -cne 'q03-material/v1' -or $m.arms -isnot [array]) { throw 'MATERIAL_SCHEMA' }
    $ids = @($m.arms | ForEach-Object id)
    if ($ids.Count -ne 2 -or @($ids | Sort-Object -Unique).Count -ne 2 -or (@($ids | Sort-Object) -join ',') -cne 'A,B') { throw 'REQUIRED_ARM_SET' }
    foreach ($arm in $m.arms) {
        if ($arm.qualified -isnot [bool] -or -not $arm.qualified) { throw 'QUALIFICATION_REFUSED' }
        if ($arm.completed -isnot [bool] -or -not $arm.completed) { throw 'ARM_RESULT_REFUSED' }
        if ($arm.checkpoint -cne ($arm.id + '.checkpoint.txt')) { throw 'CHECKPOINT_REQUIRED' }
        $checkpoint = Join-Path (Split-Path $InputPath) $arm.checkpoint
        if (-not (Test-Path -LiteralPath $checkpoint)) { throw 'CHECKPOINT_REQUIRED' }
        if ([IO.File]::ReadAllText($checkpoint) -cne ('Q03-checkpoint-' + $arm.id + "`n")) { throw 'CHECKPOINT_IDENTITY' }
        if ($arm.reader_completed -isnot [bool] -or -not $arm.reader_completed) { throw 'READER_REFUSED' }
    }
} catch { $code = 9; $reason = $_.Exception.Message }
Write-LauncherJson $OutputPath @{ schema = 'q03-verifier-result/v1'; owner = $identity; input_identity = $inputIdentity
    source_identity = (Read-LauncherFile $PSCommandPath).identity; stage = 'REQUIRED_MATERIAL'; reason = $reason; exit_code = $code
    scope = 'SYNTHETIC_RECEIPT_FINALIZER_NO_MODEL_OR_CHECKPOINT_LOAD' }
Write-Host "REQUIRED_MATERIAL/$reason"
exit $code
