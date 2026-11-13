# P5K: runtime evidence по PromptToTask ablation probes

## Вердикт

Все 36 запланированных проверки из предыдущего плана были превращены в редактированные runtime-запросы и переданы в `PromptToTaskMachine.Translate(PromptToTaskRequest)`. Текущий результат честный: без загруженного обученного состояния runtime возвращает `not_ready`, а не PASS/FAIL по смысловому качеству.

Обучение, импорт или экспорт модели, запись checkpoint и утверждение semantic quality не выполнялись.

## Счетчики

- Planned probes: 36
- Runtime invoked: 36
- Not ready: 36
- Runtime output other than not-ready: 0
- Runtime errors: 0
- Training-ready rows: 0
- Semantic quality claimed: false

## Per-probe runtime result

| Probe | Row | Role | Control | Runtime status | Observed output |
|---|---|---|---|---|---|
| `P5I-PTT-ABL-001` | `P5H-DPO-PTT-001` | `train_candidate` | `exact_prompt_lookup` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-002` | `P5H-DPO-PTT-001` | `train_candidate` | `filename_path_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-003` | `P5H-DPO-PTT-001` | `train_candidate` | `keyword_only_classifier` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-004` | `P5H-DPO-PTT-001` | `train_candidate` | `source_family_id_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-005` | `P5H-DPO-PTT-001` | `train_candidate` | `nearest_only_retrieval_only` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-006` | `P5H-DPO-PTT-001` | `train_candidate` | `archive_heading_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-007` | `P5H-DPO-PTT-002` | `eval_candidate` | `exact_prompt_lookup` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-008` | `P5H-DPO-PTT-002` | `eval_candidate` | `filename_path_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-009` | `P5H-DPO-PTT-002` | `eval_candidate` | `keyword_only_classifier` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-010` | `P5H-DPO-PTT-002` | `eval_candidate` | `source_family_id_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-011` | `P5H-DPO-PTT-002` | `eval_candidate` | `nearest_only_retrieval_only` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-012` | `P5H-DPO-PTT-002` | `eval_candidate` | `accepted_label_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-013` | `P5H-DPO-PTT-003` | `train_candidate` | `exact_prompt_lookup` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-014` | `P5H-DPO-PTT-003` | `train_candidate` | `filename_path_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-015` | `P5H-DPO-PTT-003` | `train_candidate` | `keyword_only_classifier` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-016` | `P5H-DPO-PTT-003` | `train_candidate` | `source_family_id_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-017` | `P5H-DPO-PTT-003` | `train_candidate` | `nearest_only_retrieval_only` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-018` | `P5H-DPO-PTT-003` | `train_candidate` | `domain_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-019` | `P5H-DPO-PTT-004` | `holdout_candidate` | `exact_prompt_lookup` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-020` | `P5H-DPO-PTT-004` | `holdout_candidate` | `filename_path_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-021` | `P5H-DPO-PTT-004` | `holdout_candidate` | `keyword_only_classifier` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-022` | `P5H-DPO-PTT-004` | `holdout_candidate` | `source_family_id_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-023` | `P5H-DPO-PTT-004` | `holdout_candidate` | `nearest_only_retrieval_only` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-024` | `P5H-DPO-PTT-004` | `holdout_candidate` | `session_mode_label_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-025` | `P5H-DPO-PTT-005` | `eval_candidate` | `exact_prompt_lookup` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-026` | `P5H-DPO-PTT-005` | `eval_candidate` | `filename_path_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-027` | `P5H-DPO-PTT-005` | `eval_candidate` | `keyword_only_classifier` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-028` | `P5H-DPO-PTT-005` | `eval_candidate` | `source_family_id_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-029` | `P5H-DPO-PTT-005` | `eval_candidate` | `nearest_only_retrieval_only` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-030` | `P5H-DPO-PTT-005` | `eval_candidate` | `template_marker_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-031` | `P5H-DPO-PTT-006` | `holdout_candidate` | `exact_prompt_lookup` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-032` | `P5H-DPO-PTT-006` | `holdout_candidate` | `filename_path_title_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-033` | `P5H-DPO-PTT-006` | `holdout_candidate` | `keyword_only_classifier` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-034` | `P5H-DPO-PTT-006` | `holdout_candidate` | `source_family_id_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-035` | `P5H-DPO-PTT-006` | `holdout_candidate` | `nearest_only_retrieval_only` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
| `P5I-PTT-ABL-036` | `P5H-DPO-PTT-006` | `holdout_candidate` | `phase_number_shortcut` | `not_ready` | Runtime был вызван и честно вернул отсутствие готовой смысловой задачи, потому что обученное состояние не загружено. |
