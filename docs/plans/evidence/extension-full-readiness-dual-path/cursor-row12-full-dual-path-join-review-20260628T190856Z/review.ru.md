# CURSOR-ROW12-FULL-DUAL-PATH-JOIN-REVIEW-12

claim_scope: независимая join-review проверка Cursor Row12 для узкого Dual-Path Same-Scenario: Path A automated/API/core path + Path B real local Cursor UI submit-to-core для того же marker/scenario/pair.
evidence_scope: прочитаны и сопоставлены Path A review/report, Path A runner JSON, новый Path B submit packet, trace-correlation, runtime evidence, prior Path B route packet, prior join review, helper exit, screenshot metadata; визуально проверены 4 ключевых Path B screenshot.
count_claimed: 1 bounded join review; 0 product/code edits; 0 governance/skills/plan edits; 0 runtime/UI reruns by this reviewer; 0 commits/pushes.
independent_evidence_count: 26 unique inspectable evidence records/files checked by this reviewer; 4 of the 5 Path B screenshots were visually inspected.
subagent_id_or_NONE: CURSOR-ROW12-FULL-DUAL-PATH-JOIN-REVIEW-12
downgrade_needed: нет для узкого Cursor Row12 dual-path same-scenario submit-to-core proof; да для product readiness, full Cursor readiness, token savings, semantic quality, training, checkpoint promotion, locator update, and broad full local desktop pass.

## Verdict

PASS_NARROW_CURSOR_ROW12_DUAL_PATH.

Доказательство теперь поддерживает ровно узкое утверждение:

Cursor Row12 имеет same-scenario dual-path evidence for the same marker/scenario/pair:

- Path A: Cursor Extension Host automated/API/core path через `xiigenSmartSkills.runtimeProof.invoke` дошел до `llm_mvp_core` и сохранил core/request/response/trace refs.
- Path B: real local Cursor Extension Development Host UI route через `XIIGen: Select Smart Skill` принял Row12 marker/scenario/pair, был submitted через UI input, и copied runtime evidence показывает submit-to-core for the same marker/scenario/pair.
- Join: Path A и Path B совпадают по `marker`, `scenario_id`, и `pair_id`.

Это не product readiness и не full Cursor extension readiness.

## Same-Scenario Anchors

| Field | Path A | Path B | Result |
| --- | --- | --- | --- |
| marker | `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1` | `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1` | MATCH |
| scenario_id | `frdp-p01-cursor-token-savings-baseline` | `frdp-p01-cursor-token-savings-baseline` | MATCH |
| pair_id | `frdp-p01-row12-cursor-token-baseline-v1` | `frdp-p01-row12-cursor-token-baseline-v1` | MATCH |

Path A command: `xiigenSmartSkills.runtimeProof.invoke`.

Path B command/UI route: `xiigen.smartSkills.select` opened from the Cursor command palette as `XIIGen: Select Smart Skill`.

## Path A Evidence

Source files reviewed:

- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-host-visible-runtime-review-20260628T183240Z/review.ru.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/reference-preservation-row12-cursor-fix-20260628T153936Z/row12-host-visible-runner-report.json`

Path A structured facts:

- `status=PASS_PATH_A_ROW12_HOST_RUNNER_PACKET_CREATED`
- `passed=true`
- `path_a_host_runner_attempted=true`
- `command_invoked=xiigenSmartSkills.runtimeProof.invoke`
- `required_trace_refs_present=true`
- `latest_response_present=true`
- `same_run_correlation_matched=true`
- `path_b_local_real_app_ui_evidence_claimed=false`
- `token_savings_claimed=false`
- `semantic_quality_claimed=false`
- `readiness_claimed=false`
- `external_ai_used=false`

All 7 Path A refs named by the runner report existed during this review:

- core request ref
- core response ref
- core trace ref
- host trace ref
- entrypoint response ref
- redacted trace ref
- latest ref

## Path B Evidence

Source files reviewed:

- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-path-b-ui-submit-to-core-20260628T185708Z/report.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-path-b-ui-submit-to-core-20260628T185708Z/trace-correlation.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-path-b-ui-submit-to-core-20260628T185708Z/run-report.ru.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-path-b-ui-submit-to-core-20260628T185708Z/logs/helper-exit.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-path-b-real-ui-scenario-20260628T183924Z/report.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/cursor-path-a-b-join-review-20260628T185039Z/join-review.json`

Path B structured facts:

- `status=PASS_UI_SUBMIT_REACHED_CORE_SAME_MARKER`
- `pass=true`
- `submitted_to_core=true`
- `same_marker=true`
- `no_secrets_captured=true`
- `external_ai_used=false`
- `readiness_claimed=false`
- `token_savings_claimed=false`
- `semantic_quality_claimed=false`
- `product_code_edits=0`
- `blocker=null`
- helper exit code: `0`

Path B screenshots:

- 5 screenshot files exist in the submit packet.
- This reviewer visually inspected screenshots 01-04.
- Screenshot 01 shows Cursor Extension Development Host / XIIGen Smart Skills surface.
- Screenshot 02 shows Cursor command palette filtered to `xiigen select smart skill` with `XIIGen: Select Smart Skill`.
- Screenshot 03 shows the Row12 marker/scenario/pair text in the XIIGen input.
- Screenshot 04 shows post-submit XIIGen surface with `Core API response recorded for adapter functionality`.
- Screenshot 05 was existence/metadata checked but not used as primary visual proof by this reviewer.

Path B runtime evidence:

- 6 copied runtime JSON files exist under `runtime-evidence/`.
- Directly parsed by this reviewer: request, core trace, host trace, core response, entrypoint response.
- The request file contains the Row12 marker, scenario id, and pair id.
- The request file has `HostFacts.host_entrypoint=command:xiigen.smartSkills.select`.
- The core trace has `endpoint=xiigen-extension-core-api`.
- The core trace has `call_target=SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- The core trace has `response_success=true`.
- The host trace has `entrypointName=xiigen.smartSkills.select`.
- The host trace has `entrypointType=command`.
- The host trace has `coreBoundaryCorrelation.sameRunCorrelationMatched=true`.
- The entrypoint response has `coreBoundaryReached=true`.

Important boundary: the runtime files themselves still do not claim full Cursor host UI proof. The UI proof comes from the separate Path B scenario packet plus screenshots; the runtime files prove the submit reached core for that same scenario.

## Non-Claims

I did not verify product readiness.
I did not verify full Cursor extension readiness.
I did not verify token savings.
I did not verify semantic quality.
I did not verify training.
I did not verify checkpoint promotion.
I did not verify locator update.
I did not run Cursor or rerun the UI scenario in this review.
I did not use external AI/web.
I did not capture secrets.
I did not edit product code.
I did not edit governance, skills, plans, or source files.
I did not commit or push.

Boundary literals preserved:

- `boundary/API evidence != host readiness`
- `marker-only != full local desktop pass`
- `trace != savings`
- `schema != training`
- `checkpoint candidate != promotion`

## Commands Run

- `rg --files` over the required evidence folders.
- `Get-Content -Raw` for Path A review and Path A runner JSON.
- `Get-Content -Raw` for new Path B report, trace-correlation, run report, prior join JSON, and prior route packet.
- PowerShell `ConvertFrom-Json` summaries for Path A/Path B same marker/scenario/pair comparison.
- PowerShell `ConvertFrom-Json` summaries for Path B request/core-trace/host-trace/core-response/entrypoint-response runtime evidence.
- PowerShell `Test-Path`/`Get-Item` checks for Path A refs, Path B runtime JSON files, and Path B screenshots.
- `view_image` visual inspection for screenshots 01-04.
- `[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')` for output folder timestamp.
- `Get-Date -AsUTC -Format "yyyyMMddTHHmmssZ"` failed because this PowerShell lacks `-AsUTC`; no evidence claim depends on that failed command.
- `git diff --check` ran after writing the evidence packet; exit code `0`; output recorded in `git-diff-check.log`.

## Next Smallest Bounded Action

No implementable blocker remains inside this review slice.

The next smallest bounded action is parent/chain integration: consume this PASS_NARROW_CURSOR_ROW12_DUAL_PATH packet in the broader readiness ledger while preserving the non-claims above.
