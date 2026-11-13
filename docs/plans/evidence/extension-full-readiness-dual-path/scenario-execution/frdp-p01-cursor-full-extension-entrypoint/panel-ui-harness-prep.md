# FRDP-P01 Cursor Panel UI Harness Prep

CURRENT TASK: Prepare the smallest panel-command harness for `xiigenSmartSkills.panel` without running Cursor.
NOT THE TASK: Cursor/host UI execution, online prompt send, product readiness, joined pass, stage, commit, or push.
EVIDENCE-ONLY: Existing Phase 1B/1C artifacts explain why this harness is needed but do not make a new readiness claim.

## Harness

`frdp-p01-cursor-panel-ui-proof.js` is a narrow sibling of `frdp-p01-cursor-extension-host-proof.js`.

It calls:

```js
vscode.commands.executeCommand("xiigenSmartSkills.panel", payload)
```

The payload uses the FRDP-P01 marker `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint`, correlation `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send`, and an explicit `evidenceRoot`. The default evidence root is the existing Phase 1B extension-host evidence folder:

```text
docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence
```

## Validated Fields

The harness validates only the JSON loaded from `result.hostPanelUiRef`:

- `schemaVersion`
- `hostKind`
- `panelUiCreated`
- `panelTitle`
- `correlationId`
- `runtimeProofScenario`
- `marker`
- `sameMarkerInPrompt`
- `sameMarkerInCorrelation`
- `panelCommandAcceptedRuntimeProofInput`
- `sameSharedRequestPathAsRuntimeProofInvoke`
- `semanticQualityClaimed`
- `externalAiOrUnrelatedToolUsed`
- `responseRef`
- `traceRef`
- non-empty `visibleText`

It does not validate semantic quality, screenshot readiness, product readiness, or a full joined desktop pass.

## Exact Next Command

Run this only in the next authorized host-evidence subtask:

```powershell
$scenarioRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint"
$extensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$runRoot = Join-Path $scenarioRoot ("panel-ui-proof\run-" + (Get-Date -Format "yyyyMMddTHHmmss"))
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

Expected narrow output artifact:

```text
<runRoot>/frdp-p01-cursor-panel-ui-proof-validation.json
```

Expected field result:

```json
{
  "command_invoked": "xiigenSmartSkills.panel",
  "payload_correlation_id": "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send",
  "payload_marker": "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint",
  "field_results": [
    { "field": "panelUiCreated", "expected": true, "passed": true },
    { "field": "panelCommandAcceptedRuntimeProofInput", "expected": true, "passed": true },
    { "field": "sameSharedRequestPathAsRuntimeProofInvoke", "expected": true, "passed": true },
    { "field": "visibleTextNonEmpty", "expected": true, "passed": true }
  ],
  "product_readiness_claimed": false,
  "online_prompt_or_message_sent_by_harness": false
}
```
