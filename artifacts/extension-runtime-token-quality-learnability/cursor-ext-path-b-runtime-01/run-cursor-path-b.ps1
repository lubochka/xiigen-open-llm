$ErrorActionPreference = "Stop"

$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-runtime-01"
$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$ExtensionRoot = "C:\Projects\cursor-xiigen-smart-skills"
$TestPath = Join-Path $EvidenceRoot "path-b-extension-test-entrypoint.js"
$Row23Input = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
$RunId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$CorrelationId = "cursor-path-b-runtime-01-$RunId"
$Stdout = Join-Path $EvidenceRoot "cursor-path-b.stdout.log"
$Stderr = Join-Path $EvidenceRoot "cursor-path-b.stderr.log"
$SummaryPath = Join-Path $EvidenceRoot "cursor-path-b-command-summary.json"
$UserDataDir = Join-Path $EvidenceRoot "cursor-user-data"
$ExtensionsDir = Join-Path $EvidenceRoot "cursor-extensions-dir"
$ResultPath = Join-Path $EvidenceRoot "path-b-extension-host-result.json"

New-Item -ItemType Directory -Force -Path $EvidenceRoot, $UserDataDir, $ExtensionsDir | Out-Null
Remove-Item -LiteralPath $Stdout, $Stderr, $SummaryPath, $ResultPath -Force -ErrorAction SilentlyContinue

$env:CURSOR_PATH_B_EVIDENCE_ROOT = $EvidenceRoot
$env:CURSOR_PATH_B_ROW23_INPUT = $Row23Input
$env:CURSOR_PATH_B_CORRELATION_ID = $CorrelationId
$env:PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = Join-Path $EvidenceRoot "extension-host-evidence"

$argsList = @(
  "--extensionDevelopmentPath", $ExtensionRoot,
  "--extensionTestsPath", $TestPath,
  "--user-data-dir", $UserDataDir,
  "--extensions-dir", $ExtensionsDir,
  "--disable-workspace-trust",
  "--skip-welcome",
  "--skip-release-notes",
  "--skip-onboarding",
  "--disable-telemetry",
  "--disable-updates",
  "--log", "trace",
  "--headless",
  "--new-window",
  $ExtensionRoot
)

$start = Get-Date
$process = Start-Process -FilePath $CursorExe -ArgumentList $argsList -NoNewWindow -PassThru -RedirectStandardOutput $Stdout -RedirectStandardError $Stderr
$timedOut = $false
$resultObserved = $false

while (-not $process.HasExited) {
  if (Test-Path -LiteralPath $ResultPath) {
    $resultObserved = $true
    Start-Sleep -Seconds 2
    if (-not $process.HasExited) {
      Stop-Process -Id $process.Id -Force
    }
    break
  }

  if (((Get-Date) - $start).TotalSeconds -gt 90) {
    $timedOut = $true
    Stop-Process -Id $process.Id -Force
    break
  }

  Start-Sleep -Seconds 1
}

$process.WaitForExit()
$elapsedSeconds = [Math]::Round(((Get-Date) - $start).TotalSeconds, 3)
$resultObserved = $resultObserved -or (Test-Path -LiteralPath $ResultPath)
$stdoutText = if (Test-Path -LiteralPath $Stdout) { Get-Content -LiteralPath $Stdout -Raw } else { "" }
$stderrText = if (Test-Path -LiteralPath $Stderr) { Get-Content -LiteralPath $Stderr -Raw } else { "" }

$summary = [ordered]@{
  schema = "xiigen.cursor_ext_path_b_runtime_01.command_summary.v1"
  subagent_id_or_nickname = "CURSOR-EXT-PATH-B-RUNTIME-01"
  command = "$CursorExe --extensionDevelopmentPath $ExtensionRoot --extensionTestsPath $TestPath --user-data-dir <evidence-user-data> --extensions-dir <evidence-extensions-dir> --disable-workspace-trust --skip-welcome --skip-release-notes --skip-onboarding --disable-telemetry --disable-updates --log trace --headless --new-window $ExtensionRoot"
  process_id = $process.Id
  exit_code = $process.ExitCode
  timed_out = $timedOut
  result_file_observed = $resultObserved
  process_stopped_after_evidence = ($resultObserved -and $process.ExitCode -ne 0)
  elapsed_seconds = $elapsedSeconds
  evidence_root = $EvidenceRoot
  user_data_dir = $UserDataDir
  extensions_dir = $ExtensionsDir
  result_ref = $ResultPath
  stdout_ref = $Stdout
  stderr_ref = $Stderr
  correlation_id = $CorrelationId
  cursor_cli = $CursorExe
  row23_input_ref = $Row23Input
  no_network_installs_or_downloads_started_by_runner = $true
  no_external_ai_prompt_submission_by_runner = $true
  stdout_summary = $stdoutText.Substring(0, [Math]::Min($stdoutText.Length, 2000))
  stderr_summary = $stderrText.Substring(0, [Math]::Min($stderrText.Length, 2000))
}

$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $SummaryPath -Encoding UTF8

if (-not $resultObserved) {
  throw "Cursor Path B result file was not observed. See $SummaryPath"
}

if (Test-Path -LiteralPath $ResultPath) {
  Get-Content -LiteralPath $ResultPath -Raw
}
