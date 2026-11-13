# P9: проверка OrchestrationReasoning checkpoint

## Вердикт

`FAIL_EVIDENCE_READY_FOR_PARENT_REVIEW`.

P8B checkpoint свежо загружается через `OrchestrationReasoningMachine.LoadCheckpointAsync`, но не покрывает две зарезервированные source-disjoint положительные строки. Это честный компонентный FAIL, а не заявка на `P8 pass` или semantic quality.

## Что проверено

- P5E-DPO-ORCH-003: ветвление решения, обзор риска перед ремонтом, точечное исправление выбранного фрагмента.
- P5E-DPO-ORCH-005: изоляция независимых потоков и защита от переноса состояния.
- 14 shortcut-контролей: дословный маркер запроса, файл/путь/заголовок, `source_family_id`, `row_id`, ближайшее поверхностное совпадение, проектный маркер и перестановка порядка.

## Результаты

- Reserved positives: 0 passed, 2 failed.
- Ablation controls: 14 blocked, 0 shortcut leaks.
- Training/eval rows were not used as checkpoint input.
- Checkpoint was not rewritten.
- Final/common model assets were not touched.

## Почему это FAIL

P8B содержит train-only паттерны для границы полномочий, ядра/адаптера и зависимостей. Этого недостаточно для двух reserved behaviors: ветвление с обзором риска и изоляция независимых потоков.

## Что это не доказывает

Этот отчет не доказывает semantic quality, `P8 pass`, готовность API, готовность расширения, готовность финальной модели или SkillSelection.

## Следующее безопасное действие

Нужна отдельная train-only repair-фаза: добавить независимое покрытие для ветвления/точечного ремонта и изоляции потоков, сформировать новый checkpoint-кандидат и повторить P9-проверку.
