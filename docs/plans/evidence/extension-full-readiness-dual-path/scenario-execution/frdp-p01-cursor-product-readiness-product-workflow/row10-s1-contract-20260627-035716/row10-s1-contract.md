# Row10-S1 Contract/Harness Dry Prep

```text
claim_scope: Row10-S1 planning/contract packet only; no product execution claim
evidence_scope: accepted Row10 split/harness prep, postfix PASS review, and read-only local interface references listed below
count_claimed: 1 Row10-S1 contract Markdown and 1 companion JSON when both files exist
independent_evidence_count: source reads plus allowed JSON/text/whitespace checks after creation
subagent_id_or_NONE: Row10-S1 bounded sub-agent
downgrade_needed: yes for execution readiness; S1 proves only the contract boundary, not executable dual-path readiness
```

## Verdict

PASS for Row10-S1 contract creation only.

S1 is `PREP_ONLY_EXCLUDED_FROM_EXECUTABLE_DUAL_PATH`. It does not count as a product-readiness pass, a Cursor UI pass, a runtime pass, a token/quality/lifecycle/checkpoint/locator pass, or a joined Row10 pass.

## Source Authority

- Accepted prep folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-split-harness-prep-20260627-033319\`
- Postfix review PASS folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-split-harness-prep-postfix-review-20260627-035348\`
- Current contract folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s1-contract-20260627-035716\`
- Current Markdown: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s1-contract-20260627-035716\row10-s1-contract.md`
- Current JSON: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s1-contract-20260627-035716\row10-s1-contract.json`

## What S1 Proves

S1 proves only that the future Row10 executable slices have a written contract for:

- shared Row10 marker and scenario identity;
- Path A automated/boundary/API candidate references before host UI;
- Path B future real local Cursor UI route, including auth/session preflight and browser-login recovery;
- required evidence filenames and claim locks;
- no hidden deterministic shortcut boundary;
- `llm_mvp_core` logic ownership and thin Cursor extension boundary;
- explicit nonclaims for local model backup/export/import/continue-training and every out-of-scope runtime/training/lifecycle operation.

## What S1 Does Not Prove

S1 does not prove:

- Path A executed successfully;
- Path B Cursor UI was opened, clicked, typed into, or screenshot-captured;
- Cursor host readiness, visible answer readiness, product readiness, or formal joined pass;
- token savings, semantic quality improvement, training/learnability, backup/export/import/continue-training, checkpoint promotion, locator update, or lifecycle readiness;
- absence of all hidden shortcuts in implementation. It only defines the checks that future executable slices must pass.

## Shared Scenario Contract

- Scenario id: `frdp-p01-cursor-product-readiness-product-workflow`
- Host: `Cursor`
- Shared marker: `FRDP-P01-ROW10-CURSOR-PRODUCT-WORKFLOW-V1`
- Mode: `no-send-local-extension-runtime`
- Prompt hash status: `PLACEHOLDER_NOT_EXECUTABLE_IN_S1`
- Exact future prompt hash: `BLOCKED_UNTIL_FUTURE_EXECUTION_WORK_ORDER_NAMES_EXACT_ROW10_PROMPT`
- Same-scenario rule: Path A and Path B must use the same marker, scenario id, exact prompt text, and prompt hash in the future executable slice.

## Path A Contract

Path A is the automated/boundary/API check before host UI. S1 does not execute Path A. Future Path A execution is blocked until a separate Row10 execution work order names the exact request JSON, response JSON, trace JSON, prompt text, prompt hash, and expected output filenames.

Concrete candidate references found by read-only inspection:

- `tools\xiigen-extension-core-api.ps1`
  - Script target: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`
  - Core project reference created by the script: `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj`
  - Request type named by the script: `XiigenExtensionRequest`
  - Required input/output shape in the script: `RequestPath`, `ResponsePath`, optional `TracePath`, optional `CoreRoot`
  - Trace fields include `endpoint=xiigen-extension-core-api`, `script_path=tools/xiigen-extension-core-api.ps1`, `call_target=SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`, `user_prompt_sha256`, `host_runtime_claimed=false`, `external_ai_used=false`, and `training_started=false`
- `tools\xiigen-extension-host-gate.ps1`
  - Candidate host-boundary mode: `-Mode HostBoundary -Host cursor`
  - Candidate report ref: `artifacts/host-boundary-cursor.json`
  - Candidate Cursor host repo path checked by the script: `C:\Projects\cursor-xiigen-smart-skills`
  - Candidate Cursor host contract path checked by the script: `C:\Projects\cursor-xiigen-smart-skills\xiigen-extension-contract.json`
  - Boundary fields checked by the script include `thin_client=true`, `uses_xiigen_exe_api=true`, `core_logic_in_llm_mvp_core=true`, `host_training_logic=false`, `external_ai_calls=false`, `deterministic_shortcut=false`, and `hidden_business_logic=false`
- `tools\phase40-extension-core-api.ps1`
  - Candidate check-set/core API contract command only.
  - It explicitly downgrades to `core_api_check_set_runtime_not_full_host` and keeps `fullHostRuntimeEvidence=false`, `semanticQualityClaimed=false`, `externalAiUsed=false`, and `deterministicEvidenceUsedAsLearning=false`.
- Future S4/S6 guard artifact refs inherited from accepted prep:
  - `phase-03-command-output/04-dotnet-quality-scoring.txt` for S4 only if a future approved Row10 safe input/path is named.
  - `phase-03-command-output/06-dotnet-checkpoint-locator-readback.txt` for S6 checkpoint/locator readback guard only if the future work order keeps production state unchanged.

Path A is `BLOCKED_MISSING_EXECUTION_WORK_ORDER_DETAILS` if any of these are absent in the future executable slice:

- exact Row10 prompt text;
- exact prompt hash;
- request JSON filename;
- response JSON filename;
- trace JSON filename;
- accepted Row10-safe input/path for S4 if S4 is the slice;
- concrete lifecycle boundary/API interface for S5;
- concrete locator dry-run/staging boundary/API interface for locator update/reload parts of S6.

## Path B Contract

Path B status: `FUTURE_REAL_LOCAL_CURSOR_UI_ROUTE_SCRIPTED_NOT_EXECUTED`.

Path B is a future real local Cursor UI route. S1 does not execute Path B and does not capture screenshots. Future Path B is blocked unless the execution work order can prove the exact route below.

Program and window preflight:

1. Target program: real local Cursor, preferably an isolated Extension Development Host or a Row10 run folder dedicated to the slice.
2. Target window: visible Cursor window for the XIIGen Smart Skills extension, not a generic Cursor chat, not New Agent, not a login textbox, and not a background/unknown window.
3. Record before interaction: Cursor executable or launcher path, run folder, user-data path, extensions path, visible window title, active target-window control status, and any modal/error windows.
4. If target-window control is not proven, mark Path B `BLOCKED_CURSOR_TARGET_WINDOW_NOT_PROVEN`.

Auth/session preflight and browser-login recovery:

1. Prove signed-in/trusted account state before entering the Row10 prompt.
2. If Cursor opens anonymously, session is lost, logged-out UI is visible, wrong account is visible, or sign-in is required, record `LOGIN_RECOVERY_REQUIRED`.
3. Click only a visible enabled sign-in option in deterministic priority order.
4. Capture only non-secret origin/title/screenshot evidence.
5. Stop before password, token, cookie, one-time code, recovery code, or any session secret.
6. Allow human credential entry outside recorded capture.
7. Repeat auth preflight and record `no secrets captured`.
8. If authenticated/trusted state cannot be proven without secret capture, mark Path B `BLOCKED_AUTH_OR_SECRET_SAFE_RECOVERY_NOT_PROVEN`.

Exact future UI clicks and typing:

1. Open or focus the `XIIGen Smart Skills` panel.
2. Verify the prompt textarea/input is visible as id `xiigen-smart-skills-prompt` or an equivalent visible prompt label inside the XIIGen panel.
3. Click/focus that textarea/input.
4. Paste/type the exact Row10 slice prompt containing the shared marker and scenario id.
5. Capture `row10-sN-cursor-prompt-entered-before-submit.png`.
6. Click button id `xiigen-smart-skills-submit` or the visible Submit button inside the XIIGen panel.
7. Wait for a visible result/status/error.
8. Capture the slice-specific visible result screenshot/reference listed below.
9. Record safe response ref or trace ref only if visible and non-secret.

Expected visible state:

- Cursor remains on the XIIGen Smart Skills panel.
- The shared marker or a safe ref linked to the same prompt hash is visible.
- The prompt was not entered into generic Cursor chat, New Agent, a login textbox, or any unrelated field.
- Required screenshot/reference exists, or the slice is `BLOCKED_REQUIRED_PATH_B_VISIBLE_EVIDENCE_MISSING`.

## Required Future Evidence Filenames

- S1 contract files: `row10-s1-contract.md`, `row10-s1-contract.json`
- S2 Path A: `row10-s2-path-a-adapter-core-boundary.json`
- S2 Path B before submit: `row10-s2-path-b-cursor-prompt-entered-before-submit.png`
- S2 Path B result/error: `row10-s2-path-b-cursor-visible-result-or-error.png`
- S2 joined comparison: `row10-s2-joined-comparison.md`
- S3 Path A: `row10-s3-path-a-token-fields.json`
- S3 Path B: `row10-s3-path-b-cursor-token-visible-evidence.png`
- S3 joined comparison: `row10-s3-joined-comparison.md`
- S4 Path A: `row10-s4-path-a-semantic-guard.json`
- S4 Path B: `row10-s4-path-b-cursor-semantic-visible-evidence.png`
- S4 joined comparison: `row10-s4-joined-comparison.md`
- S5 Path A: `row10-s5-lifecycle-dry-run-boundary.json`
- S5 Path B: `row10-s5-cursor-lifecycle-visible-evidence.png`
- S5 later evidence list: `row10-s5-later-local-model-evidence-required.md`
- S6 Path A: `row10-s6-checkpoint-locator-dry-run-boundary.json`
- S6 Path B: `row10-s6-cursor-checkpoint-locator-visible-evidence.png`
- S7 review Markdown: `row10-s7-row10-joined-review.md`
- S7 review JSON: `row10-s7-row10-joined-review.json`

## Boundary And Nonclaim Locks

No hidden deterministic shortcuts check:

- Fixture-only, marker-only, source-only, trace-only, schema-only, precomputed, or hardcoded-answer evidence cannot substitute for same-scenario dual-path evidence.
- Future review must reject any slice where Path A and Path B do not share the exact marker, prompt, prompt hash, and request/reference chain.
- Future review must reject any extension-side business logic, hidden shortcut, or answer synthesis that bypasses `llm_mvp_core`.

Logic-in-core boundary:

- Product logic must stay in `llm_mvp_core`.
- Candidate core boundary is `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` via `tools\xiigen-extension-core-api.ps1`.
- The host/extension side may package, pass, display, and trace requests/responses, but must not own skill selection, orchestration reasoning, token-savings claims, semantic-quality claims, training, checkpoint promotion, locator updates, or lifecycle mutations.

Extensions-thin boundary:

- Cursor extension must remain a thin client/adapter.
- Expected host-boundary lock is `thin_client=true`, `uses_xiigen_exe_api=true`, `core_logic_in_llm_mvp_core=true`, `host_training_logic=false`, `external_ai_calls=false`, `deterministic_shortcut=false`, and `hidden_business_logic=false`.

Local model backup/export/import/continue-training nonclaims:

- S1 does not run, prove, or prepare a completed local model backup/export/import/continue-training pass.
- S5 may only name later lifecycle evidence after a concrete `llm_mvp_core` lifecycle boundary/API interface exists.
- Any backup/export/import/share/continue-training operation is outside S1 and must remain unclaimed here.

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
- `path_a_execution_claim=false`
- `path_b_ui_execution_claim=false`
- `local_model_backup_export_import_continue_training_claim=false`

## PASS Criteria For This S1 Packet

S1 is PASS only if:

- this Markdown exists;
- companion JSON exists and parses;
- Markdown and JSON agree on title, verdict, paths, claim locks, Path A status, Path B status, and next allowed action;
- contract says S1 is `PREP_ONLY_EXCLUDED_FROM_EXECUTABLE_DUAL_PATH`;
- contract defines what S1 proves and does not prove;
- contract names concrete candidate Path A refs or explicit blockers;
- contract scripts future real local Cursor Path B with program/window/auth/clicks/typing/expected state/evidence;
- contract keeps all claim locks false;
- no Cursor/UI/runtime/tests/builds/training/checkpoint/locator/lifecycle operation is executed.

## BLOCKED Criteria For Future Execution

Future Row10 execution is BLOCKED if:

- exact prompt text or prompt hash is missing;
- Path A request/response/trace filenames are missing;
- Path A interface is not concrete for the chosen slice;
- S5 still lacks a concrete lifecycle boundary/API interface;
- S6 locator dry-run/reload still lacks a concrete dry-run/staging boundary/API interface;
- Cursor target window is not proven;
- auth/session preflight cannot be proven without secret capture;
- required Path B screenshot/reference is missing;
- the prompt goes into generic Cursor chat, New Agent, login textbox, or an unrelated field;
- any hidden deterministic shortcut, host-side business logic, external AI substitution, or marker-only proof is used.

## NEEDS_FIX Criteria

This S1 packet is NEEDS_FIX if:

- JSON does not parse;
- Markdown and JSON disagree on title, verdict, paths, claim locks, Path A status, Path B status, or next allowed action;
- S1 wording implies Path A or Path B has already executed;
- any readiness, token savings, quality, training, lifecycle, checkpoint, locator, runtime, or test claim is opened;
- future Cursor UI steps are vague or omit program/window/auth/click/type/expected-state/screenshot requirements;
- no-blocked wording allows execution despite missing exact prompt hash, Path A files, or Path B visible evidence.

## Next Allowed Action

Next allowed action: assign a bounded Row10-S1 contract reviewer for maximum 10 minutes to review only `row10-s1-contract.md` and `row10-s1-contract.json`, run JSON parse, Markdown/JSON agreement checks, and whitespace checks. No product execution, Cursor UI, runtime invocation, tests/builds beyond JSON/text checks, code edits, board/status edits, training/checkpoint/locator/lifecycle operations, commit, push, web, external AI, or nested subagents.
