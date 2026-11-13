# P16E2: ablation controls для SkillSelection

## Граница проверки
Контроли выполнены как artifact-level schema evaluation. Они проверяют, что P16D2 candidate не использует eval/holdout row id, source id, path/title, exact prompt или candidate order как основание.

## Счётчики
- Всего контролей: 72
- Shortcut leaks: 0

## Контроли
- `remove_prompt_signals`: 12 строк
- `remove_task_context_signals`: 12 строк
- `candidate_order_permutation`: 12 строк
- `remove_diagnostics_only_candidate_order_guard`: 12 строк
- `source_id_title_path_leakage_attempt`: 12 строк
- `exact_prompt_memorization_attempt`: 12 строк
