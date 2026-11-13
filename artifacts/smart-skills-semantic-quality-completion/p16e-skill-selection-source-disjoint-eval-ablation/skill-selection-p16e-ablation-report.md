# P16E: ablation report

## Результат

Проверены 72 ablation rows по reserved eval/holdout material.

Shortcut leaks: 0.

## Controls

1. `remove_prompt_signals`: проверяет, не держится ли выбор только на surface prompt summary.
2. `remove_task_context_signals`: проверяет, не держится ли выбор только на task/context boundary.
3. `candidate_order_permutation`: проверяет, не выбирается ли skill из-за порядка списка кандидатов.
4. `remove_diagnostics_only_candidate_order_guard`: проверяет, что diagnostics-only guard не является selection authority.
5. `source_id_title_path_leakage_attempt`: проверяет, не помогает ли provenance/source/path-like материал.
6. `exact_prompt_memorization_attempt`: проверяет, не проходит ли строка за счет exact prompt memory.

## Что это доказывает

На уровне P16D schema evaluator не найден shortcut leak: candidate order, source/title/path/id и exact prompt не дают скрытого positive pass.

## Что это не доказывает

Это не semantic-quality readiness. Positive source-disjoint eval не прошел: 0 pass, 8 fail, 4 gap. Поэтому ablation pass без positive eval pass остается только no-shortcut evidence для текущего слабого кандидата.
