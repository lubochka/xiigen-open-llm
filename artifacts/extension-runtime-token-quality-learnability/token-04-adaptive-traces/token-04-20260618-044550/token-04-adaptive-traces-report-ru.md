# TOKEN-04: adaptive/user-local target traces

## Что означает adaptive/user-local target в этом run

В этом TOKEN-04 run adaptive/user-local target означает только handler-level вызов `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` с `XiigenTargetLocator`, указывающим на уже существующий QSG28 staged/test-only target root. Это не production locator, не финальная user-local readiness, не live UI/chat host capture, не доказательство semantic quality и не основание для token savings claim.

Использованный target locator: `artifacts/extension-runtime-token-quality-learnability/qsg28-semantic-host-matrix/qsg28-two-unit-test-locator.json`.

- locator_candidate_id: `qsg28-two-unit-test-locator`
- target_candidate_id: `qsg28-qsg27-prompt-to-task-qsg26-skill-selection-target`
- activation_scope: `test_only_qsg28_combined_qsg27_prompt_to_task_qsg26_skill_selection_root`
- PromptToTask target: `staged-two-unit-target-root/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json`
- SkillSelection target: `staged-two-unit-target-root/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`
- OrchestrationReasoning user/staged target: отсутствует в QSG28 root; сервис использовал repo/common fallback.

## Что сделано

Собраны 54 scoped handler-level adaptive rows для тех же TOKEN-01/TOKEN-03 `pair_id` values. Каждая adaptive row указывает на matching TOKEN-03 baseline row через `matching_token_03_baseline_row_ref` и сохраняет тот же `pair_id`, `scenario_id`, `host`, `method`, prompt hash, tokenizer/accounting version и redaction policy там, где TOKEN-03 предоставляет эти поля.

QSG28 staged/test-only target locator был доступен и использован только как тестовый staged target. Training не запускался, external AI не вызывался, checkpoint/locator/production model/extension repo не мутировали.

PromptToTask и SkillSelection user/staged assets загрузились для всех 54 adaptive rows. OrchestrationReasoning user/staged asset отсутствовал в QSG28 target root, поэтому все 54 rows несут label `ORCHESTRATION_USER_ADAPTIVE_ASSET_MISSING_COMMON_FALLBACK_USED`. Это scope limitation и блокер для learnability, semantic quality, production promotion и final readiness claims.

## Текущее состояние

- TOKEN-01/TOKEN-03 ожидаемые `pair_id` values: 54
- adaptive trace rows captured: 54
- missing adaptive trace rows: 0
- deferred claim rows: 54
- hosts: Claude 18, Cursor 18, Codex 18
- methods: prompt-to-task 18, orchestration-plan 18, skill-selection 18
- claim flags: `claim_allowed=false`, `token_savings_claimed=false`, `quality_improvement_claimed=false`, `semantic_quality_claimed=false`, `learnability_claimed=false`, `final_readiness_claimed=false`

Этот packet готов только как scoped handler-level input для QUALITY-03 same-pair non-regression review. Он не доказывает live-host token savings для Claude/Cursor/Codex, потому что TOKEN-03 и TOKEN-04 не являются same-run live UI/chat host token pair.

## Что осталось

QUALITY-03 same-pair non-regression еще не запускался и не проходил для этих 54 pairs. Token counts без QUALITY-03 не могут открыть savings claim. QSG28 target locator остается staged/test-only, поэтому production locator/checkpoint readiness, rollback closure, semantic quality, learnability и final readiness остаются закрытыми.

Если позже нужен live-host savings claim, потребуется отдельная same-scope live UI/chat host baseline/adaptive pair capture. Этот TOKEN-04 packet такого claim не открывает.

## Следующий шаг

После parent review/commit/push acceptance для TOKEN-04 следующий bounded phase: QUALITY-03 same-pair non-regression для 54 handler-level TOKEN-03 baseline rows и TOKEN-04 adaptive rows. QUALITY-01/QUALITY-02 следует refresh/run first только если parent решит, что текущие rubric/eval artifacts не применимы к этому TOKEN-04 packet.

## Основные артефакты

- Adaptive traces JSONL: `adaptive-token-traces.jsonl`
- Adaptive traces JSON: `adaptive-token-traces.json`
- Accounting report: `token-04-accounting-report.json`
- Pairing report: `token-04-pairing-report.json` и `token-04-pairing-report-ru.md`
- Deferred rows: `token-04-missing-deferred-rows.jsonl`
- Raw/private traces: `raw-private/`
- Redacted traces: `redacted-shared/`
