# P16E: source-disjoint eval report

## Результат

Оценены 12 reserved строк: 6 `eval_candidate` и 6 `holdout_candidate`.

Итог:

- pass: 0;
- fail: 8;
- gap: 4.

Оценка выполнена через schema-level evaluator по P16D checkpoint sections. Это не production runtime.

## Почему нет positive pass

P16D содержит только четыре learned profiles:

- `task_understanding` -> `code_review`;
- `prompt_context` -> `documentation_sync`;
- `intermediate_checks` -> `verification_before_completion`;
- `skill_selection` -> `code_review`.

Reserved строки требуют более широкий набор skills: `claim_verification`, `planning`, `planning_dpo_authoring`, `trainable_architecture_decision`, `output_readability_gate`, `test_integrity` и другие. Эти skills не покрыты P16D profiles.

## Gap rows

Четыре строки получили gap:

- две `candidate_position_guard` строки: guard не имеет права выбирать skill;
- две `prompt_understanding` строки: section пустой, потому что evidence-only train row был исключен.

## Fail rows

Восемь строк получили fail: schema evaluator нашел overlap с существующими profiles или не нашел нужный profile, но predicted skill не совпал с expected chosen. Это означает недостаточную train-only coverage, а не semantic-quality failure всей системы.

## Вывод

P16E source-disjoint eval не дает positive SkillSelection evidence. Нужен repair с дополнительными train-only rows/material или отдельное parent решение о следующем bounded gap phase.
