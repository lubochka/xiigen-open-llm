param([Parameter(Mandatory)][string]$Stage, [Parameter(Mandatory)][string]$Fixture,
    [Parameter(Mandatory)][string]$OutputRoot, [string]$Experiment, [string]$Attempt)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '../ExperimentLauncher.psm1') -Force
$self = Get-ProcessIdentity $PID
$read = Read-LauncherFile $Fixture -Json
$f = $read.value
Write-LauncherJson (Join-Path $OutputRoot "$Stage-process-$PID.json") @{ stage = $Stage; owner = $self
    fixture = $read.identity; source = (Read-LauncherFile $PSCommandPath).identity }
if ($Stage -eq 'grandchild') { Start-Sleep -Seconds 20; exit 0 }
if ($Stage -eq 'run') {
    # This is the identity of the very buffer parsed above, not a second read.
    $consumedIdentity = [ordered]@{ path = $read.identity.path; bytes = $read.identity.bytes; sha256 = $read.identity.sha256 }
    if ($f.Contains('omit_consumer_digest') -and $f.omit_consumer_digest) { $consumedIdentity.Remove('sha256') }
    Write-LauncherJson (Join-Path $OutputRoot 'consumed-inputs.json') @{ schema = 'consumed-inputs/v1'; inputs = @($consumedIdentity) }
}
[IO.File]::AppendAllText((Join-Path $OutputRoot 'callbacks.ndjson'), (@{ stage = $Stage; owner = $self } | ConvertTo-Json -Compress) + "`n", [Text.UTF8Encoding]::new($false))
if ($Stage -eq 'build') {
    if ($f.Contains('build_delay_ms')) { Start-Sleep -Milliseconds $f.build_delay_ms }
    [IO.File]::WriteAllBytes((Join-Path $OutputRoot 'dummy-artifact.bin'), [Text.Encoding]::UTF8.GetBytes('Q03-dummy-build-artifact/v1'))
    exit 0
}
if ($f.Contains('spawn_grandchild') -and $f.spawn_grandchild) {
    $shell = (Get-Process -Id $PID).Path
    $argsQuoted = @('-NoProfile','-File',$PSCommandPath,'-Stage','grandchild','-Fixture',$Fixture,'-OutputRoot',$OutputRoot | ForEach-Object { '"' + $_ + '"' })
    $child = Start-Process $shell -ArgumentList $argsQuoted -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $OutputRoot 'grandchild.stdout.log') -RedirectStandardError (Join-Path $OutputRoot 'grandchild.stderr.log')
    Write-LauncherJson (Join-Path $OutputRoot 'grandchild-identity.json') (Get-ProcessIdentity $child.Id)
}
if ($f.Contains('run_delay_ms')) { Start-Sleep -Milliseconds $f.run_delay_ms }
$code = if ($f.Contains('child_exit')) { [int]$f.child_exit } else { 0 }
if ($f.Contains('verifier_input')) {
    $shell = (Get-Process -Id $PID).Path
    $verifier = Join-Path $PSScriptRoot 'Verify-DummyMaterial.ps1'
    $receipt = Join-Path $OutputRoot 'verifier-result.json'
    $argsQuoted = @('-NoProfile','-File',$verifier,'-InputPath',$f.verifier_input,'-OutputPath',$receipt | ForEach-Object { '"' + $_ + '"' })
    $child = Start-Process $shell -ArgumentList $argsQuoted -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $OutputRoot 'verifier.stdout.log') -RedirectStandardError (Join-Path $OutputRoot 'verifier.stderr.log')
    $vi = Get-ProcessIdentity $child.Id
    if (-not $child.WaitForExit(10000)) { $child.Kill($true); $child.WaitForExit(); $code = 9 }
    else { $child.WaitForExit(); $code = $child.ExitCode }
    Write-LauncherJson (Join-Path $OutputRoot 'verifier-process.json') @{ owner = $vi; exit_code = $code; program_owner = $self }
}
if (-not ($f.Contains('omit_status') -and $f.omit_status)) {
    $id = [int]$Experiment.Substring(1,3)
    $m = if ($f.Contains('measurement')) { [string]$f.measurement } else { 'PASS' }
    $b = if ($f.Contains('budget')) { [string]$f.budget } else { 'PASS' }
    $scientific = if ($f.Contains('scientific')) { [string]$f.scientific } else { 'NOT_OBSERVED' }
    $status = [ordered]@{ learning_validity = 'NOT_APPLICABLE'; hypothesis_result = $scientific }
    if ($id -in @(108,109)) { $status['опыт'] = $Experiment; $status.execution_status = 'COMPLETED'; $status.measurement_status = $m; $status.budget_status = $b }
    elseif ($id -eq 110) { $status.experiment = $Experiment; $status.process_status = 'COMPLETED'; $status.measurement_status = $m; $status.budget_status = $b }
    elseif ($id -eq 111) { $status.run_id = "$Experiment/$Attempt"; $status.process_status = 'COMPLETED'; $status.measurement_status = if ($m -eq 'PASS') { 'VALID' } else { 'INVALID' }; $status.budget_status = if ($b -eq 'PASS') { 'WITHIN_ASSIGNMENT' } else { 'EXCEEDED' } }
    else { $status.experiment = $Experiment; $status.execution_status = 'COMPLETED'; $status.controls_accepted = $m -eq 'PASS'; $status.budget_exact = $b -eq 'PASS'; $status.exit_code = $code; if ($id -eq 113) { $status.reducer_agrees_with_runtime = $true } }
    Write-LauncherJson (Join-Path $OutputRoot 'status.json') $status
}
Write-LauncherJson (Join-Path $OutputRoot 'program-result.json') @{ owner = $self; exit_code = $code; verifier_called = $f.Contains('verifier_input') }
Write-Host "DUMMY_PROGRAM_EXIT=$code"
exit $code
