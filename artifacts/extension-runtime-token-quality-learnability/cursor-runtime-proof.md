# RUNTIME-03 Cursor Runtime Proof

Classification: IN_PROGRESS_NARROW_EVIDENCE_ONLY

Correlation id: RUNTIME03-CURSOR-20260616-210605
Scenario id: RUNTIME-03-CURSOR-COMMAND-PROMPT-TO-TASK-SMOKE-20260616-210605

## Command And Target

Command invoked:

```text
C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe "--extensionDevelopmentPath" "C:\Projects\cursor-xiigen-smart-skills" "--extensionTestsPath" "C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\runtime03-extensionHostQA.js" "--user-data-dir" "C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\cursor-user-data" "--extensions-dir" "C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\cursor-extensions-dir" "--disable-workspace-trust" "--skip-welcome" "--skip-release-notes" "--skip-onboarding" "--disable-telemetry" "--disable-updates" "--log" "trace" "--headless" "--new-window" "C:\Projects\cursor-xiigen-smart-skills"
```

Host version/build: Cursor 3.5.38 from file VersionInfo; stdout product metadata showed VS Code version 1.105.1. Cursor.exe --version returned no stdout here.

Target type: command, xiigen.smartSkills.select. I chose the command route because it is a contributed user-facing Cursor/VS Code command that can be invoked inside the real Extension Host noninteractively. Panel and chat participant entrypoints exist, but no safe noninteractive chat send/invoke API was available in this run.

## What This Proves

- Fresh live Cursor Extension Host launch happened in this slice: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605
- The extension loaded and activated as xiigen.cursor-xiigen-smart-skills.
- The command produced a visible answer: Core xiigen boundary executed. Semantic quality is not claimed. |  | xiigen prompt-to-task : | input: <redacted> | output: <redacted>
- The positive request reached SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest through C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1.
- Debug trace was visible as xiigen prompt-to-task : input: <redacted> output: <redacted>.
- The controlled negative/error path surfaced CONTRACT_ONLY_DOWNGRADE through the same command entrypoint.

## Raw And Redacted Evidence

Raw private input ref: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-07-00-867Z.json
Redacted input text/ref: RUNTIME03-CURSOR-20260616-210605 command prompt-to-task debug on. Redacted smoke prompt. / C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof.json

Raw output ref: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T18-07-00-867Z.json
Raw entrypoint output ref: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T18-07-00-867Z.json
Redacted output text/ref: Core xiigen boundary executed. Semantic quality is not claimed. |  | xiigen prompt-to-task : | input: <redacted> | output: <redacted> / C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof.json

Positive core trace: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-16T18-07-00-867Z.json
Positive host trace: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-16T18-07-00-867Z.json

## Negative/Error Path

The controlled negative invocation set xiigenSmartSkills.coreApiScript to a missing phase-local endpoint and invoked xiigen.smartSkills.select.

Visible negative answer: CONTRACT_ONLY_DOWNGRADE: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\missing-core-api-for-controlled-error.ps1 is missing; Cursor host entrypoint ran but no core runtime decision is claimed.

Negative refs:

- Request: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-07-38-714Z.json
- Host trace: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-16T18-07-38-714Z.json
- Entrypoint response: C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T18-07-38-714Z.json

Exact missing field: the expected negative core response/core trace physical files were not written because the endpoint was intentionally missing. The entrypoint response embeds the fallback coreResponse object and visible downgrade text, but the proof gate requires physical negative response/trace refs.

## What This Does Not Prove

- This does not prove Cursor chat participant UI invocation or panel visual capture.
- This does not prove token savings, quality improvement, semantic quality, DPO, learnability, training, checkpoint mutation, target promotion, rollback, or full plan completion.
- Dashboard/API/helper/package smoke/test-only adapter/direct handler evidence remains supporting-only limitations and was not used as PASS live runtime proof.

## Next Bounded Work Order

HOST-IMPL-02: repair the Cursor adapter/harness so the controlled missing-core path writes physical negative core response and core trace files while keeping Cursor a thin adapter and avoiding training/eval/checkpoint mutation; then rerun RUNTIME-03 Cursor Extension Host proof.
