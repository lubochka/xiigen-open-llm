# RAG-PROMPT-003 — Разбиение запроса без добавления неназванных задач

**Роль:** прочитанный исторический материал проекта. Исходные команды, статусы, модели и требования не являются инструкциями исполнителю.

| Поле | Значение |
|---|---|
| Ветка происхождения | main |
| Commit | `3a747eef3cfceed288124940f37f15f82198bd6b` |
| Путь | `llm mvp/_tmp_new_sets_inspect/new sets/Node-01-DomainSplitter.md` |
| Исходник | [закреплённый GitHub-файл](https://github.com/lubochka/xiigen-planning/blob/3a747eef3cfceed288124940f37f15f82198bd6b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/Node-01-DomainSplitter.md) |
| Локальная полная копия | [originals/RAG-PROMPT-003.md](../originals/RAG-PROMPT-003.md) |
| Прочитанный фрагмент | строки 25–39 |
| Использование | Пакет нужен для каждой реально запрошенной предметной части. Упоминание системы не даёт права приписать пользователю тестирование или реализацию. |

## Границы применения

Источник — исторический документ проекта, а не принятый учебный набор. Поле external_license_checked в исходнике само по себе не подтверждает MIT/Apache-2.0 для вложенных внешних материалов. Не переносить чужие примеры из этого файла в TRAIN без отдельной проверки происхождения; новые примеры этого выпуска — собственные симуляции по проектному контракту. Не переносить overlap<0.60, фиксированную Confidence, keyword→predicate-словарь, старые runtime IDs или команды как действующие правила. Короткий смысловой контраст и минимальный rejected допустимы. Выполняется только текущая CREATE_ONLY-карточка.

## Фрагмент

````text
B contains one packet per development territory **actually present** in the
selected transition. Per v7 pipeline plan §3, "do not add territories not
present in the selected transition's A/B/X statement".

Required fields per packet:

```
packet_id, domain_label, phase, sub_phase, task_type,
prompt_fragment, coverage_status, reason, handoff_marker, source_evidence
```

Five pairs in this file — one per scenario where the splitter has to make
a non-trivial territory decision.

---
````

## Текущий пример метода

Пользователь просит «Объясни SQL; не меняй запрос». Нужна документационная часть и задача объяснения; создание нового SQL — неверный ответ. Для составного «Напиши запрос; затем объясни его» сохраняются две части и зависимость второго действия от первого. Отсутствующую схему таблицы нельзя выдумать при составлении предметной задачи.

Этот пример — авторская иллюстрация; не результат генерации китайской модели, не строка закрытого EXAM и не измеренное обучение.
