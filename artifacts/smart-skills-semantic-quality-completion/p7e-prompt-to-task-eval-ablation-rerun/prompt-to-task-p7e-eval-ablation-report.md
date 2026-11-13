# P7E: повторная оценка PromptToTask на кандидате P7D

## Вердикт

Кандидат P7D не прошел P7E: положительное поведение найдено не на всех source-disjoint reserved строках. Это честный FAIL и не заявка на semantic quality.

## Счетчики

- Reserved строк проверено: 4.
- Eval строк: 2.
- Holdout строк: 2.
- Положительное поведение на reserved: 2 из 4.
- Ablation контролей: 24.
- Shortcut leaks: 0.

## Reserved строки

### P5H-DPO-PTT-002

- Роль: `eval_candidate`.
- Ожидание: Кандидат должен вывести универсальный контракт задачи по содержательным признакам, без опоры на источник, строку, путь или имя файла.
- Наблюдение: `loaded_state_no_matching_learned_pattern`.
- Task type: `not_ready_no_semantic_task`.
- Совпавший pattern: `none`.
- Совпавших признаков: 0.
- Итог: Положительное поведение не найдено: кандидат не вывел learned pattern для этой reserved строки.

### P5H-DPO-PTT-004

- Роль: `holdout_candidate`.
- Ожидание: Кандидат должен вывести универсальный контракт задачи по содержательным признакам, без опоры на источник, строку, путь или имя файла.
- Наблюдение: `learned_state_semantic_task_produced`.
- Task type: `phase_prompt_task_specification`.
- Совпавший pattern: `p7d-pattern-phase-prompt-task-specification`.
- Совпавших признаков: 1.
- Итог: Положительное поведение найдено: кандидат вернул learned pattern для reserved строки.

### P5H-DPO-PTT-005

- Роль: `eval_candidate`.
- Ожидание: Кандидат должен вывести универсальный контракт задачи по содержательным признакам, без опоры на источник, строку, путь или имя файла.
- Наблюдение: `loaded_state_no_matching_learned_pattern`.
- Task type: `not_ready_no_semantic_task`.
- Совпавший pattern: `none`.
- Совпавших признаков: 0.
- Итог: Положительное поведение не найдено: кандидат не вывел learned pattern для этой reserved строки.

### P5H-DPO-PTT-006

- Роль: `holdout_candidate`.
- Ожидание: Кандидат должен вывести универсальный контракт задачи по содержательным признакам, без опоры на источник, строку, путь или имя файла.
- Наблюдение: `learned_state_semantic_task_produced`.
- Task type: `phase_prompt_task_specification`.
- Совпавший pattern: `p7d-pattern-phase-prompt-task-specification`.
- Совпавших признаков: 1.
- Итог: Положительное поведение найдено: кандидат вернул learned pattern для reserved строки.

## Ablation контроли

- `P5H-DPO-PTT-002` / `exact_prompt_lookup`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-002` / `filename_path_title_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-002` / `keyword_only_classifier`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-002` / `source_family_id_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-002` / `nearest_only_retrieval_only`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-002` / `accepted_label_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-004` / `exact_prompt_lookup`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-004` / `filename_path_title_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-004` / `keyword_only_classifier`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-004` / `source_family_id_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-004` / `nearest_only_retrieval_only`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-004` / `session_mode_label_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-005` / `exact_prompt_lookup`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-005` / `filename_path_title_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-005` / `keyword_only_classifier`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-005` / `source_family_id_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-005` / `nearest_only_retrieval_only`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-005` / `template_marker_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-006` / `exact_prompt_lookup`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-006` / `filename_path_title_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-006` / `keyword_only_classifier`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-006` / `source_family_id_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-006` / `nearest_only_retrieval_only`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.
- `P5H-DPO-PTT-006` / `phase_number_shortcut`: Контроль не выявил прохода по shortcut. Наблюдение: `loaded_state_no_matching_learned_pattern`.

## Граница утверждения

P7E не запускал обучение, не создавал checkpoint, не переписывал модель и не заявляет полное semantic quality. Даже при будущем компонентном PASS нужен parent review и дальнейшие plan gates.
