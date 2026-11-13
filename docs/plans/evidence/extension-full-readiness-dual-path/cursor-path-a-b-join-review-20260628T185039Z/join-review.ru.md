# CURSOR-PATH-A-B-JOIN-REVIEW-10

claim_scope: Независимый join-review для Cursor Row12: Path A host command/core-boundary packet плюс Path B real local Cursor UI route/input/cancel packet.
evidence_scope: Локальные evidence-файлы в `C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\cursor-host-visible-runtime-review-20260628T183240Z`, `reference-preservation-row12-cursor-fix-20260628T153936Z`, и `cursor-path-b-real-ui-scenario-20260628T183924Z`; 4 PNG screenshots просмотрены визуально этим reviewer.
count_claimed: 1 join-review packet, 0 product/code edits, 0 UI reruns, 0 submit-to-core attempts.
independent_evidence_count: 12 local evidence records/checks: Path A review, Path A report JSON, Path B scenario prep, Path B report JSON, Path B run report, Path B screenshot listing, 4 screenshot visual inspections, Path A parsed summary, Path B parsed summary.
subagent_id_or_NONE: CURSOR-PATH-A-B-JOIN-REVIEW-10
downgrade_needed: Да. Evidence supports only narrow joined partial Cursor proof. It does not support product readiness or full same-scenario UI submit-to-core proof.

## Verdict

READY_FOR_NEXT_UI_SUBMIT_SLICE.

Path B packet is valid narrow evidence for a real local Cursor UI route/input/cancel slice. It is not broken and does not need repair for that narrow claim.

The join is allowed only as a narrow partial Cursor proof:

- Path A proves core-submit/core-boundary behavior through the host command/API route.
- Path B proves the real local Cursor UI can reach the XIIGen user-facing route, accept the same Row12 marker/scenario/pair anchors in the input, and cancel without submit.
- Same-scenario UI submit-to-core is not proven.

Exact remaining blocker:

`Path B submit-to-core not proven`.

Smallest next bounded action:

Run a real local Cursor UI submit/capture slice for the same Row12 marker/scenario/pair, with safe redaction/no secrets, exact screenshots/log refs, and structured evidence proving whether the UI submit reaches core for the same scenario.

## What Path A Proves

Reviewed Path A evidence:

- `C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\cursor-host-visible-runtime-review-20260628T183240Z\review.ru.md`
- `C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\reference-preservation-row12-cursor-fix-20260628T153936Z\row12-host-visible-runner-report.json`

Path A report parsed as valid JSON and supports these narrow facts:

- `passed=true`
- `path_a_host_runner_attempted=true`
- `command_invoked=xiigenSmartSkills.runtimeProof.invoke`
- `required_trace_refs_present=true`
- `same_run_correlation_matched=true`
- `path_b_local_real_app_ui_evidence_claimed=false`
- `token_savings_claimed=false`
- `semantic_quality_claimed=false`
- `readiness_claimed=false`

Path A therefore proves only a host command/API/core-boundary submit path for Row12. It does not prove local UI interaction, Path B, product readiness, token savings, semantic quality, training, or checkpoint promotion.

## What Path B Proves

Reviewed Path B evidence folder:

`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\cursor-path-b-real-ui-scenario-20260628T183924Z`

Reviewed required files:

- `scenario-prep.ru.md`
- `report.json`
- `run-report.ru.md`
- screenshot listing for the evidence folder

Path B `report.json` parsed as valid JSON and supports these narrow facts:

- `pass=true`
- `status=PASS_PATH_B_REAL_CURSOR_UI_ROUTE_EVIDENCE_CREATED`
- `scenario_id=frdp-p01-cursor-token-savings-baseline`
- `pair_id=frdp-p01-row12-cursor-token-baseline-v1`
- `marker=FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
- `screenshot_count=4`
- `no_secrets_captured=true`
- `external_ai_used=false`
- `submitted_to_core=false`

Screenshots were independently opened and visually checked:

- `01-target-extension-development-host-top.png`: shows Cursor Extension Development Host / XIIGen surface.
- `02-command-palette-xiigen-select-smart-skill-top.png`: shows command palette route with `XIIGen: Select Smart Skill`.
- `03-row12-marker-in-xiigen-input-top.png`: shows Row12 marker/scenario/pair text in XIIGen input.
- `04-after-cancel-no-submit-top.png`: shows the UI after cancellation/no-submit state.

Path B therefore proves a real local Cursor UI route/input/cancel path for the same Row12 anchors. It does not prove backend/core submission from the UI.

## Same-Scenario Join

The Path A and Path B packets share the same Row12 anchors:

- marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
- scenario_id: `frdp-p01-cursor-token-savings-baseline`
- pair_id: `frdp-p01-row12-cursor-token-baseline-v1`
- Path A expected prompt hash: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`

The shared anchors allow a narrow join between:

- Path A: host/API command reaches core for Row12.
- Path B: real Cursor UI route accepts the same Row12 marker/scenario/pair and cancels.

They do not allow a full same-scenario submit claim because the Path B packet explicitly records `submitted_to_core=false`.

## Not Claimed

I did not verify product readiness.
I did not verify full Cursor readiness.
I did not verify token savings.
I did not verify semantic quality.
I did not verify training.
I did not verify checkpoint promotion.
I did not run a new UI scenario.
I did not submit the Path B UI input to core.
I did not use external AI/web.
I did not capture secrets.
I did not edit product/code/governance files.

Boundary literals preserved:

- `boundary/API evidence != host readiness`
- `marker-only != full local desktop pass`
- `trace != savings`
- `schema != training`
- `checkpoint candidate != promotion`

