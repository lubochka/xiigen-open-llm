# N49-01 и N49-03: обучение потеряло содержимое пары до LoRA

**Статус:** old/current SOURCE CONFIRMED; модельно-видимая проекция на исторических 50+50 — выполненный аналитический расчёт; `.NET` native retraining / quality impact — **NOT_RUN**. Это не исправление production source.

## N49-01 — Wiki Request Router: source-conditioned DPO стал defect-type-conditioned DPO

| Измерение | Донор `local_wiki_planning` | Текущий `codex/dpo-implementation` |
|---|---|---|
| Кто формирует DPO | `ProjectWikiRequestRoutingTrainingPipeline.ToDpoMetricPair` | Тот же метод |
| Вход для нейронной policy | Instruction + **`pair.Prompt`**, исходный запрос исполнителя | **`pair.RejectedType`** → строка `CODE_TRACE_DPO DEFECT_TYPE_x` без реального запроса |
| Preferred | **`pair.Chosen`**, в т.ч. target route/unit | Шаблон `PREFERRED_CODE_TRACE_OUTPUT` 12 раз + метки type |
| Rejected | **`pair.Rejected`**, конкретная неправильная маршрутизация | Шаблон `REJECTED_CODE_TRACE_OUTPUT` 12 раз + метки type |
| `ExpectedRouteKind` | В оригинальном chosen, а также обучающем примере структурного классификатора | В нейронном `ToDpoMetricPair` вообще не читается |
| Какой gate смотрит получателя | Структурный `TrainAndEvaluateAsync` и отдельный neural DPO metric runner | Структурная ветка всё ещё видит реальные пары, neural DPO видит только surrogate |

**Первичные кодовые доказательства:**

- Донор `ProjectWikiRequestRoutingTrainingPipeline.cs:109-116`: [exact prompt + original chosen/rejected](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L109-L116).
- Текущая версия `:59-73`: [создание отдельного `-signal-token` корпуса для DPO](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L59-L73).
- Текущая версия `:131-168`: [практически полная замена учебной пары synthetic tokens по rejectedType](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L131-L168).
- Генератор CURRENT `:27-42`: [создаёт исходные Prompt/Chosen/Rejected/ExpectedRouteKind](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingDpoGenerator.cs#L27-L42). То есть информация утрачена **при проекции**, не обязательно при авторстве исходных пар.
- CURRENT структурный runner `:42-65`: [обучает свой perceptron по Prompt/ExpectedRouteKind](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L42-L65). Его accuracy и neural DPO относятся **к разным обучаемым recipients**.

Обучение из `RejectedType` само по себе может быть полезным для **детектора классов ошибок**, но оно **не эквивалентно** изучению маршрута для новой фразы. Маркер `CODE_TRACE_DPO` в router DPO свидетельствует о копировании признаков другого домена; это source fact, но его численный эффект ещё не измерен.

### Проверенная проекция исторических 50+50 строк — самое дешёвое дискриминирующее свидетельство

Прочитан **первичный** JSON старого опыта `2026-05-20-...-043`, включая реальные `trainingBatch.pairs` и `heldOutBatch.pairs`. Прямой подсчёт в исследовательской сессии: в каждом наборе **50 разных pairId и 50 разных prompt**, пересечение prompt между наборами **0**. Актуальный `ToDpoMetricPair` формирует все три модельно-видимые строки как функции **одного поля `RejectedType`**, кроме отдельного идентификатора PairId, который не является текстом policy.

| RejectedType | Train (из 50) | Heldout (из 50) | Разных правильных target route внутри train |
|---|---:|---:|---:|
| `route_to_static_script` | 5 | 5 | 1 |
| `wrong_training_unit` | **29** | **29** | **6** |
| `route_validation_to_answer` | 8 | 8 | 1 |
| `route_non_wiki_to_project_wiki` | 8 | 8 | 1 |
| **Итого** | **50** | **50** | **4 эквивалентных модельных DPO-входа** на набор |

Из 50 train строк **46 являются повторениями уже встреченной модельно-видимой тройки** по этому текущему projection; среди 50 heldout то же самое. Все **4** группы projected train/heldout пересекаются, несмотря на 0 дословных пересечений исходных prompt. Особо важно, что группа `wrong_training_unit` объединяет **29 различных учебных запросов с шестью разными целевыми маршрутами**, так что current DPO не может научиться различать эти target labels через одну эту тройку.

**Сила доказательства:** это не `dotnet test` и не новое обучение. Выполнен read-only source-equivalent пересчёт **старой реальной выборки, проецированной новым методом**. Непосредственный текущий `metricTrain` создаётся генератором с иными batchId `-signal-token`; его фактические counts/overlap этим арифметическим приёмом **НЕ доказаны**. Не утверждать «нынешний DPO run имел точно четыре уникальные строки», пока не выгружен current metric batch. Отдельный `pairId` отличается; он не восстанавливает потерянную обусловленность prompt.

**Исторический receipt честно:** runner043 сообщал structural route accuracy=**1.0** на 50 heldout и average confidence=**0.9337418**, но отдельный DPO heldout=**0.8799999952**, train accuracy=**0.6400000125**, final reward margin=**0.0050931**; `publication=null`, warning `request_routing_dpo_accuracy_below_0_92_target_publication_skipped`. Значит в доноре найдено **работающее обучение структурного route recipient** на ограниченном наборе, **не доказанный neural-publication или free-answer success**. Архив мог быть создан на историческом коде, отличающемся от HEAD донора: branch HEAD не аттестует бинарник старого run.

Архив: [полный JSON run043](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json). Воспроизводящий source-equivalent скрипт для того же JSON: [projection-probe.mjs](projection-probe.mjs).

### Критерий опровержения / ограничения

- **Опровергнуть loss of inputs** можно только показав другой active neural trainer path, который для того же intended recipient использует реальные prompt и chosen/rejected, и что обычный consumer использует именно его checkpoint. Просто наличие исходных полей у `DpoPair` ничего не опровергает.
- **Опровергнуть предположение о вреде обучению**: matched native train с новыми source-conditioned парами не увеличивает independent route accuracy/semantic recall/near-transfer относительно type-only при одинаковом budget и новом restored consumer. Тогда сохраняем source-различие, но снимаем quality priority.
- Если продукт отдельно обучает **defect classifier**, current type-only схема может быть допустима для **другого** `ILearningUnit`; нельзя маркировать её как полноценное обучение request-routing без отдельной связи.

## N49-03 — CodeTraceBuilder: пара C# тоже заменена типом дефекта

Донор [`CodeTraceBuilderTrainingPipeline.cs:119-120`](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L119-L120) вызывает `new(pair.PairId, pair.A, pair.Chosen, pair.Rejected)`. В current [`:124-129`](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L124-L129) передаются `BuildMetricPrompt(pair.RejectedType), BuildChosenSignal(...), BuildRejectedSignal(...)`. В [`:152-210`](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L152-L210) это снова fixed `CODE_TRACE_DPO`, DEFECT_TYPE, повторяемые preferred/rejected tokens и правила `missing_callee`, `wrong_caller`, `incomplete_trace` и др. **Конкретный исходный C# и ожидаемая трасса в модельной тройке не используются.**

Для C# это особенно опасно: два метода могут иметь одинаковый rejected type `missing_callee`, но разные правильные цели (например `SaveAsync→Commit` против `LoadAsync→Read`). Type-only DPO видит эквивалентную задачу и одинаковый chosen; original DPO видит различие. Одновременно hard-coded признаки могут дать хороший результат классификатору дефектов, не улучшая извлечение реальной трассы.

**Предметные проверки:** 2–4 разных метода из разных файлов с одним defect type, независимый `gold` вызовов и возвращаемых значений, inspect DPO bytes → adapter gradients → обычный `CodeTraceBuilder` consumer. Исторические structural recall=1, построенные тем же extractor, нельзя выдавать за независимое подтверждение нейронного улучшения.

## Контроль области: перенос не универсален

Для сравнения прочитаны old/current прямые DPO bridges:
- `ProjectWikiAnswerComposerTrainingPipeline`: `pair.Chosen` и `pair.Rejected` **ещё передаются**.
- `WikiContextRoutingTrainingPipeline`: `new(pair.PairId, pair.A, pair.Chosen, pair.Rejected)` **ещё передаётся**.
- `ProjectWikiQueryPlanningTrainingPipeline`: выбранный/отклонённый текст **ещё передаётся**.

То есть гипотеза адресная для двух найденных recipients, не «все pipelines сломаны». В уже известных [WikiTrace donor/проекция](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md) ранее исследовалась **другая** `WikiTraceAggregatorTrainingPipeline`. Общий класс «потеряли контекст» известен; **конкретные old/new sites N49-01/N49-03** — отдельное новое зафиксированное сравнение. Не приписывать novelty всей идее.
