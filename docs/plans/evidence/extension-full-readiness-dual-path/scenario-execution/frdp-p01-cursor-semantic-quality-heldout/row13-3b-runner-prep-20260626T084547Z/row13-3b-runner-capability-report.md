# Row13.3b Runner Capability Prep

status: `ROW13_3B_RUNNER_PREP_PASS`

This is prep only: do not launch Cursor, do not click submit, do not train, do not mutate checkpoints/locators/models, do not edit production extension source, and do not perform git actions.

## Finding

The Row12 runner does not currently support an exact prompt override for `TypeabilityProofOnly`.

Evidence inspected:
- Runner parameters include `-TypeabilityProofOnly`, `-SubmitResultProof`, timeout/hold options, `-EvidenceBase`, `-ExtensionRoot`, `-CursorExe`, and `-KeepCursorOpen`; no prompt parameter is exposed.
- The runner assigns `TypeabilityProofOnly` text with `row12-ui-typeability-probe-$stamp`.
- The runner passes that local probe text into the CDP/WebView typeability path.

The stock runner can prove typeability for its generated Row12 probe. It cannot, by itself, prove that the Row13.3 exact prompt was opened/typed.

## Minimal Safe Path

A source fix is not required for Row13.3b prep. The minimal safe path is an evidence-local wrapper:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-3b-runner-prep-20260626T084547Z\ROW13-3b-exact-prompt-typeability-wrapper.ps1`

The wrapper copies the Row12 runner into this evidence folder and patches only the local copy so `TypeabilityProofOnly` uses this exact prompt:

`The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim.`

Expected prompt SHA-256:

`eca453b1fd9e7c925a21287bd52d3b8c907405efac387d7ebb2d8b6db2e60dd4`

## Next Row13.3b Command

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-3b-runner-prep-20260626T084547Z\ROW13-3b-exact-prompt-typeability-wrapper.ps1" -Run
```

This is the next Row13.3b command for the actual open/type run. It will launch Cursor when run with `-Run`; it was not run during this prep.

## Expected Runtime And Cleanup

Max expected run time: about 5-6 minutes with `PanelHoldMs=120000`, `HostReadyTimeoutSec=90`, `WindowReadyTimeoutSec=45`, `PanelSettleSec=4`, and CDP timeout 30s.

Cleanup expectation: the recommended command does not pass `-KeepCursorOpen`, so the copied Row12 runner should close launched Cursor processes in its `finally` cleanup. If `-KeepCursorOpen` is added for manual inspection, the operator must close Cursor manually. Evidence artifacts remain under the Row13.3b folder.

## Human-Readable Next Work Order

Run the evidence-local wrapper command above for Row13.3b actual Cursor panel open/type proof. Allowed actions: launch Cursor Extension Development Host with temp profile/extensions, open the real `xiigenSmartSkills.panel`, type the exact Row13.3 prompt into `textarea#xiigen-smart-skills-prompt` via `TypeabilityProofOnly`, and capture screenshot/DOM/CDP/readback/hash/session/context evidence. Forbidden actions: submit click, training, checkpoint/locator/model mutation, production source edits, git actions, readiness claims, selected-skill claims, staged-candidate claims, or semantic-quality claims.

Required pass evidence: readback value equals the exact prompt, readback/probe hash equals `eca453b1fd9e7c925a21287bd52d3b8c907405efac387d7ebb2d8b6db2e60dd4`, and no submit/core/panel-submission path was invoked.

Read-back phrases: exact prompt override; TypeabilityProofOnly; do not launch Cursor; next Row13.3b command.
