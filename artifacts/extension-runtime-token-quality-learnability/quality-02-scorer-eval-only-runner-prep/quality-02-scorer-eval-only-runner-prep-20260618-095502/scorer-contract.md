# QUALITY-02 scorer contract

Run: `quality-02-scorer-eval-only-runner-prep-20260618-095502`

Этот контракт только подготавливает будущий eval-only scorer. В этом срезе не запускались model/host/eval/training, не менялись checkpoint/locator/source code и не создавались scores.

## Что future scorer должен принять

Будущая строка scoring input обязана иметь:

- `scenario_id`, `pair_id`, `route`, `mode`, `host_name`, `method`;
- `approval_ref`, `user_local_root_ref`, `raw_private_input_ref`, `redacted_shared_evidence_ref`;
- `source_ref`, `doc_id`, `prompt_sha256`;
- `baseline_output_ref`, `adaptive_output_ref`;
- `privacy_redaction_status`;
- `no_common_mutation_attestation=true`;
- `no_training_mutation_attestation=true`;
- `no_model_checkpoint_locator_mutation_attestation=true`.

До отдельного approval это не runnable input. Сейчас разрешена только статическая подготовка контракта и dry-run validation.

## Route policy

| Route | Scenarios | Pair rows | Статус |
|---|---:|---:|---|
| `USER_ADAPTIVE_EVAL_ONLY` | 7 | 21 | Runnable только позже, после точного approval и privacy/source gates. |
| `EVIDENCE_ONLY` | 5 | 33 | Non-runnable. Можно использовать только как control/no-overclaim context. |
| `COMMON_BLOCKED` | 12 | 54 | Strict/common route закрыт для всех строк. |

## Score layers

| Layer | Machine target | PASS rule | Required notes |
|---|---|---|---|
| PromptToTask | `PromptToTaskMachine` | score >= 4 and no critical failure | human reviewer note required |
| OrchestrationReasoning | `OrchestrationReasoningMachine` | score >= 4 and phase order preserved | human reviewer note required |
| SkillSelection | `SkillSelectionMachine` | score >= 4 and no leakage signal | human reviewer note required |
| Final usefulness | `FinalOutputUsefulness` | score >= 4 and no overclaim | human reviewer note required |

Score anchors are 0..4, aligned to QUALITY-01 rubric anchors: catastrophic, wrong, incomplete, usable_with_repair, pass.

## Required output shape for a future scorer

Every future scored row must include:

- scenario and pair identity;
- route and runnable status;
- baseline/adaptive output refs;
- per-layer score object with `score_0_to_4`, `pass_fail`, `failure_codes`, `reviewer_note`, and `evidence_refs`;
- `human_reviewer_note`;
- `inter_rater_status`;
- `quality_regression`;
- `semantic_quality_claim_allowed=false` unless separate runtime/eval/training gates are satisfied;
- all claim locks false unless later plan gates explicitly open them.

Adaptive must be >= baseline for every layer and must add no new failure code. Averaging cannot hide a failed layer, and token decrease cannot compensate quality regression.

## Failure labels

The contract carries the QUALITY-01 taxonomy forward:

- `FAIL_APPROVAL_BOUNDARY`
- `FAIL_TARGET_ROLLBACK_BOUNDARY`
- `FAIL_RUNTIME_SUBSTITUTION`
- `FAIL_PHASE_ORDER`
- `FAIL_LEAKAGE_SIGNAL`
- `FAIL_SKILL_MISSING`
- `FAIL_HUMAN_UNREADABLE`
- `FAIL_OVERCLAIM`
- `FAIL_QUALITY_REGRESSION`
- `INCONCLUSIVE_REVIEW`
- `FAIL_SOURCE_DISJOINT_SPLIT`
- `FAIL_HELD_OUT_MISSING`
- `FAIL_NO_MEMORIZATION_CONTROL`
- `FAIL_ABLATION_EXPLAINS_GAIN`
- `FAIL_DETERMINISTIC_BASELINE_ONLY`
- `FAIL_NARROW_STAGED_ONLY`
- `SEMANTIC_CLAIM_BLOCKED_NARROW_LAYER_EVIDENCE`
- `INCONCLUSIVE_QUALITY_PAIRING`
- `NON_RUNNABLE_EVIDENCE_ONLY`
- `COMMON_BLOCKED_NO_PERMISSIVE_SOURCE`

## What this proves

- The future scorer has an explicit input/output contract, route policy, layer model, failure taxonomy and reviewer-note requirement.

## What this does not prove

- No quality improvement, semantic readiness, token savings, learnability, live-host readiness, common readiness, training readiness, target promotion, locator update or final readiness is proven.

## Next independent track

- Parent review/commit for this artifact packet; after that, a bounded runner implementation or artifact dry-run selection work order may be issued only with explicit current authority.
