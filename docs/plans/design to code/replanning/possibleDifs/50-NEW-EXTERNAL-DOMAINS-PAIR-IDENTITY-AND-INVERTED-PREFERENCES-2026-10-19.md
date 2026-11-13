# N50 — новый измеренный пробел external_domains: равные и противоположные стороны DPO, случайная метрика сходства

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — P70/P81

Текущий WikiTrace converter уже доставляет A/Chosen/Rejected;50 combined vectors не равны50 unique prompt-only inputs. P81 contrast1.71827459 при d(T2)>0 демонстрирует, почему каждый intended sign оценивается отдельно. Проверять actual input IDs/masks, response side, identical/inverted/zero-weight pairs и baseline matched rows. Historical external_domains counts не экстраполируются на этот набор или весь corpus.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**Дата:** 2026-10-19. **Состояние:** `SOURCE_MEASURED + CODE_PATH_VERIFIED / RP01_CAUSAL_EFFECT_NOT_RUN`. **Donor:** [external_domains @ 3036a01](https://github.com/lubochka/llm_mvp_core/tree/3036a01acec938f0b83fa4ffbda26202cc35d639). **Сфера:** исторический `external domains/documentation/learning sets/**`, не RP01 simulation C#500 без дополнительной проверки caller lineage. **Запрещённая область записи:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**`.

## 1. Прямая проверка сохранённых данных — 15 файлов, 3 000 записей

Прочитаны **все строки** `*-run-001.jsonl`, `*-run-002.jsonl`, `*-run-003.jsonl` в каждой из пяти директорий. Для каждой строки сравнивались точные строки `Prompt`, `Chosen`, `Rejected` (без нормализации и эвристик). Вычислены точные совпадения ответов, число уникальных входов, комбинации `curriculum_tier` с `rejected_type`. Остальные **47 файлов на предметное направление не анализировались**; экстраполяция на все 50 запусков недопустима.

| Тема/задача документирования | Строк | Разных `Prompt` | `Chosen == Rejected` | `tier=3` + `missing-edge-case` |
|---|---:|---:|---:|---:|
| C#/.NET | 600 | 15 | **50** | **53** |
| SQL/T-SQL | 600 | 11 | **50** | **53** |
| Angular/TypeScript | 600 | 10 | **50** | **53** |
| MongoDB/C# | 600 | 11 | **60** | **30** |
| Elasticsearch/C# | 600 | 8 | **60** | **30** |
| **Итого** | **3 000** | **55** | **270 (9%)** | **219 (7,3%)** |

В разрезе запусков:
- C# / SQL / Angular: `run-001` — 13 ties / 22 possible reversals; `run-002` — 19 / 13; `run-003` — 18 / 18.
- Mongo / Elasticsearch: `run-001` — 14 / 12; `run-002` — 27 / 5; `run-003` — 19 / 13.

Первичные файлы, проверенные по всем строкам: [C# 001](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-001.jsonl), [C# 002](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-002.jsonl), [C# 003](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-003.jsonl); [SQL 001](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/sql-tsql/sql-tsql-run-001.jsonl), [SQL 002](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/sql-tsql/sql-tsql-run-002.jsonl), [SQL 003](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/sql-tsql/sql-tsql-run-003.jsonl); [Angular 001](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/angular-typescript/angular-typescript-run-001.jsonl), [Angular 002](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/angular-typescript/angular-typescript-run-002.jsonl), [Angular 003](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/angular-typescript/angular-typescript-run-003.jsonl); [Mongo 001](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/mongodb-csharp/mongodb-csharp-run-001.jsonl), [Mongo 002](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/mongodb-csharp/mongodb-csharp-run-002.jsonl), [Mongo 003](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/mongodb-csharp/mongodb-csharp-run-003.jsonl); [Elastic 001](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/elasticsearch-csharp/elasticsearch-csharp-run-001.jsonl), [Elastic 002](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/elasticsearch-csharp/elasticsearch-csharp-run-002.jsonl), [Elastic 003](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/elasticsearch-csharp/elasticsearch-csharp-run-003.jsonl).

## 2. N50-A: 270 пар с НУЛЕВЫМ смысловым контрастом

**CODE-VERIFIED:** [`build_chosen` L917–926](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/generate_learning_sets.py#L917-L926) при `tier=2` строит текст `**Intent:** ...\n\n**Design decision:** ...`. [`build_rejected` L950–955](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/generate_learning_sets.py#L950-L955) при `missing-edge-case` **строит точно ту же строку**. Маркер `[WRONG: ...]` возвращается вторым результатом и записывается в **отдельное JSON-поле** [L968–1002](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/generate_learning_sets.py#L968-L1002), поэтому он не отличает видимые стороны. Прямая подтверждающая строка: [C# run-001, line 3](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-001.jsonl#L3).

**Что известно:** идеальный pairwise DPO для идентичных `Chosen/Rejected` не получает полезного *различающего* контраста; они должны быть отклонены при подготовке. **Что неизвестно:** дошли ли эти строки до реально выполнявшегося исторического тренера; текущий [RP01 simulation Inspect L65–71](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L65-L71) явно отказывает при `Chosen == Rejected`. Следовательно, **нельзя говорить, что именно эти 270 пар создали нулевые градиенты в RP01**.

**Новая гипотеза N50-A/H1:** исторический обучающий конвейер либо молча расходовал budget на ties, либо текущий конвертер отверг некоторую долю задуманного обучения; проверить счётчики `raw → admitted → materialized → trained` c отказами по причине и конкретными pair_id, не переписывая историю.

## 3. N50-B: 219 пар с подозрением на обратную учебную метку

**CODE-VERIFIED:** при `tier=3` preferred `Chosen` содержит только `**Intent:** ...`. При `missing-edge-case` отвергаемая сторона содержит `**Intent:** ...` + `**Design decision:** ...` и, по собственной заявленной шкале [исторического плана L49–77](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/PLAN-CODE-DOCUMENTATION-DPO-v2.md#L49-L77), может быть **качественнее** предпочтительной. Пример: [C# run-001, line 48](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-001.jsonl#L48).

**Граница доказательства:** `POTENTIAL_LABEL_INVERSION`, а не `SEMANTICALLY_REVERSED_CONFIRMED`: независимый эксперт должен оценить обе стороны по смыслу, а не по длине/формату. Причинный эффект на обученную модель `NOT_RUN`.

## 4. N50-C: метрика перекрытия не вычисляется

В [L26–32](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/generate_learning_sets.py#L26-L32) заданы диапазоны `OVERLAP_RANGES`; [L964–977](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/generate_learning_sets.py#L964-L977) возвращает `round(rng.uniform(lo, hi),3)`, **не токенизируя ни одну сторону**. Метка `annotation_strong` производна от случайного числа `or_val > 0.45`. В [C# 001 строка 1](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-001.jsonl#L1) и [строка 31](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/learning%20sets/dotnet-csharp/dotnet-csharp-run-001.jsonl#L31) одинаковые `Prompt/Chosen/Rejected`, но `overlap_ratio=0.299` против `0.324`. Идентичный материал не может иметь два разных детерминированных значения фактического token-overlap при неизменном tokenizer.

Исторический [план L86–104](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/PLAN-CODE-DOCUMENTATION-DPO-v2.md#L86-L104) требовал реально вычислять соотношение и отсеивать `>0.60`; здесь вместо измерения опубликована оценка по типу ошибки. **Если** downstream фильтр доверяет полю, качество допуска может быть искажено — проверка реального downstream consumer обязательна.

## 5. N50-D: 55 задач под 3 000 строк, возможная подмена обобщения повтором

Файл создаётся циклическим выбором шаблона `shuffled_tmpl[i % len(shuffled_tmpl)]` [L1021–1039](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/generate_learning_sets.py#L1021-L1039); 15 проверенных файлов имеют лишь **55 уникальных Prompt**. Запусков много, независимых исходных задач очень мало. Это источник риска утечки template-family в валидацию и неустойчивого generalization. **Не доказано**, что историческая обученная модель действительно тренировалась на всех этих файлах или что её положительные метрики получены на них. Проверять отдельно `prompt-family-disjoint`, `repository/file-disjoint`, `literal-ID-overlap` и близкие модификации задач.

## 6. N50-E: непроведённая компиляция и invisible annotation

Все 3 000 проверенных строк имеют `compiled_prompt_in_training=false`, в то время как [план L86–92](https://github.com/lubochka/llm_mvp_core/blob/external_domains/external%20domains/documentation/PLAN-CODE-DOCUMENTATION-DPO-v2.md#L86-L92) предписывает compiled prompt там, где compiler есть. Значимость зависит от языка и конкретного compiler, поэтому **C#/SQL/Angular** проверять отдельно; не объявлять false ошибкой, если компиляция недоступна/не применима по контракту.

Все эти строки хранят `annotation` в метаданных и **не включают** `[WRONG]` в `Rejected`. Это формальное расхождение с тем же планом. Однако слепое добавление `[WRONG]` в обучаемый отрицательный ответ способно научить модель распознавать метку вместо смысловой ошибки; такой патч допустим только после отдельного ослеплённого A/B + отрицательного контроля. Сопоставить с [существующим исследованием annotation→translation-memory](41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md): оно касается *другого* consumer и не подтверждает эффект здесь.

## 7. N50-F: несовместимые загрузчики = возможная потеря учебного корпуса, а не слабость модели

[Старый loader L18–26](https://github.com/lubochka/llm_mvp_core/blob/external_domains/11.DPO/DpoDataset.cs#L18-L26) десериализует `Prompt/Chosen/Rejected` в legacy `DpoPair`. [Современный loader L77–126](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoDataset.cs#L77-L126) также умеет legacy, но возвращает `Admission=null`; это **не разрешение тренировать**. [Текущий simulation Inspect L57–75](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L57-L75) принимает другой строгий контракт (`id/domain/provenance/prompt/chosen/rejected/...` в lowercase), явно отвергает extra поля и равные стороны. Исторический JSONL напрямую не соответствует этому контракту; это **не доказывает фактический отказ**, пока не указан настоящий caller. Данные надо конвертировать **только после** проверки прав/происхождения и сохранения сопоставимой идентичности пары; не отменять входные проверки.

## 8. Сверка с уже работавшими агентами / граница новизны

- [N38 E09 within-file](38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md): проблема **AST-узлов одного C# файла**, а здесь **синтетические source template families**; пересечение только в общем семействе leakage.
- [N40 corpus identity](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md): проблемы resume и novelty admission; **не** измеренная частота равных/потенциально обратных пар в archived external domains.
- [N41 annotation](41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md): dangerous rejected→translation mappings, другой recipient.
- [N44 requested mechanisms](44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md): RP01 C#500 из **другого** simulation corpus; не переносить счётчики 55/270 туда.
- [Соседний read-only planning index](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/00-README.md): ранее отмечал teacher/DPO/data/consumer ошибки; новые N50 — именно измеренные свойства **указанных исторических JSONL**. Абсолютная новизна относительно сотен документов без полного полнотекстового поиска **не заявляется**.

## 9. Причинная карта и verdict

`source corpus → validation/lossless authoring → legal admission → trainer pair tensors → DPO loss → learned checkpoint → actual consumer → blind semantic accuracy`.

На сегодня **VERIFIED** только левый участок с аномалиями. `admitted pair IDs / Steps`, фактический `Student` и качество конечного ответа для данного корпуса — **NOT_TRACED / NOT_RUN**. До их восстановления N50 — сильный дефект создания/учёта данных, но **не установленная root cause RP01**. Исполняемые задания и отрицательные контроли: [N50 causal mini-experiments](50-EXTERNAL-DOMAINS-CAUSAL-EXPERIMENTS-2026-10-19.md).
