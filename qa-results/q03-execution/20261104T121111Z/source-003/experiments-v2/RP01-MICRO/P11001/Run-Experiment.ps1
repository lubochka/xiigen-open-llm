# P110: одна общая цепь claim / stage / terminal. Исторические результаты не переписываются.
[CmdletBinding()]
param(
    [string]$Mode = 'all',
    [double]$BuildDeadlineSeconds = 600,
    [double]$RunDeadlineSeconds = 1200,
    [double]$TotalDeadlineSeconds = 1800,
    [string]$QaStage = 'run',
    [string]$QaOutputRoot,
    [string]$QaAttemptId,
    [string]$QaFixture
)
$ErrorActionPreference = 'Stop'
# Никакой нормализации Program.cs/csproj/src или старых результатов при входе.
$module = Join-Path $PSScriptRoot '../../../scripts/experiment-launcher/ExperimentLauncher.psm1'
Import-Module $module -Force
$code = Invoke-ExperimentLauncher -Experiment 'P110-PRIMARY-01' -HostDirectory $PSScriptRoot -EntryPath $PSCommandPath -Mode $Mode `
    -BuildDeadlineSeconds $BuildDeadlineSeconds -RunDeadlineSeconds $RunDeadlineSeconds -TotalDeadlineSeconds $TotalDeadlineSeconds `
    -QaStage $QaStage -QaOutputRoot $QaOutputRoot -QaAttemptId $QaAttemptId -QaFixture $QaFixture
exit $code
