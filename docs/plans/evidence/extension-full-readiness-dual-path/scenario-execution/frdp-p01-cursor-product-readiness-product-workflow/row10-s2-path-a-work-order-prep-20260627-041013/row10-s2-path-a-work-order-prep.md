# Row10-S2 Path A Work-Order Prep

```text
claim_scope: Row10-S2 Path A work-order prep only; no product/API scenario execution, Cursor UI, runtime, tests/builds except JSON/text checks, code edits, board/status edits, training, checkpoint, locator, lifecycle, commit, push, web, external AI, or nested subagents
evidence_scope: accepted Row10 split/harness prep, Row10-S1 contract PASS packet, Row10-S1 review PASS packet, and read-only inspection of exact llm_mvp_core boundary contract/script files
count_claimed: 1 Markdown work-order packet and 1 companion JSON packet after both files exist
independent_evidence_count: source reads plus allowed JSON/text/whitespace checks after creation
subagent_id_or_NONE: bounded-subagent-row10-s2-path-a-work-order-prep
downgrade_needed: yes for all execution/readiness claims; this packet proves only that a future Path A work order is prepared for review
```

## Вердикт

PASS for work-order prep, not for execution.

Этот пакет только готовит будущий Row10-S2 Path A automated/boundary/API work order. Он не запускал `tools\xiigen-extension-core-api.ps1`, не вызывал `XiigenExtensionApiService`, не запускал Cursor, не запускал runtime/tests/builds, не менял code/board/status, не делал training/checkpoint/locator/lifecycle и не делал commit/push.

Следующее действие: сначала отдельный bounded reviewer проверяет этот work-order packet. Только если review verdict is PASS, другой отдельный bounded sub-agent может выполнить Path A по этому work order.

## Человеческий смысл S2 Path A

Row10-S2 проверяет узкую automated/boundary/API сторону Cursor product workflow: запрос от тонкого Cursor adapter должен доходить до `llm_mvp_core`, а product logic должен оставаться в core. Это не Path B UI proof и не product readiness.

Path A будущего исполнителя должен проверить:

- same marker/scenario/prompt hash сохранены в request, response, trace и report;
- known core boundary command/interface используется, если review разрешит execution;
- `llm_mvp_core` owns logic;
- Cursor extension remains thin;
- no hidden deterministic shortcut substitutes for the same-scenario boundary;
- no candidate-position-like scaffold evidence masquerades as semantic quality/training/readiness.

## Источники

- Accepted Row10 prep: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-split-harness-prep-20260627-033319\`
- Row10-S1 contract PASS: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s1-contract-20260627-035716\`
- Row10-S1 contract review PASS: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s1-contract-review-20260627-040559\`
- Core API wrapper inspected read-only: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- Thin host boundary wrapper inspected read-only: `C:\Projects\llm_mvp_core\tools\xiigen-extension-host-gate.ps1`
- API contract inspected read-only: `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs`
- README API boundary inspected read-only: `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\README.md`
- Narrow CheckSetResults builder lines inspected read-only: `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs`

## Exact Prompt

Hash strategy: SHA256 over UTF-8 bytes of the exact text below, with LF line separators and no trailing newline.

Prompt hash: `e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506`

```text
FRDP-P01-ROW10-CURSOR-PRODUCT-WORKFLOW-V1
scenario_id=frdp-p01-cursor-product-readiness-product-workflow
slice=Row10-S2
host=Cursor
mode=no-send-local-extension-runtime
task=Adapter/core submit smoke through thin Cursor adapter. Process this as a boundary/API request only: keep product logic in llm_mvp_core, do not claim Cursor UI readiness, product readiness, token savings, semantic quality, training, checkpoint promotion, locator update, or lifecycle readiness. Return safe refs/status only.
```

## Redaction And Hash Strategy

- The future request JSON must include `UserPromptOrTaskText` with the exact prompt text because `XiigenExtensionRequest` requires prompt/task text.
- The future trace/report must compare by `user_prompt_sha256` / `prompt_sha256`, not by copying raw prompt into every artifact.
- The future trace must keep `user_prompt_redacted=true`, `raw_private_prompt_persisted=false`, and `contains_secrets=false`.
- The prompt is classified as a non-secret scenario marker prompt. No credentials, tokens, cookies, private corpus rows, model internals, or raw private user content may be included.
- Any response/debug text that exposes secrets, private paths beyond allowed evidence refs, raw corpus rows, or model internals is a FAIL.

## Known Boundary Interface

Boundary status: `KNOWN_FROM_S1_AND_READ_ONLY_CONTRACT_INSPECTION`.

Future Path A command/interface to use after review PASS:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\xiigen-extension-core-api.ps1 -RequestPath "<future-run-folder>\row10-s2-path-a-request.json" -ResponsePath "<future-run-folder>\row10-s2-path-a-response.json" -TracePath "<future-run-folder>\row10-s2-path-a-trace.json" -CoreRoot "C:\Projects\llm_mvp_core"
```

Known target:

- Script: `tools\xiigen-extension-core-api.ps1`
- Parameters: `RequestPath`, `ResponsePath`, optional `TracePath`, optional `CoreRoot`
- C# target: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest(XiigenExtensionRequest)`
- Core project reference: `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj`
- Evidence boundary: core-owned endpoint invocation only; no host runtime claim.

If the future executor cannot find this script, cannot find `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj`, or cannot create request/response/trace in the future run folder, it must mark `BLOCKED_MISSING_BOUNDARY_INTERFACE` and list exact missing fields instead of inventing another interface.

## Future Request JSON Shape

Filename: `row10-s2-path-a-request.json`

Shape:

```json
{
  "HostName": "Cursor",
  "HostType": "cursor",
  "Method": "prompt-to-task",
  "UserPromptOrTaskText": "<exact prompt text above>",
  "HostFacts": {
    "scenario_id": "frdp-p01-cursor-product-readiness-product-workflow",
    "row": "Row10",
    "slice": "Row10-S2",
    "path": "A",
    "host": "Cursor",
    "mode": "no-send-local-extension-runtime",
    "marker": "FRDP-P01-ROW10-CURSOR-PRODUCT-WORKFLOW-V1",
    "cursor_ui_execution": "false",
    "host_runtime_claimed": "false"
  },
  "DebugEnabled": true,
  "RequestedSkills": [],
  "AvailableSkills": [],
  "CheckSetInputs": {
    "prompt_sha256": "e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506",
    "row10_s2_path_a_boundary": "adapter_core_submit_smoke",
    "logic_owner_required": "llm_mvp_core",
    "extension_thin_required": "true",
    "no_hidden_deterministic_shortcut": "true",
    "no_candidate_position_like_masquerade": "true",
    "product_readiness_claim": "false",
    "cursor_ui_readiness_claim": "false",
    "token_savings_claim": "false",
    "semantic_quality_claim": "false",
    "training_learnability_claim": "false",
    "checkpoint_promotion_claim": "false",
    "locator_update_claim": "false",
    "lifecycle_readiness_claim": "false"
  },
  "Provenance": {
    "source_row10_split_prep": "row10-split-harness-prep-20260627-033319",
    "source_row10_s1_contract": "row10-s1-contract-20260627-035716",
    "source_row10_s1_review": "row10-s1-contract-review-20260627-040559",
    "work_order_packet": "row10-s2-path-a-work-order-prep-20260627-041013",
    "raw_prompt_classification": "non_secret_scenario_marker_prompt"
  },
  "CorrelationId": "frdp-p01-row10-s2-path-a-<future-timestamp-or-guid>",
  "TargetLocator": null
}
```

## Future Response JSON Shape

Filename: `row10-s2-path-a-response.json`

Expected top-level fields from `XiigenExtensionResponse`:

- `Success`: boolean.
- `Status`: string; expected successful narrow status is `core_boundary_partial_no_semantic_quality_claim`.
- `ChatDisplayText`: string; must be safe/redacted and must not claim readiness.
- `SelectedSkills`: array of strings.
- `CheckSetResults`: object/dictionary with string values.
- `StructuredTaskRefs`: array of strings.
- `ScenarioRefs`: array of strings.
- `Warnings`: array of strings.
- `Errors`: array of strings.
- `DebugTrace`: object or null.

Expected `CheckSetResults` keys for PASS:

- `core_boundary_executed` equals `"true"`.
- `semantic_quality_claimed` equals `"false"`.
- `runtime_extension_claimed` equals `"false"`.
- `prompt_to_task_status` exists.
- `orchestration_reasoning_status` exists.
- `orchestration_reasoning_internal_status` exists.
- `skill_selection_evidence` exists only as boundary evidence, not as readiness/quality proof.
- `skill_selection_method_semantic_surface_required`, if present, must not be used to claim semantic quality.

Response FAIL conditions:

- `Success` is not true.
- `semantic_quality_claimed` is true or missing with quality-claim wording elsewhere.
- `runtime_extension_claimed` is true.
- Any field claims Cursor UI readiness, product readiness, token savings, semantic quality improvement, training/learnability, checkpoint promotion, locator update, lifecycle readiness, or local model backup/export/import/continue-training readiness.
- Any response relies on marker-only, fixture-only, source-only, trace-only, schema-only, precomputed, or hardcoded answer proof as a pass.
- Any field treats candidate-position/scaffold evidence as trained semantic quality or final readiness.

## Future Trace JSON Shape

Filename: `row10-s2-path-a-trace.json`

Expected fields from `tools\xiigen-extension-core-api.ps1`:

- `schema_version`: `1`.
- `endpoint`: `xiigen-extension-core-api`.
- `script_path`: `tools/xiigen-extension-core-api.ps1`.
- `call_target`: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- `request_path`: future request JSON path.
- `response_path`: future response JSON path.
- `started_utc`: timestamp.
- `completed_utc`: timestamp.
- `host_name`: `Cursor`.
- `host_type`: `cursor`.
- `method`: `prompt-to-task`.
- `correlation_id`: starts with `frdp-p01-row10-s2-path-a-`.
- `user_prompt_sha256`: `e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506`.
- `user_prompt_redacted`: true.
- `raw_private_prompt_persisted`: false.
- `contains_secrets`: false.
- `response_success`: true for PASS.
- `response_status`: expected `core_boundary_partial_no_semantic_quality_claim`.
- `semantic_quality_claimed`: false or `"false"`.
- `runtime_extension_claimed`: false or `"false"`.
- `host_runtime_claimed`: false.
- `legacy_contract_substitution_used`: false.
- `external_ai_used`: false.
- `training_started`: false.
- `core_asset_loader`: `XiigenExtensionApiService common asset root constructor`.
- `claim_scope`: core-owned endpoint invocation only; no host runtime claim.

Trace FAIL conditions:

- prompt hash mismatch.
- call target mismatch.
- `host_runtime_claimed`, `external_ai_used`, or `training_started` is true.
- `legacy_contract_substitution_used` is true.
- trace is absent and the future work order did not explicitly mark `TRACE_REF_NOT_SURFACED`.

## Future Output Filenames

The future executor should use a fresh future run folder and create or collect these filenames:

- Request JSON: `row10-s2-path-a-request.json`
- Response JSON: `row10-s2-path-a-response.json`
- Trace JSON: `row10-s2-path-a-trace.json`
- Boundary packet JSON: `row10-s2-path-a-adapter-core-boundary.json`
- Thin Cursor boundary evidence JSON: `row10-s2-path-a-host-boundary-cursor.json`
- Execution report Markdown: `row10-s2-path-a-execution-report.md`

If the future executor is not authorized to run or copy thin Cursor host-boundary evidence into the run folder, it must mark `BLOCKED_MISSING_CURSOR_HOST_THIN_BOUNDARY_EVIDENCE`, not PASS the extension-thin subcheck.

## Boundary Checks

Future Path A PASS requires all of these:

- Logic in `llm_mvp_core`: trace `call_target` equals `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` and response `CheckSetResults.core_boundary_executed` equals `"true"`.
- Extension thin: thin boundary evidence must show `thin_client=true`, `uses_xiigen_exe_api=true`, `core_logic_in_llm_mvp_core=true`, `host_training_logic=false`, `external_ai_calls=false`, `deterministic_shortcut=false`, and `hidden_business_logic=false`.
- No hidden deterministic shortcut: reject fixture-only, marker-only, source-only, trace-only, schema-only, precomputed, hardcoded answer, `legacy_contract_substitution_used=true`, or host-side business logic.
- No candidate-position-like masquerade: reject any claim that candidate-position/scaffold/static evidence proves semantic quality, training, token savings, or product readiness. If `candidate_position` or equivalent scaffold wording appears, record it as non-pass evidence and keep claims false.

## Claim Locks

- `product_readiness_claim=false`
- `cursor_ui_readiness_claim=false`
- `cursor_host_readiness_claim=false`
- `token_savings_claim=false`
- `semantic_quality_claim=false`
- `training_learnability_claim=false`
- `checkpoint_promotion_claim=false`
- `locator_update_claim=false`
- `lifecycle_readiness_claim=false`
- `runtime_execution_claim=false`
- `path_b_ui_execution_claim=false`

## Pass/Fail Assertions For Future Executor

PASS only if:

- request JSON parses and exactly matches the prompt hash, marker, scenario id, slice, host, and claim locks above;
- core API command exits successfully;
- response JSON parses and satisfies the response assertions above;
- trace JSON parses and satisfies the trace assertions above;
- adapter/core boundary packet records the command, request/response/trace filenames, prompt hash, and claim locks;
- thin Cursor boundary evidence exists or the packet clearly marks the thin-boundary subcheck BLOCKED;
- no forbidden claim opens;
- no hidden shortcut or candidate-position-like masquerade is accepted as proof.

NEEDS_FIX if:

- this work-order packet and companion JSON disagree on prompt hash, request filename, response filename, trace filename, or claim locks;
- future request/response/trace shape is ambiguous;
- any Path A execution is implied as already completed by this prep packet;
- any readiness, token savings, quality, training, lifecycle, checkpoint, locator, runtime, or Path B UI claim is opened.

BLOCKED if:

- `tools\xiigen-extension-core-api.ps1` or `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj` is missing;
- the future executor lacks authority to run the core API command;
- request/response/trace output paths cannot be created;
- the concrete boundary interface no longer matches the contract fields above.

## Forbidden In This Prep

This prep did not and must not:

- run product/API scenario execution;
- launch Cursor or execute Cursor UI;
- invoke runtime, tests, builds, evals, training, checkpoint, locator, or lifecycle;
- edit code, boards, statuses, dashboards, governance, or skills;
- stage, commit, push, reset, checkout, clean git state;
- use web, external AI, or nested subagents.

## Exact Next Review Task

Assign a bounded Row10-S2 Path A work-order reviewer for maximum 10 minutes.

Review scope:

- Read only this Markdown and companion JSON.
- Run JSON parse.
- Check Markdown/JSON agreement on prompt hash, request filename, response filename, trace filename, and claim locks.
- Run `git diff --check --no-index` whitespace checks against the two packet files.
- Do not execute Path A.

Only if reviewer verdict is PASS, assign a separate bounded Row10-S2 Path A executor to run the future Path A command/interface and create execution evidence in a new folder.

## Machine Fields

- schema_version: `row10-s2-path-a-work-order-prep/v1`
- artifact_type: `planning_work_order_prep_only`
- generated_at_local: `2026-06-27T04:10:13+03:00`
- subagent_id: `bounded-subagent-row10-s2-path-a-work-order-prep`
- verdict: `PASS_WORK_ORDER_PREP_READY_FOR_REVIEW`
- scenario_id: `frdp-p01-cursor-product-readiness-product-workflow`
- row: `Row10`
- slice: `Row10-S2`
- path: `A`
- prompt_sha256: `e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506`
- request_filename: `row10-s2-path-a-request.json`
- response_filename: `row10-s2-path-a-response.json`
- trace_filename: `row10-s2-path-a-trace.json`
- execution_authorized_now: `false`
- next_action: `review this work-order, then only if PASS execute Path A in a separate bounded sub-agent`
