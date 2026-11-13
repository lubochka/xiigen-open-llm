# Row10 Cursor Product Workflow Split/Harness Prep

Это planning-only prep для Row10 `frdp-p01-cursor-product-readiness-product-workflow`. Он не запускает Cursor, Core API, тесты, UI, runtime, eval, training, checkpoint, locator или lifecycle операции. Смысл этого пакета: разложить широкий Cursor product-workflow readiness на маленькие будущие под-сценарии до 10 минут каждый, чтобы следующий sub-agent мог сначала review this prep, а только потом отдельным work order запускать один bounded slice.

## Evidence Read Before This Prep

- Row10 board object: Cursor product workflow readiness across adapter/core, token, quality, lifecycle, checkpoint, locator, and host evidence.
- Row10 current board status: automated/core/API needs concrete work; local real Cursor product workflow not run; joined pass not moved; board category `not_moved`.
- Row10 current next action: build one Cursor product-workflow boundary harness or split the workflow into sub-scenarios, then later run a safe Cursor extension-host command for the same marker.
- Row1 next-selection packet: Row1 local UI route is parked due desktop/foreground condition; Row10 was selected as planning-only Cursor product-workflow split/harness prep.
- Current dashboard/action-plan counts: 28 total scenarios, 14 scoped accepted, 0 product-ready scenarios, 0 formal joined pass, 14 remaining/open, 0 readiness claims opened.
- Existing Cursor context read: Cursor marker-only evidence exists for a different row; marker-only evidence, JSON/log-only evidence, source-only evidence, or opening Cursor alone is not UI proof.
- Row12 Cursor auth/UI script context read: future Cursor UI proof must include auth/session preflight, target-window control, browser login recovery if Cursor opens anonymously, no secrets captured, visible XIIGen Smart Skills panel, textarea `xiigen-smart-skills-prompt`, submit button `xiigen-smart-skills-submit`, and no generic Cursor New Agent/login textbox substitution.

## Current Row10 Meaning

Row10 is not a product-readiness pass. It is an open Cursor workflow row that needs a real same-scenario chain:

1. Path A checks the llm_mvp_core boundary/API/adapter path up to the host boundary where possible.
2. Path B checks the same scenario through the real local Cursor program/UI with visible screenshots or equivalent user-visible evidence.
3. A joined review compares the two paths and keeps claim locks closed unless both paths pass for every required component.

The future Row10 marker should be stable across both paths:

```text
FRDP-P01-ROW10-CURSOR-PRODUCT-WORKFLOW-V1
scenario_id=frdp-p01-cursor-product-readiness-product-workflow
host=Cursor
mode=no-send-local-extension-runtime
```

## Global Future Preconditions

Before any future Row10 execution slice:

1. Create a fresh future run folder under the Row10 scenario execution path.
2. Record the exact sub-scenario id, marker, prompt hash, expected evidence filenames, and claim locks before launch.
3. Load the relevant qa/testing and test-integrity skills/guides if the future work order authorizes UI/test execution.
4. Prove Cursor target-window control before clicks or typing. Global foreground alone is not enough.
5. Run auth/session preflight before panel execution.
6. If Cursor opens anonymously, session is lost, logged-out UI is visible, or wrong account is visible, use browser login recovery:
   - record `LOGIN_RECOVERY_REQUIRED`;
   - click only a visible enabled sign-in option in deterministic priority order;
   - capture only non-secret origin/title/screenshot;
   - stop before password, token, cookie, one-time code, recovery code, or session secret;
   - allow human credential entry outside recorded capture;
   - repeat auth preflight and record `no secrets captured`.
7. Start Path B from a proven visible XIIGen Smart Skills panel state, not from generic Cursor chat, not from the New Agent/login textbox, and not from marker-only proof.

## Split Plan

Each slice below is future work, not executed by this packet.

## Sub-Scenario Coverage Status

- `Row10-S1`: `PREP_ONLY_EXCLUDED_FROM_EXECUTABLE_DUAL_PATH`. S1 is a contract/harness prep slice only; it does not count as executable dual-path coverage and must not claim Path B UI proof.
- `Row10-S2`: `FUTURE_EXECUTABLE_DUAL_PATH_REQUIRED`. Future execution must produce Path A boundary/API evidence and Path B required Cursor screenshot/reference evidence for the same marker and prompt hash, or mark the slice BLOCKED.
- `Row10-S3`: `FUTURE_EXECUTABLE_DUAL_PATH_REQUIRED`. Future execution must produce Path A token/accounting evidence and Path B required Cursor screenshot/reference evidence for the same marker and prompt hash, or mark the slice BLOCKED.
- `Row10-S4`: `FUTURE_EXECUTABLE_DUAL_PATH_REQUIRED_WITH_PATH_A_GUARD_INTERFACE_OR_BLOCKED`. Future execution may use the llm_mvp_core quality-scoring guard boundary artifact named in `phase-03-command-output/04-dotnet-quality-scoring.txt` only with an approved Row10 safe input/path; otherwise mark `BLOCKED_MISSING_PATH_A_BOUNDARY_API_INTERFACE`.
- `Row10-S5`: `BLOCKED_MISSING_PATH_A_LIFECYCLE_BOUNDARY_API_INTERFACE`. Phase 03 evidence says asset lifecycle has `NO_BOUNDARY_COMMAND_DISCOVERED`; this slice cannot execute until a concrete llm_mvp_core lifecycle boundary/API interface is named.
- `Row10-S6`: `FUTURE_EXECUTABLE_DUAL_PATH_REQUIRED_WITH_CHECKPOINT_LOCATOR_INTERFACE_OR_BLOCKED`. Future execution may use the checkpoint/locator readback guard artifact named in `phase-03-command-output/06-dotnet-checkpoint-locator-readback.txt`; locator update/reload remains BLOCKED unless a concrete dry-run/staging locator boundary/API interface is named for Row10.
- `Row10-S7`: `FUTURE_JOINED_REVIEW_ONLY_AFTER_ACCEPTED_S2_TO_S6`. S7 is review-only and cannot pass until required executable slices have accepted Path A and Path B packets or explicit accepted blockers.

| Slice | Human meaning | Max time | Path A boundary | Path B Cursor UI | PASS criteria | BLOCKED criteria |
|---|---|---:|---|---|---|---|
| Row10-S1 | Contract and harness dry prep for the shared marker/prompt. | 10 min | Prep-only: build or validate a non-mutating harness contract that names the exact llm_mvp_core call boundary and expected host adapter refs. | Prep-only: no Path B UI execution, no screenshot proof, and excluded from executable dual-path coverage. Future Path B steps are only scripted for S2-S6. | Contract names same marker, prompt hash, Path A fields, required future Path B screenshot/reference filenames, no-claim locks, next executable slice, and explicit `PREP_ONLY_EXCLUDED_FROM_EXECUTABLE_DUAL_PATH`. | Any missing marker, prompt hash, evidence filename list, claim lock, UI target step for future executable slices, or ambiguity that S1 itself proved Path B UI. |
| Row10-S2 | Adapter/core submit smoke through thin Cursor adapter. | 10 min | Non-mutating llm_mvp_core adapter/API smoke with Row10 marker up to host boundary; request id and response ref are mandatory, and trace absence must be recorded as `TRACE_REF_NOT_SURFACED`. | Real Cursor XIIGen panel: focus textarea `xiigen-smart-skills-prompt`, paste exact Row10 prompt, click `xiigen-smart-skills-submit`, capture required visible result/error screenshot/reference. | Same marker/scenario visible or ref-linked in both paths; extension is thin client; core logic remains in llm_mvp_core; required Path B screenshot/reference exists. | Generic Cursor chat/New Agent used, required visible panel screenshot/reference missing, or extension contains business logic instead of thin adapter. |
| Row10-S3 | Token accounting baseline/adapted fields, no savings claim. | 10 min | Collect same-scenario token accounting fields from boundary/API for Row10 marker; record raw before/after fields if authorized and keep token savings claim false. | Submit the same Row10 token prompt through Cursor panel and capture required visible token/result screenshot/reference; if no visible token/result state exists, mark BLOCKED. | Path A and B evidence preserve same prompt hash and token field names; required Path B screenshot/reference exists; no token savings claim. | Token trace only without same-scenario comparison, required Path B visible evidence missing, or any "savings proven" wording. |
| Row10-S4 | Semantic-quality guard evidence, no quality claim. | 10 min | Use the llm_mvp_core quality-scoring guard boundary artifact `phase-03-command-output/04-dotnet-quality-scoring.txt` only with an approved Row10 safe input/path and leakage fields; if the future work order lacks that exact boundary/API input path, mark `BLOCKED_MISSING_PATH_A_BOUNDARY_API_INTERFACE`. | Submit the same semantic guard prompt through Cursor panel and capture required visible answer/result/error screenshot/reference. | Evidence records approved input provenance, leakage guard fields, Path A boundary/API artifact/ref, required visible Cursor screenshot/reference, and explicit no-quality-claim lock. | Missing approved safe input/path, missing Path A boundary/API interface, leakage guard absent, required Path B visible evidence missing, or any quality-improvement claim. |
| Row10-S5 | Lifecycle dry-run plan for backup/export/import/continue-training evidence later. | 10 min | BLOCKED until a concrete llm_mvp_core lifecycle boundary/API interface is named. Current evidence says asset lifecycle has `NO_BOUNDARY_COMMAND_DISCOVERED`; do not execute backup/export/import/share/continue-training. | Cursor UI may show only a no-send lifecycle status/request result for the same marker; capture required visible screenshot/reference or mark BLOCKED. | Only after a concrete Path A lifecycle boundary/API interface exists: evidence says lifecycle operation not executed now, required Path B screenshot/reference exists, and later local model backup/export/import/continue-training evidence is named. | Missing Path A lifecycle boundary/API interface, missing required Path B screenshot/reference, any real lifecycle mutation, missing later-evidence list, or lifecycle readiness claim. |
| Row10-S6 | Checkpoint and locator staging/dry-run refs, no promotion/update claim. | 10 min | Use the llm_mvp_core checkpoint/locator readback guard artifact `phase-03-command-output/06-dotnet-checkpoint-locator-readback.txt` for checkpoint candidate/readback/fresh-load/direct-promotion rejection; locator dry-run/reload requires a concrete Row10 dry-run/staging boundary/API interface or `BLOCKED_MISSING_PATH_A_BOUNDARY_API_INTERFACE`. | Cursor panel submits the same no-mutation checkpoint/locator status prompt and captures required visible refs/error screenshot/reference. | Evidence separates candidate/staging/dry-run from promotion/update; required Path A boundary/API artifact/ref and required Path B screenshot/reference exist; production state unchanged. | Missing Path A boundary/API interface for locator dry-run/reload, missing required Path B screenshot/reference, production locator/checkpoint mutation, hidden deterministic shortcut, or promotion/update claim. |
| Row10-S7 | Joined Row10 review and readiness lock. | 10 min | Review accepted Path A packets from S2-S6. | Review accepted Path B Cursor UI packets from S2-S6. | All required executable slices have same marker/prompt hash, Path A/B comparison, required screenshot/reference evidence or accepted blocker, and reviewer acceptance; product readiness still requires a separate final readiness review. | Any missing slice, marker mismatch, unreviewed packet, required screenshot/reference missing without BLOCKED, marker-only/UI substitution, hidden shortcut risk, or unresolved blocker. |

## Exact Future Path B UI Steps

These steps apply to executable Row10-S2 through Row10-S6 only after a separate execution work order authorizes that slice.

1. Program/window target: real local Cursor window for the XIIGen Smart Skills extension, preferably an Extension Development Host or isolated run folder named for the Row10 slice.
2. Prelaunch evidence: record Cursor executable/launcher path, run folder, user-data/extensions path, visible windows, modal/error windows, and target-window control status.
3. Auth preflight: prove signed-in/trusted account state or run the browser login recovery branch above. Do not capture secrets.
4. Panel open state: show `XIIGen Smart Skills` panel with prompt textarea and Submit button before entering Row10 prompt.
5. Click/type: click or focus textarea id `xiigen-smart-skills-prompt` or the visible prompt input label; paste/type the exact Row10 slice prompt containing the shared marker and scenario id.
6. Evidence before submit: screenshot `row10-sN-cursor-prompt-entered-before-submit.png`, redacted if needed.
7. Submit: click button id `xiigen-smart-skills-submit` or the visible Submit button inside the XIIGen panel.
8. Evidence after submit: wait for visible result/status/error; capture `row10-sN-cursor-visible-result-or-error.png`; record safe response ref/trace ref only if visible and non-secret.
9. Expected visible state: Cursor remains on the XIIGen Smart Skills panel; the marker or trace/ref is visible or safely linked; no generic Cursor chat/New Agent/login textbox received the prompt.
10. Compare: record Path A/Path B marker, prompt hash, request/ref fields, visible evidence, non-claims, and mismatch/blocker fields in the slice packet.

## Evidence Filenames For Future Slices

Future executable slices should use these names inside their own run folders:

- `row10-s1-contract.json`
- `row10-s1-contract.md`
- `row10-s2-path-a-adapter-core-boundary.json`
- `row10-s2-path-b-cursor-prompt-entered-before-submit.png`
- `row10-s2-path-b-cursor-visible-result-or-error.png`
- `row10-s2-joined-comparison.md`
- `row10-s3-path-a-token-fields.json`
- `row10-s3-path-b-cursor-token-visible-evidence.png`
- `row10-s3-joined-comparison.md`
- `row10-s4-path-a-semantic-guard.json`
- `row10-s4-path-b-cursor-semantic-visible-evidence.png`
- `row10-s4-joined-comparison.md`
- `row10-s5-lifecycle-dry-run-boundary.json`
- `row10-s5-cursor-lifecycle-visible-evidence.png`
- `row10-s5-later-local-model-evidence-required.md`
- `row10-s6-checkpoint-locator-dry-run-boundary.json`
- `row10-s6-cursor-checkpoint-locator-visible-evidence.png`
- `row10-s7-row10-joined-review.md`
- `row10-s7-row10-joined-review.json`

## Boundaries And Claim Locks

- No hidden deterministic shortcuts: no fixture-only, marker-only, source-only, trace-only, schema-only, or precomputed answer proof can stand in for same-scenario dual-path evidence.
- llm_mvp_core owns product logic. Cursor extension code must remain a thin client/adapter boundary.
- Path A automated/boundary/API evidence is not Path B local Cursor UI proof.
- Path B local Cursor UI proof requires real visible Cursor UI/program evidence, screenshots/crops, or equivalent user-visible capture.
- Local model backup/export/import/continue-training evidence is later work and cannot be inferred from lifecycle dry-run prep.
- Product readiness stays false until all required Row10 Path A and Path B slices pass, a joined comparison passes, hidden shortcut risk is closed, and a separate final readiness review accepts the evidence.

Current claim locks:

- `product_readiness_claim=false`
- `formal_joined_pass=false`
- `cursor_host_readiness_claim=false`
- `visible_answer_claim=false`
- `token_savings_claim=false`
- `semantic_quality_claim=false`
- `training_or_learnability_claim=false`
- `lifecycle_readiness_claim=false`
- `checkpoint_promotion_claim=false`
- `locator_update_claim=false`
- `runtime_execution_claim=false`
- `test_execution_claim=false`

## Forbidden In This Prep

This packet did not and must not claim it did:

- run Cursor, browser login, UI automation, runtime, tests, build, eval, heldout, training, checkpoint, locator, lifecycle, or product workflow;
- edit code, boards, plans, dashboards, governance, skills, or existing evidence;
- stage, commit, push, reset, checkout, or clean git state;
- use web, external AI, nested subagents, or old transcript/state authority.

## Review Gate

The next bounded sub-agent should review this prep before any execution. The review should check:

1. Every future slice is <=10 minutes.
2. Every executable slice has Path A and Path B for the same marker/scenario.
3. Auth/session preflight and browser login recovery are explicit and secret-safe.
4. Exact UI click/type/expected evidence steps are present.
5. Claim locks keep product readiness and related claims closed.
6. JSON companion parses and matches the Markdown.
7. This packet stayed planning-only and did not execute product/runtime/UI/tests/training.

## Machine Summary

- artifact_type: `planning_only_split_harness_prep`
- scenario_number: `10`
- scenario_id: `frdp-p01-cursor-product-readiness-product-workflow`
- host: `Cursor`
- verdict: `PASS_ROW10_SPLIT_HARNESS_PREP_CREATED`
- product_execution_authorized_now: `false`
- next_bounded_subagent_task: review this Row10 prep packet, not execute Row10.
