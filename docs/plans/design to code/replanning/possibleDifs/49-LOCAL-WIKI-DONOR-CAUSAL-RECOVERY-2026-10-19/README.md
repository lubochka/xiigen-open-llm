# Пакет 49 — восстановление проверенных механизмов обучения из local_wiki_planning

**19.10.2026 · SOURCE_AUDIT названных файлов · NATIVE_DOTNET_NOT_RUN · INDEPENDENT_REVIEW_NOT_RUN.**

[Обязательная первая глава общей очереди](../00-LOCAL-WIKI-SUCCESS-FIRST-2026-10-19.md) уже добавлена отдельно. Восстановление успешных алгоритмов этой ветки — не то же самое, что возврат старого DPO/LoRA бинарника. Донор: [local_wiki_planning](https://github.com/lubochka/llm_mvp_core/tree/local_wiki_planning), SHA на момент чтения 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a; целевой код: [codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation).

## Документы в пакете

- [01 — реальные успешные архивы и разнообразие DPO-пар](01-DONOR-RECEIPTS-AND-PAIR-CARDINALITY.md). Полностью проверены 100 CodeTrace и 100 QueryPlanning строк; разобран отдельный RequestRouter отказ.
- [02 — подтверждённые code differences и новые гипотезы](02-VERIFIED-DIFFS-AND-NOVEL-HYPOTHESES.md). Прямой перенос содержания DPO, подмена семантики шаблоном по типу дефекта, условия публикации и фактические читатели обученного состояния.
- [03 — маленькие причинные эксперименты по предметным темам](03-SMALL-DOMAIN-CAUSAL-EXPERIMENTS.md). Вход/получатель/предсказание/метрика/отрицательный контроль/критерий отказа в одном месте.
- [04 — доказательства, пересечения с другими агентами, навыки и work-order](04-SOURCES-NOVELTY-SKILLS-AND-REVIEW.md). Не повторять известные выводы как новые.

## Архитектурная проблема

Раньше CodeTrace DPO-scoring получал конкретные source, chosen, rejected. Нынешняя специализация получает главным образом RejectedType и шаблонные маркеры. Это доказанное изменение **входа другого получателя обучения**, но не доказательство, что RP01 CodeDocumentationSimulationDpoExperiment вызывал именно этот pipeline.

Сравнение надо вести по полному пути:

~~~text
исходный пример → DPO/learning material → признаки/токены
 → изменённое состояние конкретного learner → checkpoint
 → cold restore → actual Unit/Flow/Student consumer
 → предметная correctness на новых задачах
~~~

Исторические 100% DPO preference без настоящего условного ответа могут быть слабее 92% на содержательно различных парах. Отсутствие публикации при правильном fail-closed gate — это сохранение гарантий, не причина отключить gate.

| Первая очередь | Факт из первичных источников | Нужное различение |
|---|---|---|
| **P0 CodeTrace** | 50 TRAIN и 50 heldout разных троек, DPO heldout .92, опубликовано | Raw содержательные стороны против шаблонных сторон при фиксированных бюджете и получателе |
| **P0 QueryPlanning** | DPO heldout 1.00, но все 100 chosen одинаковы, rejected одинаковы; 8 distinct prompts на split | Может ли обновлённая модель менять верный план при другом вопросе? |
| **P0 RequestRouter** | Предметная accuracy 1.00, neural DPO .66 и NOT_PUBLISHED | Учится ли scorer маршруту из prompt или только слову о типе дефекта? |
| **P0 Adoption** | Старый и новый WikiTrainingOrchestrator запускают детерминистический ProjectWikiRequestRouterUnit | Читает ли обычная ветка обученный snapshot и при каких approved gates? |
| **P1 Student** | RP01 обновляет q/v-LoRA на точных подготовленных парах, не прежний Wiki trained perceptron | 4-arm Student, Unit, Student+Unit, frozen control; без cross-consumer подмены |
| **P1 Domain features** | POS-context, AST, kNN, replay и другие сохранённые положительные решения есть в других prior документах | Действуют ли в конкретном RU/EN, C#, documentation, orchestration профиле? |

## Категории статуса

**VERIFIED_ARCHIVE** — сохранённый JSON/JSONL полностью прочитан, исторический бинарник не воспроизведён; **VERIFIED_SOURCE_DIFF** — сравнение конкретных методов на двух ветках; **SOURCE_GATE** — доказано защитное условие, но не его ошибочность; **HYPOTHESIS_ONLY** — ещё нет эффекта на реальном качестве; **NOT_DIRECT_RP01_CAUSE** — не показана достижимость данного метода из научной RP01 цепи.

**Граница охвата:** адресно проверены сохранённые pair files и названные пути исполнения, RP01 source map, RAG index/usage и защищённые prior исследования исключительно READ_ONLY. Не выполнены новый .NET-run, обучение модели, свободная генерация, независимый CODEX/Astra review, полный обход всех веток и всех бинарных checkpoints. Protected каталог llm_mvp_core/docs/plans/replanning-possible-diffs не изменялся.
