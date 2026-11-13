# P16E1: train-only repair material для SkillSelection

## Краткий вывод

P16E показал слабое место SkillSelection: reserved eval/holdout строки не прошли положительную проверку, хотя shortcut leak не был найден. P16E1 не запускает обучение и не создает checkpoint. Эта фаза только материализует безопасные train-only candidate rows для родительского review.

Вердикт: `PASS_CANDIDATE_WITH_GAPS_READY_FOR_PARENT_REVIEW`. Новых candidate rows: 10; из них 9 дают положительный skill contrast, а 1 строка оставлена diagnostics-only для candidate-order guard. Все строки остаются `candidate_training_ready=false`.

## Что доказал P16E

- Положительная source-disjoint оценка: 8 fail и 4 gap из 12.
- Ablation controls: shortcut leak не найден.
- Главный пробел: в P16D слишком мало train-side профилей для prompt understanding, task understanding, prompt context, intermediate checks и runtime chain.
- Candidate-position guard нельзя чинить как positive selector: он должен оставаться diagnostics-only и блокировать shortcut.

## Что добавлено в P16E1

| Unit | Repair rows | Смысл |
|---|---:|---|
| prompt_understanding | 2 | Распознавать claim verification и planning по смыслу запроса, без exact prompt. |
| task_understanding | 2 | Отличать DPO authoring и trainable architecture decision от преждевременной реализации. |
| prompt_context | 1 | Использовать no-claim/context boundary для выбора claim verification. |
| intermediate_checks | 2 | Различать readability gate и test integrity. |
| runtime_chain | 2 | Останавливать цепочку на claim verification или trainable architecture gate. |
| candidate_position_guard | 1 | Только diagnostics-only: порядок кандидатов не выбирает skill. |

## Границы безопасности

Eval/holdout строки P16B не используются как training material. Они упоминаются только в gap map и quarantine ledger, чтобы показать, что именно провалилось. Новые rows используют redacted summaries и safe source pointers; raw private prompt text не копировался.

Common training не заявлен. Локальные skills и parent artifacts могут быть adaptive/project evidence, но common learning требует отдельной проверки источника и лицензии.

## Что остается заблокированным

1. Parent должен решить, какие P16E1 rows принять как train-side material.
2. Для строк с пересекающимися source families нужен source-disjoint review перед checkpoint rebuild.
3. Если parent принимает достаточно строк, следующий безопасный gate - P16D2 checkpoint candidate rebuild.
4. Если source/license gates не проходят, нужна еще одна train-only materialization pass.

## Проверки для parent review

- JSON/JSONL parse всех P16E1 artifacts.
- Contamination check: P16B eval/holdout ids отсутствуют в train-only candidate rows.
- Scan на raw private prompt и exact-source leakage.
- UTF-8/mojibake scan.
- No-overclaim true-flag scan.
- `git diff --check` для P16E1 artifacts и state.
