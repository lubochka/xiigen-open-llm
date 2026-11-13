# Row13.3a UI Scenario Prep

Status: `ROW13_3A_UI_SCENARIO_PREP_PASS`

This is scenario preparation only. I did not launch Cursor, run UI automation, click submit, train, mutate checkpoints, mutate locators, mutate models, edit production code, stage, commit, push, or claim readiness.

## Chosen Prompt And Skill

- Source: Row13.1a universal-only DPO row `ROW13_1A_DPO_OUTPUT_SKEPTICISM_002`.
- Prompt: `The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim.`
- Prompt SHA-256: `eca453b1fd9e7c925a21287bd52d3b8c907405efac387d7ebb2d8b6db2e60dd4`.
- Expected skill: `output-skepticism`.
- Available skills: `output-skepticism`, `verification-before-completion`, `phase-completion-packager`, `plan-review`.

Source-disjoint caveat: this prompt is copied from a Row13.1a synthetic universal-only row, not from Row13.1c held-out exact prompt text. It can be used for a live Cursor UI duplicate probe of this selected prompt only. It must not be reported as held-out exact prompt proof, full semantic-quality proof, product readiness, token savings, training, or checkpoint promotion.

Row13.1c reference-only alignment: `P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001` expected `output-skepticism`; staged candidate selected `output-skepticism` with `candidatePass=true` and `candidateConfidence=0.75`. Row13.1c also records `exactPromptBlocked=true`, `sourceIdBlocked=true`, `pathBlocked=true`, `filenameBlocked=true`, `candidateOrderBlocked=true`, and `pairIdBlocked=true`.

## Human UI Steps

1. Launch Cursor Extension Development Host with a temporary user profile and temporary extensions directory.
   Use `Cursor.exe` with `--extensionDevelopmentPath` pointing to `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`, an extension test/runner path, `--user-data-dir <temp>`, `--extensions-dir <temp>`, `--disable-workspace-trust`, `--skip-welcome`, `--skip-release-notes`, `--skip-onboarding`, `--disable-telemetry`, `--disable-updates`, `--force-renderer-accessibility`, `--remote-debugging-address=127.0.0.1`, `--remote-debugging-port=<free local port>`, `--new-window`, and workspace path set to the extension repo.

2. Open the real XIIGen Smart Skills panel.
   Invoke `xiigenSmartSkills.panel` (`XIIGen: Open Smart Skills Panel`). Do not use `xiigenSmartSkills.__test.panelSubmitHandler` or `xiigenSmartSkills.runtimeProof.invoke` as Row13.3 real UI proof.

3. Type the chosen prompt into `textarea#xiigen-smart-skills-prompt`.
   Read back the same field value from the same selected webview context. The value must match the prompt exactly and hash to `eca453b1fd9e7c925a21287bd52d3b8c907405efac387d7ebb2d8b6db2e60dd4`.

4. For Row13.3b panel-open/type-only, stop before submit.
   Capture screenshot, DOM/CDP selector proof, target/session/context metadata, prompt value/hash, and runner metadata. Do not click submit and do not claim core invocation or selected skill.

5. For a later authorized submit/result row, click `button#xiigen-smart-skills-submit`.
   Wait for status/result rendering, then capture screenshot, DOM, runtime evidence, core request/response/trace refs, rendered result text/hash, and selected skill evidence.

## Expected Evidence Fields

- `real_cursor_ui_webview_target_found`: true; requires Cursor Extension Development Host, `vscode-webview`/XIIGen target evidence, nonzero candidate target count, and selectors `#xiigen-smart-skills-prompt` and `#xiigen-smart-skills-submit`.
- `prompt_field_value_equals_probe`: true; exact prompt value and SHA-256 must match the selected probe.
- `submit_clicked`: false for Row13.3b; true only in later submit/result phase with `submit_button_click_invoked=true` and `submit_button_clicked_by_probe=true`.
- `panel_submission_path_ran`: false for Row13.3b; true only later with `panel_submission_path_invoked=true`, `entrypointName=xiigenSmartSkills.panel`, and `entrypointType=panel-webview-submit`.
- `core_invoked_with_staged_candidate`: not confirmed in Row13.3a. The Cursor extension evidence I inspected shows runtime evidence and target-locator support, but not a confirmed staged-candidate/checkpoint selector. If Row13.3 execution lacks a documented staged-candidate knob, record `staged_candidate_used=not_supported_or_not_configured` and do not claim Row13.1c staged candidate duplicate.
- `result_rendered`: false for Row13.3b; required later with `result_rendered_in_webview=true`, non-empty result hash/status, and response/trace refs when emitted.
- `selected_skill_equals_expected`: false/not claimed for Row13.3b; required later from core response `SelectedSkills` or `selectedSkillIds` including `output-skepticism`.

## Real UI Versus TEST_ONLY

Real UI proof requires:

- command path `xiigenSmartSkills.panel`;
- visible webview DOM containing `textarea#xiigen-smart-skills-prompt` and `button#xiigen-smart-skills-submit`;
- prompt typed and read back from that same visible webview context;
- submit, if authorized later, caused by clicking the visible submit button;
- panel UI artifact with visible prompt/submit fields and `panelSubmissionPathInvoked=true` after submit.

TEST_ONLY disqualifiers:

- `commandName=xiigenSmartSkills.__test.panelSubmitHandler`;
- `artifactKind=TEST_ONLY_HANDLER_ONLY_BOUNDARY`;
- `handlerPathOnly=true`;
- `fullUiEvidence=false`;
- `visibleWebviewObserved=false`;
- `desktopTextareaTypingProof=false`;
- `panelSubmissionPathInvoked=false`;
- `evidenceBoundary=handler_path_only_not_visible_webview_ui_proof`.

Handler-only evidence can prove a boundary path, but it is not Row13.3 live Cursor UI duplicate evidence.

## Row12 Runner Fit

Existing runner:

`C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\ROW12-cursor-panel-visibility-runner.ps1`

Useful as-is for:

- Extension Development Host launch with temp `--user-data-dir` and `--extensions-dir`;
- real `xiigenSmartSkills.panel` command invocation;
- CDP/WebView selector inspection;
- screenshot capture;
- `PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR` propagation;
- `-TypeabilityProofOnly` using generated `row12-ui-typeability-probe-$stamp`;
- `-SubmitResultProof` using generated `row12-ui-submit-result-probe-$stamp`.

Not enough as-is for exact Row13.3 chosen prompt:

- no observed parameter for arbitrary Row13 prompt injection;
- no observed parameter for staged candidate/checkpoint selection;
- stock `-TypeabilityProofOnly` proves typeability for a generated Row12 probe only.

Command shape for a panel-open/type-only run after adding/using an exact prompt override:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\ROW12-cursor-panel-visibility-runner.ps1" `
  -CursorExe "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe" `
  -ExtensionRoot "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills" `
  -EvidenceBase "<new Row13.3b evidence folder>" `
  -TypeabilityProofOnly `
  -PanelHoldMs 120000 `
  -KeepCursorOpen
```

Pass condition warning: this command shape is only Row13.3b PASS if the runner/wrapper sets the exact selected Row13 prompt. If stock Row12 generates `row12-ui-typeability-probe-$stamp`, the supported claim is only Row12 probe typeability, not Row13.3 prompt equality.

## Evidence Files For Execution Phase

- start-info JSON with command line, temp dirs, remote debugging port, run mode, and `no_git_action_run=true`;
- visible screenshot PNG;
- runner JSON report;
- runner MD report;
- CDP DOM/protocol snapshots for the selected webview target/context;
- runtime evidence core request/response/trace refs when submit is authorized;
- stdout/stderr logs;
- copied temp runtime path diagnostics.

## No-Overclaim Rules

After Row13.3b panel-open/type-only, the maximum supported claim is: Cursor Extension Development Host opened the XIIGen panel and the visible prompt textbox contained the exact chosen Row13 prompt, if exact value/hash evidence exists.

Forbidden after Row13.3b: selected skill, core invocation, staged candidate, semantic quality, full UI duplicate, readiness, token savings, training, checkpoint promotion, locator update.

Even after a later submit/result phase, do not claim full semantic quality, common model readiness, token savings, training success, checkpoint promotion, all-host readiness, or held-out exact-prompt duplicate.

## Next Small Work Order For Row13.3b

Run Row13.3b panel open/type only. Allowed: launch Cursor Extension Development Host with temp profile/extensions, open `xiigenSmartSkills.panel`, type the chosen prompt into `textarea#xiigen-smart-skills-prompt`, capture screenshot/DOM/runtime metadata proving real UI target and exact field value/hash. Forbidden: click submit, run submit/result automation, train, mutate checkpoints/locators/models, edit production code, git stage/commit/push, or claim readiness/selected skill.

Precondition: use a runner or wrapper that can set the exact selected Row13 prompt. Stock Row12 `-TypeabilityProofOnly` alone is insufficient for Row13.3 chosen-prompt equality.
