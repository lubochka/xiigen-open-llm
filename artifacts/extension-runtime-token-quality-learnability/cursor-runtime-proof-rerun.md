# Cursor Runtime Proof Rerun - RUNTIME-03-RERUN

Classification: **PASS live Cursor runtime** for the named Cursor Extension Host command path only. This does not claim chat participant UI, panel UI screenshot, token improvement, quality improvement, learnability, training, checkpoint mutation, rollback, Codex/Claude runtime, or full product readiness.

## Command And Host

- Entrypoint invoked: `C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe "--extensionDevelopmentPath" "C:\\Projects\\cursor-xiigen-smart-skills" "--extensionTestsPath" "C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-cursor\\runtime03-extensionHostQA.js" "--user-data-dir" "C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\cursor-user-data" "--extensions-dir" "C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\cursor-extensions-dir" "--disable-workspace-trust" "--skip-welcome" "--skip-release-notes" "--skip-onboarding" "--disable-telemetry" "--disable-updates" "--log" "trace" "--headless" "--new-window" "C:\\Projects\\cursor-xiigen-smart-skills"`
- Timestamp: launch 2026-06-16T18:35:24.7782237Z to 2026-06-16T18:36:18.5256467Z; report 2026-06-16T18:36:02.662Z to 2026-06-16T18:36:17.886Z
- Host version/build: Cursor.exe file_version=3.5.38 product_version=3.5.38 product_name=Cursor company=Anysphere
- Target type: command `xiigen.smartSkills.select` in Cursor / VS Code Extension Host
- Extension: xiigen.cursor-xiigen-smart-skills at `C:\\Projects\\cursor-xiigen-smart-skills`
- Scenario id: `RUNTIME-03-RERUN-CURSOR-COMMAND-PROMPT-TO-TASK-SMOKE-20260616-213524`
- Correlation id: `RUNTIME03-RERUN-CURSOR-20260616-213524`

## Positive Path

- Raw private input ref: `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-36-04-157Z.json`
- Raw private input text: `RUNTIME03-RERUN-CURSOR-20260616-213524 command prompt-to-task debug on. Redacted smoke prompt.`
- Redacted input text/ref: `<redacted:user-prompt>` in `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-36-04-157Z.json`
- Raw output ref: `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T18-36-04-157Z.json`
- Redacted output text/ref: `Core xiigen boundary executed. Semantic quality is not claimed. xiigen prompt-to-task : input: <redacted> output: <redacted>` in `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T18-36-04-157Z.json`
- Visible UI/chat/command answer: `Core xiigen boundary executed. Semantic quality is not claimed.

xiigen prompt-to-task :
input: <redacted>
output: <redacted>`
- Debug trace: `xiigen prompt-to-task : input: <redacted> output: <redacted>`
- XiigenExtensionApiService/core boundary: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`; response status `core_boundary_partial_no_semantic_quality_claim`; core boundary executed `true`

## Controlled Negative Path

- Invocation: missing core endpoint through the same command entrypoint
- Visible answer: `CONTRACT_ONLY_DOWNGRADE: C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\missing-core-api-for-controlled-error.ps1 is missing; Cursor host entrypoint ran but no core runtime decision is claimed.`
- Physical negative core response file: `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T18-36-17-726Z.json`
- Physical negative core trace file: `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-16T18-36-17-726Z.json`
- Scenario: `controlled_missing_core_endpoint`; correlation id: `cursor-2026-06-16T18:36:17.726Z`; error code: `CORE_ENDPOINT_MISSING`
- Raw private input ref/text: `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-36-17-726Z.json` / `RUNTIME03-RERUN-CURSOR-20260616-213524 controlled missing core endpoint. Redacted smoke prompt.`
- Redacted input/output refs: `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T18-36-17-726Z.json`, `C:\\Projects\\cursor-xiigen-smart-skills\\reports\\RUNTIME-03-RERUN-cursor\\20260616-213524\\extension-host-evidence\\xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-16T18-36-17-726Z.json`
- Negative trace fields include raw input/output and redacted input/output: true

## What This Proves

- The named Cursor Extension Host command path invoked the thin adapter and displayed a command answer.
- The positive request reached `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` in llm_mvp_core.
- The repaired controlled missing-core negative path now writes physical negative core response and core trace files.

## What This Does Not Prove

- It does not prove Cursor chat participant UI, panel screenshot, token savings, quality improvement, learnability, training, checkpoint mutation, rollback, target promotion, Claude/Codex runtime, or full product readiness.

## Next Independent Track

- Parent can review and commit the RUNTIME-03-RERUN proof/state artifacts, then continue the next bounded plan-derived track.
