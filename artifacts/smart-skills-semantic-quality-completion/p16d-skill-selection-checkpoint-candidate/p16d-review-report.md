# P16D: кандидат checkpoint для SkillSelection

## Краткий вывод

P16D создал только кандидатный артефакт checkpoint для `SkillSelection`. После parent review repair 1 в checkpoint больше нет top-level `candidate_position_weights`, а строка `evidence_only` больше не входит в learned profile / trained state.

Использовано пять разрешенных `adaptive_user_cache` строк: четыре как кандидатные learned profiles и одна как diagnostics-only guard. Это не обучение, не финальная модель, не общая runtime-подключенная модель и не доказательство semantic quality.

Кандидат нужен для родительского review и следующего gate P16E: source-disjoint eval/ablation. До P16E/P16F/P16G этот checkpoint нельзя считать готовым для API, dashboard, host runtime или финального model locator.

## Что использовано

Использованы только разрешенные `adaptive_user_cache` строки:

| Строка | Unit target | Chosen | Route |
|---|---|---|---|
| `P16B-SS-CANDPOS-001` | `candidate_position_guard` | `code_review` | `adaptive_user_cache`, diagnostics-only |
| `P16B-SS-TASK-001` | `task_understanding` | `code_review` | `adaptive_user_cache` |
| `P16B-SS-CONTEXT-001` | `prompt_context` | `documentation_sync` | `adaptive_user_cache` |
| `P16B-SS-CHECKS-001` | `intermediate_checks` | `verification_before_completion` | `adaptive_user_cache` |
| `P16B-SS-CHAIN-001` | `runtime_chain` | `code_review` | `adaptive_user_cache` |

Четыре строки перенесены в checkpoint как кандидатные профили typed-state формы. Строка `P16B-SS-CANDPOS-001` не стала skill-selection authority: она перенесена только в `candidate_order_leakage_guard` с `can_select_skill=false`, `authority=false` и `authority_scope="diagnostics_only"`.

Поля `row_id`, `source_ref`, `source_hash_or_pointer` и `source_family` используются как provenance, а не как runtime-authority. Сырые приватные prompt-тексты не копировались.

## Что исключено

Из checkpoint learned profiles исключена строка:

- `P16B-SS-PROMPT-001` — route `evidence_only`, source/license status не разрешает материализовать ее как checkpoint training material.

Из checkpoint также исключены все `eval_candidate` и `holdout_candidate` строки P16B:

- `P16B-SS-CANDPOS-002`, `P16B-SS-PROMPT-002`, `P16B-SS-TASK-002`, `P16B-SS-CONTEXT-002`, `P16B-SS-CHECKS-002`, `P16B-SS-CHAIN-002`
- `P16B-SS-CANDPOS-003`, `P16B-SS-PROMPT-003`, `P16B-SS-TASK-003`, `P16B-SS-CONTEXT-003`, `P16B-SS-CHECKS-003`, `P16B-SS-CHAIN-003`

Исключение сделано, чтобы сохранить source-disjoint будущий eval/holdout. Эти строки остаются материалом для P16E, а не входом checkpoint.

## Как материализован кандидат

Материализация сделана как artifact-level checkpoint schema, а не как training run. P16D не запускал обучение и не менял production code/test code. Для разрешенных profile-строк взяты редактированные summary/boundary признаки, очищенные от project-stack рецептов и shortcut-сигналов.

Candidate-position данные не представлены как numeric trained selection state. Вместо этого добавлен typed diagnostics-only блок `candidate_order_leakage_guard`:

- `can_select_skill=false`;
- `authority=false`;
- `authority_scope="diagnostics_only"`;
- `blocked_feature_ids` включает `candidate_list_order_skill_authority`;
- numeric final selection weights отсутствуют.

Созданные файлы:

- `skill-selection-p16d-candidate-checkpoint.json`
- `skill-selection-p16d-candidate-manifest.json`
- `skill-selection-p16d-source-manifest.json`
- `skill-selection-p16d-locator.json`
- `skill-selection-p16d-candidate-input-rows.jsonl`
- `skill-selection-p16d-excluded-evidence-only-rows.jsonl`

Checkpoint sha256:

`9238A1577A783E50AB910346B9C00F515005132EA5D09B7D3FFB5DF7BCB4C20F`

## Parent review repair 1

До repair:

- top-level `candidate_position_weights` выглядел как loadable trained selection state;
- `P16B-SS-PROMPT-001` имел route `evidence_only`, но был материализован в `prompt_understanding` learned profile.

Repair:

- top-level `candidate_position_weights` удален;
- добавлен `candidate_order_leakage_guard` только для диагностики утечки порядка кандидатов;
- `P16B-SS-PROMPT-001` перенесен в excluded/evidence-only ledger;
- materialized profile count теперь равен 4, allowed train-candidate rows used равен 5.

Оставшийся gap:

- prompt-understanding learned profile в этом candidate checkpoint пустой, потому что единственная train-candidate строка для него была `evidence_only`. Это должно быть проверено или исправлено в будущих train-only/source-review фазах, а не закрыто shortcut-материализацией.

## Почему это только кандидат

P16B явно не сделал строки training-ready: лицензии/common-source boundary не закрыты, source-disjoint eval/ablation еще не проведены, fresh-load/continue-training еще не доказаны. Поэтому P16D фиксирует кандидатный checkpoint только для review и следующей проверки.

Не заявляется:

- semantic quality;
- full API readiness;
- SkillSelection readiness;
- trained-model readiness;
- dashboard readiness;
- host runtime readiness;
- final model readiness.

## Следующие gates

1. P16E: source-disjoint eval/ablation для SkillSelection checkpoint candidate.
2. P16F: fresh-load и continue-training lifecycle evidence, если P16E пройдет.
3. P16G: API firewall/dashboard-boundary review только после положительного SkillSelection evidence.

## Проверочная рамка P16D

P16D проверяет только материализацию candidate asset:

- JSON/JSONL парсятся;
- checkpoint sha256 совпадает с manifest;
- checkpoint input содержит только `train_candidate`;
- eval/holdout row ids явно исключены;
- UTF-8 читаемость без mojibake;
- нет true-флагов semantic/full/final readiness;
- изменения ограничены P16D artifacts и active state.
