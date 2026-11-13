# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-PAIR-INTEGRITY-STATIC-MATRIX-04

## Человеческий статус

Всего: 63 proof refs.
Сделано: 9/63 PASS refs уже были приняты раньше; этот packet не добавляет новый PASS.
Сейчас выполнено: подготовлена статическая матрица pair integrity и placeholder validation contract для будущих baseline/adaptive captures по `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`.
Осталось: 54 proof refs; approvals 0/9; runner-ready 0/9; quality-pass 0/9.
Текущий результат: static pair-integrity/support evidence only.
Следующее действие: `TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-RUNTIME-CAPTURE-APPROVAL-OR-STATIC-SUPPORT-05`.

## Что подготовлено

Этот packet не запускает Claude, Cursor, Codex, внешний AI, модель, runner, scorer, training, DPO или dotnet test. Он только фиксирует будущую проверку парности двух строк:

- baseline side: `baseline_common_no_adaptive`;
- adaptive side: `adaptive_user_local_target`;
- pair: `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`;
- scenario: `TOKEN-SCENARIO-PROMPT-TO-TASK-001`;
- host: Claude;
- method: prompt-to-task.

Матрица использует статическую trace schema из packet `token-baseline-adaptive-comparison-20260621-124528`. Она заранее говорит, какие поля должны совпасть, какие могут отличаться, и какие missing labels удерживают token claim закрытым до настоящей paired capture и same-pair quality non-regression.

## Поля, которые обязаны совпасть

Эти поля должны быть одинаковыми у baseline и adaptive rows. Если хотя бы одно поле отсутствует или различается, будущая token comparison остается закрытой:

| Field | Почему должно совпасть | Missing label | Mismatch label |
|---|---|---|---|
| `pair_id` | Обе строки должны относиться к одной проверяемой паре. | `INVALID_PAIR_OR_SCENARIO` | `PAIR_ID_MISMATCH` |
| `scenario_id` | Сравнение допустимо только на одном сценарии. | `INVALID_PAIR_OR_SCENARIO` | `SCENARIO_ID_MISMATCH` |
| `host_name` | Нельзя сравнивать разные hosts как одну token pair. | `MISSING_HOST_PAIR` | `HOST_NAME_MISMATCH` |
| `host_runtime_surface` | Runtime surface должен быть одинаковым, иначе меняется evidence scope. | `MISSING_HOST_PAIR` | `HOST_RUNTIME_SURFACE_MISMATCH` |
| `method` | Метод должен остаться `prompt-to-task` на обеих сторонах. | `MISSING_HOST_PAIR` | `METHOD_MISMATCH` |
| `evidence_classification` | Static/helper/dashboard evidence нельзя смешивать с live host runtime. | `INVALID_EVIDENCE_CLASSIFICATION` | `EVIDENCE_CLASSIFICATION_MISMATCH` |
| `live_scope` | Live/narrow/static scope должен совпадать, иначе comparison нечестная. | `INVALID_EVIDENCE_CLASSIFICATION` | `LIVE_SCOPE_MISMATCH` |
| `prompt_hash` | Prompt должен быть тем же самым без чтения raw-private content. | `INVALID_PROMPT_PAIR` | `PROMPT_HASH_MISMATCH` |
| `prompt_redacted_ref` | Redacted prompt reference должен указывать на тот же безопасный input. | `INVALID_PROMPT_PAIR` | `PROMPT_REDACTED_REF_MISMATCH` |
| `tokenizer_name` | Token counts должны считаться одним tokenizer. | `INVALID_TOKENIZER_ACCOUNTING_PAIR` | `TOKENIZER_NAME_MISMATCH` |
| `tokenizer_version` | Версия tokenizer должна совпадать. | `INVALID_TOKENIZER_ACCOUNTING_PAIR` | `TOKENIZER_VERSION_MISMATCH` |
| `accounting_version` | Accounting method должен быть тем же. | `INVALID_TOKENIZER_ACCOUNTING_PAIR` | `ACCOUNTING_VERSION_MISMATCH` |
| `counted_span_policy` | Нельзя сравнивать разные counted spans. | `INVALID_TOKENIZER_ACCOUNTING_PAIR` | `COUNTED_SPAN_POLICY_MISMATCH` |
| `token_count_estimate_mode` | Exact/estimate policy должен совпадать. | `INVALID_TOKENIZER_ACCOUNTING_PAIR` | `TOKEN_COUNT_ESTIMATE_MODE_MISMATCH` |
| `privacy_redaction_policy_ref` | Redaction policy должна быть той же на обеих сторонах. | `MISSING_RAW_OR_REDACTED_TRACE_REF` | `PRIVACY_REDACTION_POLICY_MISMATCH` |
| `debug_setting_ref` | Debug setting влияет на prompt/output shape и должен совпасть. | `MISSING_DEBUG_SETTING_REF` | `DEBUG_SETTING_MISMATCH` |

## Поля, которые могут отличаться

Эти поля могут отличаться, потому что они определяют baseline или adaptive side, либо являются результатами будущей capture. Они не открывают token claim сами по себе:

| Field | Ожидаемое различие или правило |
|---|---|
| `capture_side` | Baseline row должна быть `baseline_common_no_adaptive`; adaptive row должна быть `adaptive_user_local_target`. |
| `model_mode` | Baseline row должна быть `common_baseline`; adaptive row должна быть `adaptive_user_local_target`. |
| `adaptive_enabled` | Baseline: `false`; adaptive: `true`. |
| `user_local_checkpoint_id` | Baseline: `null`; adaptive: exact user-local checkpoint или target checkpoint later. |
| `target_checkpoint_id` | Baseline: `null`; adaptive может назвать target checkpoint later. |
| `target_locator_mode` | Baseline может быть `none`; adaptive может назвать locator mode only after allowed target binding. |
| `target_locator_ref` | Baseline: `null`; adaptive может указать locator ref only after allowed target binding. |
| `token_count_input` | Будущий measured value; сравнение возможно только после пары и quality gate. |
| `token_count_output` | Будущий measured value; сам по себе не доказывает quality. |
| `token_count_system` | Будущий measured value; должен соответствовать same counted-span policy. |
| `token_count_tool` | Будущий measured value; должен соответствовать same counted-span policy. |
| `token_count_total` | Будущий measured value; token delta без quality non-regression не является success. |
| `quality_comparison_ref` | Должен появиться после same-pair quality comparison. |
| `quality_non_regression_status` | Должен быть PASS later, иначе token claim остается закрытым. |
| `raw_private_trace_ref` | Ref может отличаться по run, но content не читается этим packet. |
| `redacted_trace_ref` | Ref может отличаться по run, но prompt hash и pair scope должны совпасть. |
| `trace_id` | Trace id уникален для каждой capture side. |
| `correlation_id` | Correlation id уникален для каждого future run. |
| `freshness_nonce_hash` | Nonce уникален для каждого future run. |
| `run_started_at_local` | Время запуска future capture может отличаться. |
| `run_completed_at_local` | Время завершения future capture может отличаться. |
| `visible_answer_ref` | Output ref будет разным между baseline и adaptive. |
| `negative_or_error_path_ref` | Ref может отличаться, но должен существовать при future runtime proof. |

## Labels, которые держат token claim закрытым

- `MISSING_FRESH_RUNTIME_AUTHORITY`: нет свежего разрешения Luba на Claude/runtime/model capture.
- `INVALID_PAIR_OR_SCENARIO`: pair или scenario отсутствует либо не совпадает.
- `MISSING_HOST_PAIR`: host, runtime surface или method не образуют одну пару.
- `INVALID_PROMPT_PAIR`: prompt hash или redacted prompt ref отсутствует либо не совпадает.
- `INVALID_TOKENIZER_ACCOUNTING_PAIR`: tokenizer/accounting/counted-span/estimate policy отсутствует либо не совпадает.
- `MISSING_BASELINE_COMMON_NO_ADAPTIVE_REF`: baseline side не доказывает common baseline без adaptive state.
- `MISSING_ADAPTIVE_USER_LOCAL_TARGET_REF`: adaptive side не привязана к exact user-local checkpoint или target checkpoint.
- `MISSING_RAW_OR_REDACTED_TRACE_REF`: отсутствуют raw-private ref или redacted trace ref как references only.
- `MISSING_QUALITY_NON_REGRESSION`: same-pair quality non-regression не прошла.
- `INVALID_EVIDENCE_CLASSIFICATION`: static/helper/dashboard/API evidence помечено как live host runtime или scope смешан.
- `OVERCLAIM_STATIC_MATRIX`: report пытается открыть readiness, quality, learnability или token claim из статической матрицы.
- `COUNT_DRIFT`: board/counts ушли от total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9, quality-pass 0/9.

## Что этот packet доказывает

- Статическая матрица pair integrity для будущих baseline/adaptive rows подготовлена.
- Placeholder validation contract создан для будущей проверки authority, pair scope, host/method, prompt hash, tokenizer/accounting, side constraints, refs, quality dependency, evidence classification, no-overclaim и counts.
- Counts сохранены: total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
- Dashboard/API/helper/static evidence остается support/static evidence and is not labelled live host runtime.

## Что этот packet не доказывает

- Не доказана экономия токенов.
- Не доказано улучшение качества.
- Не доказан live host runtime Claude/Cursor/Codex.
- Не доказаны semantic quality, learnability, DPO, training, checkpoint/model/locator/target mutation, common model readiness, target promotion, rollback safety, product readiness или final readiness.
- Не создан baseline trace, adaptive trace, token accounting report, token delta report или quality non-regression result.

## No-overclaim wording

Static JSON wording from Section 11.1 applies here: "This is static evidence. It must be paired with runtime/eval evidence before readiness can be claimed."

Token wording from Section 11.1 remains future-only: "Token change is a cost/trace metric. It is success only when quality is non-regressed by the quality gate for the same pair; non-live traces cannot prove live host savings."

Dashboard/API wording remains closed for this packet: "This proves dashboard/API behavior only. It does not prove live Claude/Cursor/Codex extension runtime."

## Next independent track

Следующий bounded work order: `TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-RUNTIME-CAPTURE-APPROVAL-OR-STATIC-SUPPORT-05`.

Он должен ждать fresh runtime approval before actual capture. Если fresh approval отсутствует, он должен выбрать другой безопасный independent static/support track without runtime, external AI/model calls, raw-private reads, proof runner, proof producer, quality scorer, training, DPO, checkpoint/model/locator/target mutation, dotnet test или git.
