# P5E redacted DPO authoring card

Этот артефакт содержит семь redacted DPO authoring rows для parent review. Это не training set: training_allowed=false, eval_allowed=false, training_ready_rows=0, semantic quality не заявляется.

## P5E-DPO-PTT-001

- Source input: `P3-PTT-DRAFT-001`, target machine: `PromptToTask`, source family: `sf-prompt-to-task-basic-prompt-77A175A9`.
- Universal boundary: PromptToTask должен извлекать контракт и проверочную границу до действия, а не угадывать исполнительный маршрут по поверхности запроса.
- Chosen: Сначала превратить просьбу в проверяемый контракт задачи: цель, входные ограничения, разрешенный объем, запрещенные действия, ожидаемые артефакты, допустимую проверку и границу того, что нельзя заявлять как готовность.
- Rejected: Сразу начать выполнение по общему смыслу просьбы, кратко пересказать намерение пользователя и отложить проверочные границы до конца, потому что задача кажется понятной.
- Почему rejected полезен: это близкая ошибка типа `execution_first_without_contract_boundary`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены исходный путь, название источника, буквальный текст частного промпта, сигналы имени файла и любые локальные технические подсказки.
- Ablation: `exact_prompt_lookup`, `filename_path_lookup`, `keyword_only_classifier`, `source_title_shortcut`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## P5E-DPO-ORCH-001

- Source input: `P4-ORCH-DPO-DRAFT-001`, target machine: `OrchestrationReasoning`, source family: `sf-orchestration-phase-transition-8CD45F23`.
- Universal boundary: OrchestrationReasoning должен удерживать границу фаз: артефакт, self-check, parent review и следующий шаг не подменяют друг друга.
- Chosen: Завершить текущую фазу только после сбора evidence, самопроверки и родительского review; следующий шаг описать как отдельную границу, не смешивая его с еще не принятой работой.
- Rejected: Перейти к следующей фазе сразу после появления чернового артефакта, считая сам факт наличия документа достаточным evidence и оставляя review на более поздний общий проход.
- Почему rejected полезен: это близкая ошибка типа `phase_boundary_crossed_before_review`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены путь интеграции, название протокола, заголовки источника, точные формулировки исходного правила и любые подсказки по имени файла.
- Ablation: `filename_path_lookup`, `source_title_shortcut`, `static_phase_keyword_shortcut`, `exact_protocol_text_lookup`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## P5E-DPO-PTT-002

- Source input: `P5A-CAND-PTT-AUTHORITY-RULES-001`, target machine: `PromptToTask`, source family: `sf-p5a-prompt-to-task-authority-rules-54DF62A3`.
- Universal boundary: PromptToTask учится извлекать власть, объем, запреты и evidence boundary; внутренние технические ярлыки не должны становиться смыслом задачи.
- Chosen: Из правил задачи выделить контракт: кто дал текущее указание, какие входы разрешены, какие действия запрещены, какой результат нужен, какая проверка допустима и где требуется явное подтверждение перед продолжением.
- Rejected: Выбрать технический маршрут по внутренним ярлыкам, начать реализацию и считать ограничения вторичными, потому что в правилах встречаются знакомые слова про исполнение.
- Почему rejected полезен: это близкая ошибка типа `technical_shortcut_over_task_contract`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены имя правилового файла, названия локального проекта, язык реализации, имена сервисов, классов, индексов и фрагменты кода.
- Ablation: `filename_path_shortcut`, `stack_marker_leakage`, `service_class_name_leakage`, `exact_code_snippet_replay`, `source_state_shortcut`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## P5E-DPO-ORCH-002

- Source input: `P5A-CAND-BOTH-SKILLS-INVENTORY-001`, target machine: `OrchestrationReasoning`, source family: `sf-p5a-both-skills-inventory-905F88B4`.
- Universal boundary: Универсальная orchestration хранит дисциплину принятия решения, а не локальные adapter mechanics; переносимость важнее совпадения с конкретным окружением.
- Chosen: Разделить переносимое ядро и адаптер: общая orchestration отвечает за режим сессии, требования, evidence scope, review boundary и completion boundary, а локальные политики подставляются отдельным слоем.
- Rejected: Объявить все правила универсальными, встроить локальные команды и карту проекта в общий engine и выбрать маршрут по тому, как назывался исходный набор правил.
- Почему rejected полезен: это близкая ошибка типа `adapter_policy_leaked_into_common_engine`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены имена файлов и путей, названия языков и фреймворков, project map, команды, локальные conventions, названия skill-файлов и source-title cues.
- Ablation: `file_name_shortcut`, `skill_name_shortcut`, `stack_marker_shortcut`, `command_marker_shortcut`, `project_map_marker_shortcut`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## P5E-DPO-ORCH-003

- Source input: `P5A-CAND-ORCH-DOC07956-001`, target machine: `OrchestrationReasoning`, source family: `sf-p5a-orch-universal-orchestrator-plan-C10AD9A5`.
- Universal boundary: Оркестрация должна сохранять независимость веток, evidence-before-merge и targeted repair; порядок из источника не является runtime input.
- Chosen: Для сложной задачи выделить независимые ветки ответственности, собрать evidence в каждой ветке, выбрать review до финального оформления, объединить только принятые ветки и вернуть на ремонт лишь сбойную ветку.
- Rejected: Схлопнуть все ветки в один общий успешный маршрут, оформить финальный ответ до review и при сбое перезапустить всю задачу вместо точечного ремонта проблемной ветки.
- Почему rejected полезен: это близкая ошибка типа `single_path_late_review_full_reset`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены folder id, title, node filenames, node numbers, branded phrases, source-document-line framing и структура пакета.
- Ablation: `folder_title_shortcut`, `node_label_order_shortcut`, `source_document_line_shortcut`, `branded_phrase_shortcut`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## P5E-DPO-ORCH-004

- Source input: `P5A-CAND-ORCH-UNIVERSAL-DEPS-001`, target machine: `OrchestrationReasoning`, source family: `sf-p5a-orch-dependency-relations-7DB3DAB0`.
- Universal boundary: OrchestrationReasoning учится dependency reasoning: root decision, dependent checks, relation labels and sequencing без опоры на исходные code labels.
- Chosen: Найти главное решение задачи, затем связать с ним зависимые evidence items, gates и review obligations как отношения; порядок работы строится по зависимостям, а не по первому заметному элементу.
- Rejected: Идти по поверхностному порядку элементов, сделать первый заметный шаг главным и построить план по терминам источника вместо проверки отношений между решением, evidence и gates.
- Почему rejected полезен: это близкая ошибка типа `surface_order_instead_of_dependency_reasoning`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены расширение исходника, путь, язык реализации, namespace, class/method names, parser mechanics, grammar fields, relation token syntax и project-specific service details.
- Ablation: `file_extension_shortcut`, `class_method_word_shortcut`, `namespace_like_token_shortcut`, `grammar_label_shortcut`, `source_title_shortcut`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## P5E-DPO-ORCH-005

- Source input: `P5A-CAND-ORCH-FLOW-ISOLATION-001`, target machine: `OrchestrationReasoning`, source family: `sf-p5a-orch-flow-isolation-F85D7BAA`.
- Universal boundary: Повторно используемый orchestration asset требует evidence isolation и no-cross-contamination checks до любого readiness claim.
- Chosen: Перед readiness claim для reusable orchestration asset проверить изоляцию параллельных веток, отсутствие state bleed между запусками, безопасное схлопывание дублей и независимое evidence для результата.
- Rejected: Принять reusable asset как ready после одного успешного smoke check или из-за наличия общего компонента, не проверяя state bleed, duplicate work и независимость evidence.
- Почему rejected полезен: это близкая ошибка типа `single_smoke_check_as_readiness`; она правдоподобна, но учит неверному shortcut behavior вместо переносимой orchestration boundary.
- Leakage removed: Удалены test framework markers, assertion words, test method names, run ids, temp paths, singleton mechanics, exact count assertions и local test/project terms.
- Ablation: `test_label_shortcut`, `assertion_word_shortcut`, `run_id_marker_shortcut`, `temp_path_marker_shortcut`, `exact_count_marker_shortcut`.
- Review boundary: dpo_ready_for_parent_review=true, training_ready=false.

## Итог

Всего строк: 7. PromptToTask: 2. OrchestrationReasoning: 5. Все строки требуют parent review before training/eval.
