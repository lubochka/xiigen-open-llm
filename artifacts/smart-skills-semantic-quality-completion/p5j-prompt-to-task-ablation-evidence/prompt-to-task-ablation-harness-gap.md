# P5J: evidence по запуску PromptToTask ablation probes

## Вердикт

Найден честный harness gap. Все 36 probes из P5I остаются в статусе `not_executable`, потому что в текущем репозитории не найден существующий запускатель, который читает P5I JSONL, строит конкретные runtime-запросы и сохраняет наблюдаемые результаты по каждой probe.

Это не PASS/FAIL по качеству. Это evidence о границе исполнения: PromptToTask runtime был прочитан и проверен на наличие подходящего существующего пути, но P5I probes не запускались.

## Счетчики

- Planned probes: 36
- Executable probes: 0
- Not executable probes: 36
- Training-ready rows: 0
- Training allowed: false
- Eval allowed: false
- Semantic quality claimed: false

## Что найдено в коде

| Boundary | Вывод |
|---|---|
| `PromptToTaskMachine.Translate(PromptToTaskRequest)` | Есть одиночный runtime-метод, но нет существующего runner, который преобразует P5I plan probes в конкретные запросы и пишет результаты. |
| `PromptToTaskTaskPatternUnit.Decide(PromptToTaskRequest)` | Есть unit-level решение для готового запроса и состояния, но P5I не содержит готовый executable input для метода. |
| `PromptToTaskTaskPatternUnit.EvaluateHeldOutRows(...)` | Есть оценка готовых held-out rows, но P5I хранит план трансформаций, а не готовые rows. |
| `PromptToTaskLogicalOrchestrator.RunAsync(...)` | Нужен metadata object с `PromptToTaskRequest`; существующий P5I adapter не найден. |
| `XiigenExtensionApiService.ProcessHostExtensionRequest(...)` | Общий API boundary есть, но нет режима запуска 36 P5I controls с per-probe evidence. |
| Unit tests | Тесты фиксированные; их запуск не был бы исполнением P5I probes. |

## Почему нельзя выставить PASS/FAIL

P5I rows описывают будущие трансформации и ожидаемое устойчивое поведение, но не являются готовыми runtime-запросами. Чтобы получить настоящие observed outputs, нужен разрешенный harness, который свяжет `probe_id`, `control_id`, source-disjoint role и редактированный input с реальным вызовом PromptToTask runtime.

Создание такого harness в P5J запрещено: текущая фаза разрешает только существующий путь. Поэтому результаты не сфабрикованы.

## Проверка читаемости

Русская проза в P5J artifacts переписана после parent review. Проверены реальные mojibake-маркеры по кодовым точкам: U+00D0, U+00D1, четыре знака вопроса подряд и U+FFFD.

## Per-probe status

| Probe | Row | Role | Control | Status |
|---|---|---|---|---|
| `P5I-PTT-ABL-001` | `P5H-DPO-PTT-001` | `train_candidate` | `exact_prompt_lookup` | `not_executable` |
| `P5I-PTT-ABL-002` | `P5H-DPO-PTT-001` | `train_candidate` | `filename_path_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-003` | `P5H-DPO-PTT-001` | `train_candidate` | `keyword_only_classifier` | `not_executable` |
| `P5I-PTT-ABL-004` | `P5H-DPO-PTT-001` | `train_candidate` | `source_family_id_shortcut` | `not_executable` |
| `P5I-PTT-ABL-005` | `P5H-DPO-PTT-001` | `train_candidate` | `nearest_only_retrieval_only` | `not_executable` |
| `P5I-PTT-ABL-006` | `P5H-DPO-PTT-001` | `train_candidate` | `archive_heading_shortcut` | `not_executable` |
| `P5I-PTT-ABL-007` | `P5H-DPO-PTT-002` | `eval_candidate` | `exact_prompt_lookup` | `not_executable` |
| `P5I-PTT-ABL-008` | `P5H-DPO-PTT-002` | `eval_candidate` | `filename_path_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-009` | `P5H-DPO-PTT-002` | `eval_candidate` | `keyword_only_classifier` | `not_executable` |
| `P5I-PTT-ABL-010` | `P5H-DPO-PTT-002` | `eval_candidate` | `source_family_id_shortcut` | `not_executable` |
| `P5I-PTT-ABL-011` | `P5H-DPO-PTT-002` | `eval_candidate` | `nearest_only_retrieval_only` | `not_executable` |
| `P5I-PTT-ABL-012` | `P5H-DPO-PTT-002` | `eval_candidate` | `accepted_label_shortcut` | `not_executable` |
| `P5I-PTT-ABL-013` | `P5H-DPO-PTT-003` | `train_candidate` | `exact_prompt_lookup` | `not_executable` |
| `P5I-PTT-ABL-014` | `P5H-DPO-PTT-003` | `train_candidate` | `filename_path_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-015` | `P5H-DPO-PTT-003` | `train_candidate` | `keyword_only_classifier` | `not_executable` |
| `P5I-PTT-ABL-016` | `P5H-DPO-PTT-003` | `train_candidate` | `source_family_id_shortcut` | `not_executable` |
| `P5I-PTT-ABL-017` | `P5H-DPO-PTT-003` | `train_candidate` | `nearest_only_retrieval_only` | `not_executable` |
| `P5I-PTT-ABL-018` | `P5H-DPO-PTT-003` | `train_candidate` | `domain_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-019` | `P5H-DPO-PTT-004` | `holdout_candidate` | `exact_prompt_lookup` | `not_executable` |
| `P5I-PTT-ABL-020` | `P5H-DPO-PTT-004` | `holdout_candidate` | `filename_path_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-021` | `P5H-DPO-PTT-004` | `holdout_candidate` | `keyword_only_classifier` | `not_executable` |
| `P5I-PTT-ABL-022` | `P5H-DPO-PTT-004` | `holdout_candidate` | `source_family_id_shortcut` | `not_executable` |
| `P5I-PTT-ABL-023` | `P5H-DPO-PTT-004` | `holdout_candidate` | `nearest_only_retrieval_only` | `not_executable` |
| `P5I-PTT-ABL-024` | `P5H-DPO-PTT-004` | `holdout_candidate` | `session_mode_label_shortcut` | `not_executable` |
| `P5I-PTT-ABL-025` | `P5H-DPO-PTT-005` | `eval_candidate` | `exact_prompt_lookup` | `not_executable` |
| `P5I-PTT-ABL-026` | `P5H-DPO-PTT-005` | `eval_candidate` | `filename_path_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-027` | `P5H-DPO-PTT-005` | `eval_candidate` | `keyword_only_classifier` | `not_executable` |
| `P5I-PTT-ABL-028` | `P5H-DPO-PTT-005` | `eval_candidate` | `source_family_id_shortcut` | `not_executable` |
| `P5I-PTT-ABL-029` | `P5H-DPO-PTT-005` | `eval_candidate` | `nearest_only_retrieval_only` | `not_executable` |
| `P5I-PTT-ABL-030` | `P5H-DPO-PTT-005` | `eval_candidate` | `template_marker_shortcut` | `not_executable` |
| `P5I-PTT-ABL-031` | `P5H-DPO-PTT-006` | `holdout_candidate` | `exact_prompt_lookup` | `not_executable` |
| `P5I-PTT-ABL-032` | `P5H-DPO-PTT-006` | `holdout_candidate` | `filename_path_title_shortcut` | `not_executable` |
| `P5I-PTT-ABL-033` | `P5H-DPO-PTT-006` | `holdout_candidate` | `keyword_only_classifier` | `not_executable` |
| `P5I-PTT-ABL-034` | `P5H-DPO-PTT-006` | `holdout_candidate` | `source_family_id_shortcut` | `not_executable` |
| `P5I-PTT-ABL-035` | `P5H-DPO-PTT-006` | `holdout_candidate` | `nearest_only_retrieval_only` | `not_executable` |
| `P5I-PTT-ABL-036` | `P5H-DPO-PTT-006` | `holdout_candidate` | `phase_number_shortcut` | `not_executable` |

## Next bounded phase

`P5K_PROMPT_TO_TASK_ABLATION_HARNESS_IMPLEMENTATION_OR_AUTHORIZED_RUNNER`: явно разрешить создание или запуск harness, который читает P5I probes, строит редактированные `PromptToTaskRequest` или `PromptToTaskTaskPatternEvaluationRow`, запускает существующий PromptToTask runtime и пишет observed outputs. Без обучения, без eval-as-training, без checkpoint/model rewrite.
