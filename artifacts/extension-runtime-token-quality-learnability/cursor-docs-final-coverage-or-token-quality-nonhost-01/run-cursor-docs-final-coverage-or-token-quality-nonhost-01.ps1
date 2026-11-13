param(
  [string]$CursorRoot = "C:\Projects\cursor-xiigen-smart-skills",
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-docs-final-coverage-or-token-quality-nonhost-01"
)

$ErrorActionPreference = "Stop"
$commandOutput = Join-Path $EvidenceRoot "command-output"
New-Item -ItemType Directory -Force -Path $commandOutput | Out-Null

$commands = @(
  @{
    label = "node-version"
    file = "node.exe"
    args = @("--version")
    cwd = $CursorRoot
  },
  @{
    label = "node-check-verifier"
    file = "node.exe"
    args = @("--check", (Join-Path $CursorRoot "scripts\verify-docs-final-coverage-nonhost.js"))
    cwd = $CursorRoot
  },
  @{
    label = "run-verifier"
    file = "node.exe"
    args = @((Join-Path $CursorRoot "scripts\verify-docs-final-coverage-nonhost.js"))
    cwd = $CursorRoot
  }
)

$results = @()
foreach ($command in $commands) {
  $stdout = Join-Path $commandOutput "$($command.label).stdout.log"
  $stderr = Join-Path $commandOutput "$($command.label).stderr.log"
  $started = (Get-Date).ToUniversalTime().ToString("o")
  $process = Start-Process -FilePath $command.file -ArgumentList $command.args -WorkingDirectory $command.cwd -NoNewWindow -Wait -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
  $finished = (Get-Date).ToUniversalTime().ToString("o")
  $results += [ordered]@{
    label = $command.label
    command = "$($command.file) $($command.args -join ' ')"
    cwd = $command.cwd
    started_at_utc = $started
    finished_at_utc = $finished
    exit_code = $process.ExitCode
    stdout_ref = $stdout
    stderr_ref = $stderr
  }
}

$commandsRunPath = Join-Path $EvidenceRoot "commands-run.json"
$results | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $commandsRunPath

$failed = $results | Where-Object { $_.exit_code -ne 0 }
if ($failed.Count -gt 0) {
  Write-Error "One or more verification commands failed. See $commandsRunPath"
}

Write-Output "Verification commands completed. See $commandsRunPath"
