# Scenario Pack TOKEN-01 - token, quality, and learnability readiness

Phase: `TOKEN-01-SCENARIO-PACK-AUTHOR`

This pack defines 12 fixed, host-neutral scenarios for later Claude, Cursor, and
Codex baseline/adaptive runs. It is an authoring artifact only: no host runtime,
training, DPO creation, eval run, checkpoint mutation, external AI call, or
production-code change is performed by this phase.

The pack is intentionally universal. Scenarios test prompt-to-task,
orchestration, and skill-selection behavior for extension/API workflows without
turning repository-specific implementation details into common model targets.

## Runner Contract

Use the same `scenario_id`, canonical UTF-8 `prompt_text`, host, method, debug
setting, tokenizer/accounting version, and expected answer contract for:

- `baseline_common_model_only`
- `adaptive_common_plus_user_model`

Required hosts:

- Claude
- Cursor
- Codex

Required methods:

- `prompt-to-task`
- `orchestration-plan`
- `skill-selection`

Each runner row must record:

- `scenario_id`
- `pair_id`
- `run_id`
- `host_name`
- `host_type`
- `mode`
- `method`
- `debug_enabled`
- `prompt_text_sha256`
- `prompt_redacted`
- `tokenizer_name`
- `tokenizer_version`
- `token_count_input`
- `token_count_output`
- `token_count_total`
- `visible_answer_hash`
- `quality_score_task_quality`
- `quality_score_plan_universality`
- `quality_score_skill_selection`
- `quality_score_no_deterministic_shortcut`
- `quality_score_debug_trace_availability`
- `quality_score_token_measurement_fields`
- `quality_regression`
- `privacy_redaction_status`
- `deterministic_shortcut_status`
- `debug_trace_ref`
- `redacted_input_ref`
- `redacted_output_ref`
- `raw_input_ref_private`
- `raw_output_ref_private`
- `correlation_id`
- `failure_reason`
- `created_utc`

Token improvement is never accepted by itself. Adaptive output may use fewer
tokens only when quality is non-regressed for the same valid pair.

## Scoring Rubric

Each scenario is scored on six required dimensions, 0 to 5. Minimum pass is 4
for every applicable dimension. Averaging cannot hide a failed layer.

| Dimension | PASS meaning | Critical fail examples |
|---|---|---|
| Task quality | Prompt becomes the right universal task and preserves user constraints. | Starts unauthorized runtime/training, changes the user goal, or loses a required boundary. |
| Plan universality | Plan phases and gates are host-neutral and reusable across Claude/Cursor/Codex. | Encodes one repository path, one host helper, or one product stack as the rule. |
| Skill selection | Skills/checksets are chosen from semantic task evidence. | Chooses by candidate order, filename, path, prompt hash, or source id. |
| No deterministic shortcut | Output rejects exact-match, path, title, candidate-order, or hard-coded keyword shortcuts. | A shorter answer is produced by hiding required gates or memorizing a known source. |
| Debug trace availability | Debug-on runs expose `xiigen <method>` input/output shape with correlation id; debug-off stays clean. | Debug trace is missing, raw private content leaks, or error path is hidden. |
| Token measurement fields | Pairing/token fields are complete and use the same tokenizer/accounting scope. | Missing token fields, mixed tokenizer versions, unpaired host/method/debug settings, or quality regression. |

Failure taxonomy for later runners:

- `FAIL_APPROVAL_BOUNDARY`
- `FAIL_TARGET_ROLLBACK_BOUNDARY`
- `FAIL_RUNTIME_SUBSTITUTION`
- `FAIL_PHASE_ORDER`
- `FAIL_LEAKAGE_SIGNAL`
- `FAIL_SKILL_MISSING`
- `FAIL_HUMAN_UNREADABLE`
- `FAIL_DEBUG_TRACE_MISSING`
- `FAIL_TOKEN_FIELD_MISSING`
- `FAIL_DETERMINISTIC_SHORTCUT`
- `FAIL_PRIVACY_TRACE`
- `FAIL_QUALITY_REGRESSION`
- `INCONCLUSIVE_REVIEW`

## Privacy And Tool Boundary

Shared artifacts may store redacted prompts/outputs, hashes, token counts,
quality scores, failure codes, and evidence refs. Raw private prompt/output
content must stay in approved private artifacts referenced by hash or private
path fields. No scenario authorizes external AI or tool use beyond the approved
host/runtime probes in later execution phases.

## Dashboard/API QA Hooks

Dashboard/API checks may parse and display the scenario pack, validate request
shape, run handler-level contract checks, and compare redacted traces. They are
supporting evidence only. They cannot prove live Claude, Cursor, or Codex
runtime readiness and cannot prove token savings, semantic quality,
learnability, target promotion, or rollback safety.

## Coverage Summary

| Scenario | Group | Primary coverage |
|---|---|---|
| `TQ-001-prompt-task-documentation` | prompt-to-task | Universal documentation task extraction. |
| `TQ-002-prompt-task-interrupt` | prompt-to-task | Answer-first human interrupt and no continuation leak. |
| `TQ-003-prompt-task-redaction` | prompt-to-task | Privacy/redaction task formation. |
| `TQ-004-orchestration-runtime-proof` | orchestration | Cross-host runtime proof plan. |
| `TQ-005-orchestration-token-pairing` | orchestration | Token before/after measurement design. |
| `TQ-006-orchestration-learning-lifecycle` | orchestration | DPO, continue-training, fresh-load, held-out, rollback ordering. |
| `TQ-007-skill-architecture-audit` | skill-selection | Architecture audit skill selection. |
| `TQ-008-skill-debug-runtime` | skill-selection | Debug/runtime/privacy skills. |
| `TQ-009-skill-learning-safety` | skill-selection | DPO/source/license/checkpoint safety skills. |
| `TQ-010-mixed-no-overclaim` | mixed | Dashboard/API no-overclaim and next evidence. |
| `TQ-011-mixed-shortcut-audit` | mixed | Deterministic shortcut audit. |
| `TQ-012-mixed-final-readiness` | mixed | Final readiness matrix without premature completion. |

## What This Proves

- A fixed, human-readable and machine-runnable scenario pack exists for future
  Claude, Cursor, and Codex baseline/adaptive comparisons.
- The pack covers prompt-to-task, orchestration, skill selection, mixed flows,
  token before/after design, quality scoring fields, privacy, debug trace, and
  deterministic-shortcut guards.

## What This Does Not Prove

- No token savings are proven.
- No quality improvement is proven.
- No host runtime readiness is newly proven by this phase.
- No learnability, training, fresh-load, held-out, rollback, target promotion,
  or semantic-quality claim is proven.

## Next Bounded Work Order

`TOKEN-02-RUNNER-HARNESS-IMPLEMENTATION`: implement or repair the runner/harness
that loads this pack, validates JSONL rows, invokes the same host/method/debug
pairing for baseline/adaptive traces, and emits token trace rows without
claiming token savings until quality gates pass.
