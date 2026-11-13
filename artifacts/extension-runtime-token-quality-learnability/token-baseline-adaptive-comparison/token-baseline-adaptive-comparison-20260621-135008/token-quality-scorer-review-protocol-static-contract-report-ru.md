# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-QUALITY-SCORER-REVIEW-PROTOCOL-STATIC-CONTRACT-08

## Человеческий статус

Всего: 63 proof refs.
Сделано: 9/63 PASS refs уже были приняты раньше; этот packet не добавляет новый PASS.
Сейчас выполнено: подготовлен статический контракт будущего scorer/reviewer protocol для same-pair quality scoring по TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001.
Осталось: 54 proof refs; approvals 0/9; runner-ready 0/9; quality-pass 0/9.
Текущий результат: только static scorer/reviewer protocol support evidence.
Следующее действие: TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-RUNTIME-SCORING-INPUTS-APPROVAL-OR-DEFER-09.

## Что подготовлено

Этот packet фиксирует, какие поля будущего scorer/reviewer protocol должны существовать до запуска quality scoring для пары Claude prompt-to-task. Он не запускает scorer, reviewer, proof runner, proof producer, Claude, Cursor, Codex, runtime или модель. Он не читает raw-private content и не создает baseline/adaptive traces, scored outputs, token delta, quality comparison или quality non-regression result.

Пара зафиксирована так:

- pair: TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001;
- scenario: TOKEN-SCENARIO-PROMPT-TO-TASK-001;
- host: Claude;
- method: prompt-to-task;
- baseline side: baseline_common_no_adaptive;
- adaptive side: adaptive_user_local_target.

## Версионирование будущего протокола

Будущий scoring/reviewer packet должен содержать отдельные version/ref fields:

| Поле | Зачем нужно | Текущий статус |
|---|---|---|
| scorer_protocol_id | Имя семейства scoring protocol для same-pair качества. | Prepared as static contract only. |
| scorer_protocol_version | Версия правил подсчета и failure labels. | Deferred: scorer не запускался. |
| reviewer_protocol_version | Версия правил reviewer notes и adjudication. | Deferred: reviewer не запускался. |
| rubric_version_ref | Ссылка на rubric/input contract для PromptToTask, OrchestrationReasoning, SkillSelection и usefulness. | Available as static support evidence only. |
| regression_guard_version_ref | Версия same-pair regression guard. | Deferred: guard не запускался. |
| source_disjoint_gate_version_ref | Версия source-disjoint/no-memorization gate. | Deferred: held-out controls отсутствуют. |
| no_overclaim_gate_version_ref | Версия no-overclaim проверки. | Prepared as static gate only. |

## Schema будущего scorer input

Будущий scorer input должен принимать только ссылки и redacted evidence refs, а не raw-private content. Минимальный набор полей:

- pair_id, scenario_id, host_name, method;
- baseline_side и adaptive_side;
- pair_integrity_matrix_ref;
- quality_dependency_contract_ref;
- rubric_input_contract_ref;
- scorer_reviewer_protocol_contract_ref;
- baseline_output_ref и adaptive_output_ref;
- baseline_redacted_trace_ref и adaptive_redacted_trace_ref;
- eval_prompt_pack_ref;
- source_disjoint_heldout_split_ref;
- positive_negative_examples_ref;
- privacy_redaction_policy_ref;
- provenance_review_ref;
- tokenizer_name, tokenizer_version, accounting_version и counted_span_policy;
- no_raw_private_content_read flag;
- no_external_ai_model_call_by_scorer flag;
- dashboard_api_helper_static_not_live_runtime flag.

Текущий статус: runtime/scoring inputs отсутствуют, поэтому все rows, которым нужны live/runtime/scored refs, остаются deferred и не открывают quality, token, runtime, learning или readiness claim.

## Schema будущих reviewer notes

Reviewer notes должны быть eval/adjudication evidence only. Они не становятся DPO training targets без отдельного approved DPO authoring gate.

Минимальные поля reviewer note:

- reviewer_note_id;
- reviewer_protocol_version;
- reviewer_id_or_role;
- pair_id, scenario_id, host_name, method;
- layer_id;
- baseline_evidence_ref и adaptive_evidence_ref;
- score_baseline_0_5 и score_adaptive_0_5;
- score_delta;
- verdict: pass, fail, inconclusive или deferred;
- failure_labels;
- rationale_ref_without_raw_private_content;
- adjudication_decision_ref;
- regression_guard_ref;
- reviewer_notes_are_eval_evidence_only = true;
- reviewer_notes_used_as_training_target = false.

Текущий статус: reviewer notes не созданы, поэтому reviewer_notes_ref остается deferred.

## Regression guard schema

Same-pair regression guard должен подтвердить, что adaptive side не ухудшает качество по тому же pair/scenario/host/method перед любым token-savings success claim.

Минимальные поля guard:

- regression_guard_id и regression_guard_version;
- pair_integrity_matrix_ref;
- baseline_scores_ref и adaptive_scores_ref;
- per_layer_non_regression_status;
- overall_quality_non_regression_status;
- allowed_threshold_policy_ref;
- failure_labels;
- reviewer_notes_ref;
- ablation_no_leakage_no_memorization_ref;
- no_token_success_until_quality_non_regression_pass = true.

Текущий статус: regression guard не запускался, quality_non_regression_status отсутствует, token comparison remains closed.

## Per-layer adjudication fields

Будущий protocol должен отдельно adjudicate каждый слой:

| Layer | Обязательные поля | Текущий статус |
|---|---|---|
| PromptToTask | intent_capture_score, task_boundary_score, failure_labels, reviewer_note_ref | Deferred: scored outputs отсутствуют. |
| OrchestrationReasoning | plan_quality_score, evidence_use_score, failure_labels, reviewer_note_ref | Deferred: orchestration refs отсутствуют. |
| SkillSelection | relevance_score, no_missing_required_skill_score, failure_labels, reviewer_note_ref | Deferred: skill-selection refs отсутствуют. |
| FinalUsefulness | usefulness_score, task_completion_score, failure_labels, reviewer_note_ref | Deferred: paired judged outputs отсутствуют. |
| SafetyPrivacyProvenance | privacy_score, provenance_score, raw_private_not_read_flag | Deferred: provenance review отсутствует. |
| NoHiddenDeterminism | deterministic_shortcut_absent_score, ablation_status, no_memorization_status | Deferred: controls отсутствуют. |

## Source-disjoint и no-memorization gate refs

Quality scoring нельзя запускать как semantic-quality proof без этих будущих refs:

- eval_prompt_pack_ref with prompt hashes, not raw-private prompts;
- source_disjoint_heldout_split_ref;
- prompt_hash_overlap_check_ref;
- source_path_title_overlap_check_ref;
- correction_text_overlap_check_ref;
- vendor_trace_overlap_check_ref;
- nearest_source_overlap_check_ref;
- ablation_ref;
- no_leakage_ref;
- no_memorization_ref;
- deterministic_boundary_review_ref.

Текущий статус: refs отсутствуют или являются static support only. Semantic quality, common model readiness, target promotion и rollback claims остаются закрыты.

## No-overclaim checks

- Static JSON/report доказывает только форму будущей проверки. Он не доказывает runtime, quality, token savings, learnability или readiness.
- Dashboard/API/helper/static evidence нельзя подписывать как live Claude/Cursor/Codex runtime.
- Token delta не доказывает quality. Token comparison может быть успешным только после same-pair quality non-regression PASS.
- Quality eval в будущем докажет только rubric quality на evaluated set. Он не доказывает learning.
- Reviewer/evaluator/adjudicator notes остаются eval/adjudication evidence only и не являются DPO training target.
- Source-disjoint held-out, ablation, no-leakage and no-memorization controls должны пройти до semantic-quality, common model, target promotion или rollback claims.

## Что этот packet доказывает

- Статический scorer/reviewer protocol contract подготовлен.
- Future protocol version fields, scorer input schema, reviewer note schema, regression guard schema, per-layer adjudication fields, source-disjoint/no-memorization refs и no-overclaim checks названы.
- Counts сохранены: total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
- Dashboard/API/helper/static evidence остается support/static evidence и не labelled live host runtime.

## Что этот packet не доказывает

- Не доказана экономия токенов.
- Не доказано улучшение качества.
- Не доказан live host runtime Claude/Cursor/Codex.
- Не доказаны semantic quality, learnability, DPO, training, checkpoint/model/locator/target mutation, common model readiness, target promotion, rollback safety, product readiness или final readiness.
- Не создан baseline trace, adaptive trace, token accounting report, token delta report, scored quality report, reviewer notes или quality non-regression result.

## Next independent track

Следующий bounded work order: TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-RUNTIME-SCORING-INPUTS-APPROVAL-OR-DEFER-09.

Он должен подготовить только approval/defer packet для отсутствующих runtime/scoring inputs: baseline_common_no_adaptive trace, adaptive_user_local_target trace, scored output refs, reviewer notes, regression guard, source-disjoint held-out split, ablation/no-leakage/no-memorization refs и exact runtime/model approval boundary. Он не должен запускать runtime, scorer, proof runner, external AI/model call, training, DPO, checkpoint/model/locator/target mutation, dotnet test или git.
