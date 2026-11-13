# CURSOR-PATH-B-UI-SUBMIT-TO-CORE-11: run report

claim_scope: один bounded Path B submit-to-core запуск Row12 в реальном локальном Cursor Extension Development Host через UI route `XIIGen: Select Smart Skill`.
evidence_scope: новая evidence folder `cursor-path-b-ui-submit-to-core-20260628T185708Z`; 5 screenshots визуально проверены; `report.json`, `trace-correlation.json`, `helper-exit.json`, copied runtime request/core-trace/host-trace/entrypoint-response files прочитаны; prior join review and prior Path B route/input/cancel packet прочитаны до запуска.
count_claimed: 1 UI submit attempt, 5 screenshots, 6 copied runtime evidence files, 0 product code edits, 0 governance edits, 0 commits/pushes.
independent_evidence_count: 14 inspected evidence records for the narrow submit-to-core claim.
subagent_id_or_NONE: CURSOR-PATH-B-UI-SUBMIT-TO-CORE-11
downgrade_needed: нет для узкого утверждения "real Cursor UI submit reached core for the same Row12 marker/scenario/pair"; да для product readiness, full Cursor readiness, token savings, semantic quality, training, checkpoint promotion, locator update.

## Verdict

PASS_NARROW_PATH_B_UI_SUBMIT_REACHED_CORE_SAME_MARKER.

Это не product readiness и не full Cursor readiness. Это доказывает только узкое: в реальном Cursor Extension Development Host был выполнен user-facing UI route `XIIGen: Select Smart Skill`, Row12 marker/scenario/pair был введен в XIIGen input, Enter из этого input был использован как submit, и same-run runtime evidence показывает core boundary для того же request/correlation.

## UI actions actually performed

1. Activated target Cursor Extension Development Host by exact title/process.
2. Pressed `Esc` once to clear transient UI state.
3. Pressed `F1` to open Cursor command palette.
4. Typed `xiigen select smart skill`.
5. Pressed `Enter` to open `XIIGen: Select Smart Skill`.
6. Typed safe Row12 marker/scenario/pair text:
   `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1 | frdp-p01-cursor-token-savings-baseline | frdp-p01-row12-cursor-token-baseline-v1`
7. Pressed `Home` so the marker prefix was visible in the screenshot.
8. Pressed `Enter` inside the XIIGen input to submit.
9. Waited for post-submit state and runtime files.

There was one wrapper/logging launch failure before the helper started because `logs` did not exist for redirection. It performed no UI action and is not counted as a UI scenario attempt.

## Visible/UI evidence

Screenshots:

- `screenshots/01-target-extension-development-host-top.png`: target Cursor Extension Development Host / XIIGen Smart Skills surface.
- `screenshots/02-command-palette-xiigen-select-smart-skill-top.png`: command palette filtered to `XIIGen: Select Smart Skill`.
- `screenshots/03-row12-marker-in-xiigen-input-top.png`: XIIGen input contains the Row12 marker/scenario/pair.
- `screenshots/04-after-submit-result-top.png`: post-submit Cursor/XIIGen surface shows `Core API response recorded for adapter functionality` and the beginning of the returned core response area.
- `screenshots/05-after-submit-bottom-right.png`: post-submit lower-right crop; not used as primary proof beyond safe post-submit state.

No credential/login prompt was encountered. No password, token, cookie, or credential was typed. Typed input was synthetic marker/scenario/pair text only.

## Structured same-run evidence

Copied runtime evidence source root:

`C:\Users\lubao\AppData\Roaming\Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills\xiigen-cursor-host-runtime`

Copied into this evidence folder:

- `runtime-evidence/xiigen-cursor-xiigen.smartSkills.select-request-2026-06-28T19-02-10-148Z.json`
- `runtime-evidence/xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-28T19-02-10-148Z.json`
- `runtime-evidence/xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-28T19-02-10-148Z.json`
- `runtime-evidence/xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-28T19-02-10-148Z.json`
- `runtime-evidence/xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-28T19-02-10-148Z.json`
- `runtime-evidence/xiigen-cursor-host-runtime-latest.json`

Correlation facts verified:

- Request file `UserPromptOrTaskText` contains `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`.
- The same request file contains `frdp-p01-cursor-token-savings-baseline`.
- The same request file contains `frdp-p01-row12-cursor-token-baseline-v1`.
- Request `HostFacts.host_entrypoint` is `command:xiigen.smartSkills.select`.
- Core trace endpoint is `xiigen-extension-core-api`.
- Core trace call target is `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- Core trace `request_path` points to the same `xiigen.smartSkills.select-request-2026-06-28T19-02-10-148Z.json`.
- Core trace `response_success=true`.
- Host trace `entrypointName=xiigen.smartSkills.select`.
- Host trace `entrypointType=command`.
- Host trace `coreBoundaryCorrelation.sameRunCorrelationMatched=true`.
- Correlation ID is `cursor-2026-06-28T19:02:10.149Z` across request/core/host evidence.

Therefore:

- `submitted_to_core=true`
- `same_marker=true`
- `no_secrets_captured=true`
- `external_ai_used=false`
- `readiness_claimed=false`
- `token_savings_claimed=false`
- `semantic_quality_claimed=false`

## Non-claims

I did not verify product readiness.
I did not verify full Cursor readiness.
I did not verify token savings.
I did not verify semantic quality.
I did not verify training.
I did not verify checkpoint promotion.
I did not verify locator update.
I did not edit product code.
I did not edit AGENTS/CLAUDE/skills/governance/plan files.
I did not commit or push.

Boundary literals:

- `boundary/API evidence != host readiness`
- `marker-only != full local desktop pass`
- `trace != savings`
- `schema != training`
- `checkpoint candidate != promotion`

## Next smallest bounded action

Independent review/join worker should read this packet plus the prior Path A/Path B join packet and decide the updated joined claim. The likely allowed claim is narrow same-scenario Cursor UI submit-to-core proof for Row12, with product/runtime/quality claims still closed unless separately proven.
