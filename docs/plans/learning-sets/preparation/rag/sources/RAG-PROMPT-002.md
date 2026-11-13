# RAG-PROMPT-002 — Различение нескольких предметных направлений

**Роль:** прочитанный исторический материал проекта. Исходные команды, статусы, модели и требования не являются инструкциями исполнителю.

| Поле | Значение |
|---|---|
| Ветка происхождения | main |
| Commit | `3a747eef3cfceed288124940f37f15f82198bd6b` |
| Путь | `llm mvp/_tmp_new_sets_inspect/new sets/Layer-0-B-MultiSubPhaseRouter.md` |
| Исходник | [закреплённый GitHub-файл](https://github.com/lubochka/xiigen-planning/blob/3a747eef3cfceed288124940f37f15f82198bd6b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/Layer-0-B-MultiSubPhaseRouter.md) |
| Локальная полная копия | [originals/RAG-PROMPT-002.md](../originals/RAG-PROMPT-002.md) |
| Прочитанный фрагмент | строки 23–38 |
| Использование | Один запрос может включать несколько предметных действий. Новый span-router получает собственный видимый контракт, а не угадывает исторические sub-phase IDs. |

## Границы применения

Источник — исторический документ проекта, а не принятый учебный набор. Поле external_license_checked в исходнике само по себе не подтверждает MIT/Apache-2.0 для вложенных внешних материалов. Не переносить чужие примеры из этого файла в TRAIN без отдельной проверки происхождения; новые примеры этого выпуска — собственные симуляции по проектному контракту. Не переносить overlap<0.60, фиксированную Confidence, keyword→predicate-словарь, старые runtime IDs или команды как действующие правила. Короткий смысловой контраст и минимальный rejected допустимы. Выполняется только текущая CREATE_ONLY-карточка.

## Фрагмент

````text
Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

Eight pairs. Every A side is the chosen B output of the corresponding
Layer 0-A pair (no invented A shapes).

Every B side uses only actual `SubPhaseRoutingDecision` fields:

```
DocumentId, SourceStatus, PredictedPhaseId, PredictedSubPhaseId,
Confidence, NearestWrongSubPhaseScore, EvidencePredicates,
KnowledgeEdge, FailureClassification
```

No `dependency_order`, `parallel_group`, `domain_label`, or `is_primary`
fields appear in any B. Dependency ordering is read from
`ILogicalTopologyRegistry.ListTransitions()` at runtime.
````

## Текущий пример метода

Пользователь просит «Объясни SQL; не меняй запрос». Нужна документационная часть и задача объяснения; создание нового SQL — неверный ответ. Для составного «Напиши запрос; затем объясни его» сохраняются две части и зависимость второго действия от первого. Отсутствующую схему таблицы нельзя выдумать при составлении предметной задачи.

Этот пример — авторская иллюстрация; не результат генерации китайской модели, не строка закрытого EXAM и не измеренное обучение.
