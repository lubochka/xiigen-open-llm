# Независимый обзор root-документов 04/06/07

20.10.2026. **READ_ONLY_REVIEW; без runtime, обучения, scorer execution, сборок или исходных изменений.** Полностью прочитаны три указанных документа. Проверены актуальная policy 20.10, original `CodeDocumentationSimulationDpoExperiment` и `SimulationEnRuContinuation` в относящихся к выводу местах, два сохранённых scientific-result JSON, `evidence-validation.json`, сформированный reanalysis 24 и исходный отчёт P25 в разделах метода/результата. Это проверка источников и интерпретаций; повторный запуск assessor, модельных binary или native обучения не выполнялся.

## Вердикт

**Содержательно годны для включения в итоговый пакет.** Нового approval blocker не вводят; полезность margin/retention/resources отделена от answer accuracy; нулевой наблюдаемый параметрический эффект не объявлен ни повреждением, ни приобретённой способностью. Не выводят причину старой неудачи RP01 из новой гипотезы. Нужны две маленькие редакционные поправки 07 и проверка отложенных ссылок при сборке.

## 04 — no-delta native update

- Цепочка есть: exact input/admission → настоящий `StepVector` → completed counter → published checkpoint/AdamW/RNG readback → ошибочный combined guard. Это зависит от исхода настоящего обучения, а не самостоятельное упражнение с названием статуса.
- `DpoTrainer.StepVectorCore:921–923` действительно вызывает `ApplyOptimizer`, затем очищает градиенты. `CodeDocumentationSimulationDpoExperiment:205–207` смешивает отсутствие изменения adapter hash с integrity errors; строка220 записывает общий failed status.
- Предложенные null/true/false наблюдения правильно различают неизвестное и реально проверенное отсутствие дельты. Admission, base/reference, exact source identity, replay и saved/live optimizer guards сохраняются.
- Downstream независимо проверен: `SimulationEnRuContinuation.VerifySource:167–189` сначала проверяет все pins, затем требует точный `TRAINED_PRIVATE_CANDIDATE_NOT_ADOPTED` на 179 и count/settings. `OpenStudent:214–245` для C100 требует этот же статус на 234 плюс domain, global steps 50→100, sourceSHA, checkpoint/state/completed. Документ правильно **не** расширяет эти historical contracts новым neutral статусом. Малый no-delta candidate не становится C50 donor автоматически.
- Подключение одинакового наблюдения к обоим плечам PAD сохраняет единственный фактор PAD. Если ветвь no-delta не достигнута, её покрытие не заявляется. Искусственного Train/обнуления LR ради ветви нет.
- Исторические16 changed LoRA исключают это условие как объяснение конкретных C50/C100 free-answer failures. Документ это говорит явно.

Необязательная редактура: «математически выполненный конечный опыт» можно заменить на «завершившийся native опыт», чтобы исполнение не читалось как отдельное доказательство правильности всей математики.

## 06 — сохранённые успехи и границы

Числа нейронной таблицы сверены с фактическими локальными копиями указанных scientific-result JSON: C# margin 0.03733304977416992→0.03856058120727539, delta 0.0012275314331054651, 14/11/0; ENRU 0.012293052673339844→0.012403059005737304, delta 0.00011000633239746093, 11/9/0. Counts 500/50, 16 adapters, 39 frozen base и 0/3, 0/4 free answers соответствуют источникам. Сдвиг score честно признан успехом именно своей оси; significance и генерализация не выдуманы.

Таблица ingress соответствует 24-pair reanalysis: 11/24→19/24, композиция 0/8→8/8, accepted 12→22, wrong accepted 1→3, ни одного lost correct. Canonical initial equality и matched DLL/material за исключением phrase mappings подтверждены именно **сохранённым** evidence-validation, без нового binary rerun. Это явно оговорено. Указаны вредные polarity/question исходы, а coordination-case назван failure существующей фиксированной рубрики; не присвоена новая независимая semantic grading.

P25 исходный отчёт подтверждает 19/24, 24/24 outputs/routes, zero new training и сохранённые 3 ошибки. Его положительная проверка восстановления допустима по policy 20.10 без повторного Train. Source limitation random/untrainedB0 действительно находится в ENRU scientific-result; уникальной причиной она не объявлена. Несопоставимые C#/ENRU времена не превращены в speedup; ingress timing тоже не подменяет causal benchmark.

Ограничение этого review: основной parent заново выполнил чистый retained `assess`; reviewer прочитал его итоговый reanalysis и исходные receipts, но scorer повторно не запускал. Это не еще одна независимая runtime репликация.

## 07 — scope и порядок

Policy20.10 отражена верно: PREPARABLE — задача подготовки, NOT_RUN — отсутствие результата, нет повторного личного approval ID/SHA. Документ не отменяет ранее разрешённую программу, одновременно честно описывает текущее выполненное задание как аудит/Markdown. Передача learned state обычному consumer допустима; прежнее blanket query-only exclusion явно отозвано. Обязательный 500-pair rerun/универсальная kNN перепись не навязаны.

**Две предложенные правки:**

1. Строка 53: «измерять корректно нечисленные затраты и эффекты» → «корректно измерять затраты и эффекты». Затраты времени/CPU/RAM здесь численные.
2. Рядом с очередью 52–55 добавить: «Загрузка или восстановление, без которого выбранное настоящее обновление невозможно, входят в подготовку первого опыта. Поздняя строка consumer/restore относится к самостоятельным проверкам сохранности и использования результата». Это не оставит впечатления, что blocking loader нужно отложить после всех neural изменений, и сохраняет правило пользователя сначала разблокировать обучение.

## Ссылки при финальной сборке

На момент обзора пакет ещё собирался. Ссылки на будущие 03 PAD, 05 T и 08 kNN должны получить окончательные файлы с этими именами. Это условие упаковки, а не содержательный дефект или новый запрос разрешения. `inventory/source-scope.json` также должен существовать к моменту публикации, поскольку 07 ссылается на сохранённые там hashes; reanalysis inventory уже существует.

| Документ | Ссылка | Существует на момент проверки |
|---|---|---|
| 04-NATIVE-UPDATE-ZERO-DELTA-RESULT.md | `03-DPO-PAD-IMPLEMENTATION-BLUEPRINT.md` | ещё нет, финальная сборка |
| 06-RETAINED-LEARNING-SUCCESSES-AND-LIMITS.md | `inventory/retained-learning-result-reanalysis.json` | да |
| 06-RETAINED-LEARNING-SUCCESSES-AND-LIMITS.md | `08-KNN-HISTORY-AND-EXPERIMENT-REVIEW.md` | ещё нет, финальная сборка |
| 07-APPLIED-UPDATES-AND-LEARNING-PRIORITY.md | `05-T-TRAIN-CONTEXT-SMALL-EXPERIMENT.md` | ещё нет, финальная сборка |


## Граница доказательств

Все выводы относятся к выбранным трем документам и перечисленным source/receipt участкам. Проверка input/output/metrics не означает повторного целостного аудита прошлых campaigns, всех RAG, всех численных ядер или физической доступности B0. Собственные изменения reviewer — только этот Markdown.
