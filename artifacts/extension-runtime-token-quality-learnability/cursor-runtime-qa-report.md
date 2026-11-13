# RUNTIME-03 Cursor/VS Code Extension Host Runtime QA

Verdict: **PASS_RUNTIME_PROOF_CANDIDATE**

Correlation id: `RUNTIME03-CURSOR-20260616-121302`  
Run dir: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302`

## What this proves

- Cursor.exe launched with `--extensionDevelopmentPath` and `--extensionTestsPath` against `C:\Projects\cursor-xiigen-smart-skills`.
- Cursor loaded the development extension and invoked `xiigen.smartSkills.select` through the Extension Host command path.
- A fresh redacted smoke prompt reached `XiigenExtensionApiService.ProcessHostExtensionRequest` through `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`.
- The command returned Cursor display text: `Core xiigen boundary executed. Semantic quality is not claimed.`
- Debug display was observed in the command result as `xiigen prompt-to-task : input/output <redacted>`.
- A controlled missing-core endpoint error was surfaced as `CONTRACT_ONLY_DOWNGRADE` through the Cursor command entrypoint.

## Proof Booleans

| Field | Value |
|---|---:|
| realCursorRuntimeInvoked | True |
| extensionHostLoaded | True |
| userPromptReachedCore | True |
| visibleCursorAnswerObserved | True |
| debugTraceObserved | True |
| controlledErrorPathObserved | True |
| runtimeProofAcceptedCandidate | True |

## Commands Run

```text
C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe "--extensionDevelopmentPath" "C:\Projects\cursor-xiigen-smart-skills" "--extensionTestsPath" "C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\runtime03-extensionHostQA.js" "--user-data-dir" "C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\cursor-user-data" "--extensions-dir" "C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\cursor-extensions-dir" "--disable-workspace-trust" "--skip-welcome" "--skip-release-notes" "--skip-onboarding" "--disable-telemetry" "--disable-updates" "--log" "trace" "--headless" "--new-window" "C:\Projects\cursor-xiigen-smart-skills"
```

## Key Evidence

- Launch summary: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\runtime03-launch-summary.json`
- Positive request: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T09-14-01-285Z.json`
- Positive core response: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T09-14-01-285Z.json`
- Positive core trace: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-16T09-14-01-285Z.json`
- Positive host trace: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-16T09-14-01-285Z.json`
- Positive entrypoint response: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T09-14-01-285Z.json`
- Controlled error request: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T09-14-26-864Z.json`
- Controlled error host trace: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-16T09-14-26-864Z.json`
- Controlled error entrypoint response: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T09-14-26-864Z.json`
- Stdout log: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\cursor-runtime03.stdout.log`
- Stderr log: `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-121302\cursor-runtime03.stderr.log`

## What this does not prove

- It does not prove Cursor chat participant UI invocation; no chat send/invoke API path was used.
- It does not provide a screenshot or human UI capture; command return display text is the command-output artifact.
- It does not prove token savings, semantic quality, quality improvement, DPO, learnability, training, checkpoint mutation, target promotion, rollback, or full plan completion.

## Next Independent Track

- RUNTIME-04 Codex plugin/app runtime QA after parent review.
