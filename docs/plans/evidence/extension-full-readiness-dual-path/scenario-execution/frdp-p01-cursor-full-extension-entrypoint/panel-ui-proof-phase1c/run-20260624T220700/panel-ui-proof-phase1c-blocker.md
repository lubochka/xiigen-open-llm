# FRDP-P01 Cursor Panel UI Proof Phase 1C Blocker

Status: `BLOCKED_WITH_NEXT_ACTION`

One bounded local Cursor command was run for scenario `frdp-p01-cursor-full-extension-entrypoint` using the accepted panel harness. The run root was:

```text
C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\panel-ui-proof-phase1c\run-20260624T220700
```

Command:

```powershell
$scenarioRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint"
$extensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$runRoot = Join-Path $scenarioRoot ("panel-ui-proof-phase1c\run-" + (Get-Date -Format "yyyyMMddTHHmmss"))
New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
$env:FRDP_P01_CURSOR_PANEL_UI_PROOF_DIR = $runRoot
& "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe" `
  --new-window `
  --user-data-dir (Join-Path $runRoot "user-data") `
  --extensions-dir (Join-Path $runRoot "extensions") `
  --extensionDevelopmentPath $extensionRoot `
  --extensionTestsPath (Join-Path $scenarioRoot "frdp-p01-cursor-panel-ui-proof.js") `
  --disable-workspace-trust `
  --skip-welcome `
  --skip-release-notes `
  --skip-onboarding `
  --disable-telemetry `
  --disable-updates `
  --disable-gpu `
  --disable-chromium-sandbox `
  --log trace `
  --headless `
  $extensionRoot
```

Observed result:

- Shell exit code: `0`.
- Expected validation file was not created:
  `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\panel-ui-proof-phase1c\run-20260624T220700\frdp-p01-cursor-panel-ui-proof-validation.json`
- The run root contains Cursor profile/cache artifacts (`user-data`, `extensions`), but no `frdp-p01-cursor-panel-ui-proof-validation.json`.
- Cursor processes remained running after the command returned.

Field evidence:

- `xiigenSmartSkills.panel` command invocation through the harness: not proven by this run because the validation JSON is absent.
- Same FRDP-P01 marker in prompt and panel evidence: not proven by this run.
- Correlation id matched: not proven by this run.
- Same shared extension request path as `runtimeProof.invoke`: not proven by this run.
- Non-empty visible/panel text: not proven by this run.
- Online prompt sent by harness: `false` by harness design; no online prompt evidence was produced by this run.
- Product readiness claimed: `false`.

Next smallest fix:

Use a Cursor/VS Code extension-test launcher that waits for the Extension Host test runner to execute the `--extensionTestsPath`, or add explicit stdout/log capture around extension test bootstrap so a zero GUI-process exit without validation is classified as harness-launch failure rather than a pass.
