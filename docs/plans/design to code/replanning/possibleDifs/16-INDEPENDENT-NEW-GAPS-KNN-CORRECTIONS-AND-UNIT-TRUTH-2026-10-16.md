# 16 · Независимый source audit: поздняя коррекция KNN, настоящий эффект Unit и достоверность оценки

**16 октября 2026 · новый самостоятельный проход. Статус: DRAFT / SOURCE-VERIFIED-DIFFERENCES / RP01-CAUSAL-IMPACT-NOT-MEASURED.** Предмет — существующая программа LearningFirst RP01 против исторических обучаемых Unit/KNN. Это **не** повтор опыта, **не** новая публикация кода и **не** независимый CODEX/Astra review. Все ссылки на current core зафиксированы на `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` (ветка `codex/dpo-implementation`). Исторические ветки: `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`; `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`.

## 1. Существенное разграничение результата RP01 и прошлых успехов

В RP01 C#500 и EN→RU500 отчёты показывают 500 DPO-пар / 50 optimizer updates на домен, изменённые LoRA-матрицы и **отсутствие улучшения свободных ответов** (C# 0/3→0/3, EN→RU 0/4→0/4). Источник: [RP01 Learning Recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

Это не тот же путь, что прежние `TranslationMachine.TrainStructuredBatchAsync → ITranslationUnitTrainer.TrainOnExample → TranslationKnnStore / исправляющие карты → TrainedTranslationAdapter → ordinary translation`. Структурные E09/E34 и E51A/B; реальные исторические DPO V191/V194/repeat10 — разные объекты обучения и разные критерии успешности. Отдельно читать [исторический DPO и ветки](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [ложные предпосылки E07/E09](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [исторический E51](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md). Нельзя выдать достижение train accuracy, проверку того же исходного кода или oracle-filled Flow за unseen улучшение Student.

Требование из [«Алгоритм создания ДПО»](../../Алгоритм%20создания%20ДПО.md): typed Unit выдаёт many→many lawful alternatives, Flow изучает коррекции отношений, DynamicGraph — композиции высшего порядка, а DPO/Student и эти recipients должны быть связаны **проверяемым фактическим маршрутом**. Наличие отдельных реализаций ≠ их применение RP01.

## 2. NEW-16.01 · P0 для восстановленного KNN пути — **инверсия приоритета позднего урока**

**Подтверждённое противоречие именно в кодовом контракте; качество и связь с RP01 не измерены.**

- [TranslationUnitContracts.cs:202–217](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L202-L217): `TranslationKnnStore.AddPair` делает `bucket.Add((sourceText,targetText))`. Соседний комментарий прямо утверждает, что поздние пары имеют больший DPO-preference signal; хранение численного signal/version в bucket отсутствует.
- [TranslationUnitContracts.cs:219–238](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L219-L238): `FindNearest` для exact signature возвращает `exact.Take(k)` — **самые ранние**, не поздние.
- [TranslationUnitContracts.cs:241–292](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L241-L292): `FindNearestCascade` фильтрует по источнику/законам и затем `Take(k)`, без сортировки по новизне, принятому quality или correction priority.
- [TranslationTrainedUnits.cs:333–361](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L361): `ExecutorUnit` вызывает cascade с `k:5` и выбирает **первый** допустимый перевод.
- [TranslationUnitContracts.cs:328–351](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L328-L351): экспорт и import KNN-store сохраняют порядок пар внутри bucket: устаревшая выдача переживает restart.

**Механизм:** пять старых предложений с одной подписью, которые допускает источник/закон, занимают `Take(5)`; шестое новое корректирующее предложение не участвует в inference даже после успешного `TrainOnExample`. Обратный порядок вставки меняет output **при одинаковом множестве training rows**. `_exactSourceMap` может скрыть это на буквальном повторе: [Executor.Execute:263–291](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L291) сперва проверяет exact/phrase. Поэтому проверять отдельно **direct KNN store** (механизм) и **новые перефразы без exact/phrase попадания** (consumer). Нельзя априори объявлять всю EN→RU трассу неисправной.

**Фальсификатор:** при 5 старых допустимых + 1 новом корректирующем `FindNearestCascade` возвращает новый; или CODEX-трасса обычного consumer получает его через иной route — тогда влияние на этот route не доказано. Готовый опыт: [16-MICRO](16-MICRO-CAUSAL-EXPERIMENT-KNN-PRIORITY-AND-UNIT-STATE.md).

## 3. NEW-16.02 · P1 — общая память для разных target languages плюс cap до проверки направления

[TranslationMachine.CreateRuntime:714–744](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L714-L744) передаёт **один** `TranslationKnnStore` нескольким `ExecutorUnit` — RU_EN/EN_RU/HE_EN/EN_HE/ES_EN/EN_ES. Ключ в [TranslationUnitContracts.cs:202–216](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L202-L216) — структурная подпись без source/target language и `topic_domain`. Источник проверяется **до** `Take(k)`, но целевой язык проверяет только [Executor.TrySelectKnnCandidate:346–361](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L346-L361) **после** извлечения k кандидатов.

**Риск:** 5 допустимых по source примеров на другом target language вытесняют нужный шестой; позднее `IsTargetCompatibleForDirection` отвергает первые, но потерянный шестой уже не виден. Это hypothesis of downstream quality impact, не доказательство, что фильтр отсутствует у других consumer. Процедура проверки: 2 направления; сначала direct store с перемешанными target, затем вызываемый flow с замороженными admissible laws и без exactSourceMap совпадения; A/B — pre-search partition по направлению vs current. Замерять correct/invalid/abstain/coverage и чужоязычные ответы, а не совпадение top1 IDs.

## 4. NEW-16.03 · P1 — no-op Unit превращается в «обученную строку» и «promoted Unit»

[TranslationMachine.TrainStructuredBatchAsync:103–145](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L103-L145):
- неизвестный `UnitId` пропускается (`continue`), но `RowsTrained` считает **все** `!HeldOut` входы; числовой счётчик может расти без mutation;
- известный `trainer.TrainOnExample(example)` возвращает `void`, после чего `trainedUnits.Add(example.UnitId)` и identity history пишутся **без проверки changed state**;
- [SemanticTokenizerUnit.TrainOnExample:123–140](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L123-L140) принимает JSON literal `null` и тихо возвращается без добавления correction;
- при отсутствии heldout `AccuracyAfter = MeasureUnitCoverage(trainedUnits)` ([TranslationMachine:674–684](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L674-L684)) — доля трёх именованных Unit, а **не точность решения**.

Ожидаемый статический контрпример: один `semantic-tokenizer` с `ChosenOutputJson="null"` и `HeldOut=false` → отсутствие изменения correction maps, но `RowsTrained=1`, promoted содержит этот Unit, `AccuracyAfter=1/3`. Ещё один неизвестный Unit → `RowsTrained=1`, promoted пуст. Это несовместимость **accounting ↔ фактический обученный state**, не доказательство неправильного DPO Student training. Переиспользовать valid+no-op negative controls в миниопыте.

## 5. NEW-16.04 · P1 — одно ошибочное batch стирает флаг «имеется структурное обучение», а не знания

[TranslationMachine:124–130](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L124-L130) присваивает `_hasStructuredTraining = trainedUnits.Count > 0` после **каждого** batch; это не накопительный атрибут. [EvaluateAsync:162–200](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L162-L200) вообще не делает scoring inference, если флаг false. Сначала настоящий valid batch, затем batch с unknown UnitId: содержимое карт остаётся, индикатор true→false, `EvaluateAsync` сообщает `held_out_accuracy_measured=false` и `accuracy=0`. Это **искусственная регрессия оценки**, а не исчезновение навыка. Отличать изменение scalar/history от сохранности unit state.

## 6. NEW-16.05 · P1 — evaluator всегда RU→EN, даже для EN→RU

[TranslationMachine.TranslateForEvaluationAsync:686–705](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L686-L705) задаёт metadata `source_language="ru"`, `target_language="en"` и `topic_domain="general"` для **каждого** EvaluationItem. [TranslationRoutingFlow:65–78](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationRoutingFlow.cs#L65-L78) реально выбирает adapter на основании языков. Если этот public `EvaluateAsync` используют для EN→RU/HE→EN, он измеряет **другое** направление. Контроль: выполнить то же cold loaded model и те же заранее замороженные prompts через route с явной правильной парой языков; сравнить actual direction trace и ответы. Важно: RP01 `Rp01SimulationDpo` применяет свой generator evaluator; его известный EN→RU 0/4 **нельзя** объяснить этим кодом без call trace.

## 7. NEW-16.06 · P2 — macro average по Unit может противоречить пользовательской точности

[TranslationMachine.MeasureHeldOutAccuracy:662–671](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L662-L671) группирует heldout по `UnitId` и считает простое среднее `Accuracy(unit)`. Это осмысленная **macro метрика**, но `AccuracyAfter` ([строки 124–127](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L124-L127)) не говорит, что это macro. Условный контроль: unit-A 1/1 и unit-B 0/99 → macro=0.5 против micro=0.01. Это не доказательство ухудшения, а риск ложной интерпретации обучения. Публиковать обе метрики и поддержку n per Unit; acceptance по end-user tasks.

## 8. NEW-16.07 · P2 — shared root vocab всё ещё order-dependent при ограничении первых 32

[TrainingVectorBuilder:54–61](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L54-L61): `SelectMany(NormalizeRoots).Distinct().Take(32).Select((root,index))`. Даже при одном shared vocab перестановка **те же 33+ distinct roots** меняет отобранные корни/координаты. [BuildVector:77–101](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L77-L101) кладёт их в positional slots. Это **не дубль DIF-03**: тот описывает *отдельный словарь на каждый запрос*, этот — нестабильность **общего** словаря по порядку поступления / capacity. Проверка: те же 33+ roots, два порядка, checksum vocab/vector и fixed-query kNN outcome; повторить с версионированным замороженным vocab, одновременно re-encode весь index. Без реального consumer-effect пометка `VECTOR_IDENTITY_SEAM_ONLY`. RP01 gradient получает иной, scalar-упрощённый vector signal: причина его 0/4 **не доказана**.

## 9. Дополнительный кандидат (OPEN) — HNSW adjacency может стать несимметричным

[HnswIndex.Add:74–89](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L74-L89) добавляет ребро A↔B, но при ограничении степени перезаписывает список соседей B и может удалить A без обратной правки. Направленные adjacency сами по себе не доказанный дефект HNSW; проверить reachability/recall@k на exact reference и фиксированной последовательности insert; не смешивать с уже описанным случайным `RandomLevel` (DIF-14.04). Никакой claim о причинном ухудшении без сравнимого поиска.

## 10. Матрица трассировки и запрет завышения статуса

| Новый ID | Кодовое расхождение | Минимальная переменная | Главное независимое измерение | Actual RP01 attribution |
| --- | --- | --- | --- | --- |
| NEW-16.01 | newer correction loses to `Take(k)` | same pool, inverse insertion / explicit lawful priority | heldout translations, correct-vs-stale | OPEN |
| NEW-16.02 | shared target-language memory, late filtering | pre-search direction partition only | wrong language / lawful coverage | OPEN |
| NEW-16.03 | counted/admitted no-op Unit | JSON null vs valid chosen / unknown ID | state delta vs RowsTrained | OPEN |
| NEW-16.04 | boolean resets after unrelated batch | one extra unknown-unit batch | evaluation status vs unchanged unit state | OPEN |
| NEW-16.05 | evaluator hardcodes RU→EN | explicit language pair only | direction trace, correct translations | UNRELATED unless caller proven |
| NEW-16.06 | macro-vs-micro accuracy | heldout group mix only | per-task micro and per-unit macro | OPEN |
| NEW-16.07 | first-32 shared roots | same set, input order only | vector identity, real answer | OPEN |
| NEW-16.08 | prune edge symmetry | graph insertion, fixed corpus | recall@k vs exhaustive | OPEN |

**Evidence tags:** CODE = verified source statement; CONTRADICTION = source contradicts same-code comment/contract; EXPECTED = predicted outcome of micro; H = hypothesized impact; NOT_RUN = no model/test execution in this audit. All eight are **additional to** [DIF01–44](01-PRIORITIZED-DIFFERENCES.md), [DIF-14.*](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [MICRO-15.*](15-FAST-CAUSAL-PROBES-FOR-NEW-GAPS.md), which must remain authoritative for their already-covered seams.

## 11. DPO creation, human role and evidentiary authority — nonnegotiable

Следовать [WorkLeft-02 R16:6–18](../../09.09.26WorkLeft-02-teaching-chinese-model.md):
1. **CODEX под GPT-6 Astra 6 UltraCode выполняет роль человека: выбирает/утверждает законный учебный случай.** Пользователь не должен вручную выполнять делегированную селекцию; сортировка kNN и программа-шаблон также не эквивалентны выбору CODEX.
2. **Разрешённая внешняя открытая китайская модель самостоятельно создаёт обе точные финальные стороны DPO — `chosen` и `rejected`.** Вызов надо доказать ID, model/license/bytes/manifest. Не переименовывать deterministic simulation или старую программу в teacher.
3. **CODEX под GPT-6 Astra 6 UltraCode отдельно проверяет качество конечной пары** (correct/lawful chosen, meaningful worse rejected, no TIE/BOTH_BAD/UNJUDGEABLE, provenance). Это **не** независимый end-user Student evaluator.
4. **Student/Trainer** получает только authorized exact pair и конкретный recipient; законная pair selection сама по себе **не** показывает, что KNN/Unit/LoRA изменились.
5. **Независимая проверка Student** сравнивает замороженные source/family-disjoint tasks после cold restore и ordinary consumer; журнал считать evidence только на выполненный этап.

Для дешёвого memory-unit reproducer с заранее заданными строками разрешены локальные синтетические fixtures без нового Teacher; **такой опыт не называется DPO training, не использует teacher authority и не доказывает улучшение DPO**. Ни frozen outcomes, ни runtime acceptance, ни source tests не переписывать ради PASS.

## 12. Порядок работы, исполнение, проверка и блокирующие условия

1. **CODEX executor, branch-local reproduction:** выполнить сначала [16-MICRO](16-MICRO-CAUSAL-EXPERIMENT-KNN-PRIORITY-AND-UNIT-STATE.md), только read/test в изолированном tmp, без изменения frozen RP01/existing outcomes. Первый диагностический опыт 6+1 memory записи / max 5 возвращаемых.
2. **Architect/code-review:** line-trace Train → state → serialize/reopen → ordinary Execute; назвать точный caller, нельзя заключать «исправлена модель» по одному helper.
3. **Plan-review:** потребовать predeclared cases, same lawful source, negative controls, paired right/wrong/abstain, source-disjoint small heldout; reject training score, count or hash-only improvement.
4. **Correction candidate:** не делать wholesale merge `language_translate`; исправлять только доказанный seam, параметризовать order/rank/contract, сохранить доменные законы и quarantine; DPO side-authority отдельна.
5. **Independent Astra 6 UltraCode review:** требуется отдельный реальный CODEX review exact proposed fix, ссылки на candidate commit SHA и три предметных контрпримера; **NOT_PERFORMED** этим документом.
6. **Promotion gate:** PASS только если A/B на **одной** зафиксированной панели показывает прирост end-user utility без нарушений; если тест лишь подтвердил сортировку или metadata discrepancy — `MECHANISM_CONFIRMED / QUALITY_OPEN`; если неизменный результат — `HYPOTHESIS_WEAKENED`. Никакого Windows GitHub-run марафона вместо проверки качества.

Использованные [архитектурные и review skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills): `human-readable-architecture-audit`, `phase-planning`, `plan-review`, `generated-code-review`, `code-examination`, `prompt-patch-authoring`, `experiment-design` и `conversation-intent-and-self-reflection`. Произведён source-only manual audit, **не утверждается выполнение независимых агентов или native запуска**.

## 13. Репозиторная сверка с параллельно опубликованными агентами (не считать совпавшее открытие двумя разными дефектами)

При публикации/повторном чтении каталога появились дополнительные исследования на **том же pinned source SHA**. Не присваивать им новые root-cause номера только ради счётчика:

| Моя запись | Параллельная запись | Решение о дублировании / остающаяся delta |
| --- | --- | --- |
| NEW-16.01 FIFO recent correction | [16 KNN correction visibility / N16-KNN-01](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md) | **ONE SAME CODE SEAM**, независимое совпадение. Этот документ добавляет durable checkpoint order и отдельную фазу C1/C2; micro-run всё ещё NOT_RUN. |
| NEW-16.02 shared directions | [16 KNN / N16-KNN-03](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md) | Частичный повтор. **Отдельная delta**: cap `Take(k)` выполняется раньше `IsTargetCompatibleForDirection`, поэтому чужие по target кандидаты могут заполнить shortlist до нужного шестого. Нужен same-source/different-target fuzzer. |
| NEW-16.03 unknown UnitId counted | [17 silent dispatch / N17-A](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) | **ONE SAME CODE SEAM** для `TryGetValue→continue` и `RowsTrained`; нельзя считать двумя discoveries. **Отдельная delta**: *известный* UnitId с `ChosenOutputJson="null"` возвращает `void` no-op, но сохраняет `PromotedUnitIds` и 1/3 coverage как `AccuracyAfter`. |
| NEW-16.04 | явного аналогичного описания в прочитанном [17 silent dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) нет | **ДОПОЛНИТЕЛЬНАЯ** проблема: `_hasStructuredTraining` сбрасывается следующим unknown-only batch, `EvaluateAsync` перестаёт измерять сохранённую память. |
| NEW-16.05 | не найдено аналогичного в двух пересекающихся KNN/Unit документах | **ДОПОЛНИТЕЛЬНАЯ** проблема: evaluator жёстко выбирает RU→EN. Не приписывать текущему RP01 offline evaluator. |
| NEW-16.06 | не найдено в двух пересекающихся документах | **ДОПОЛНИТЕЛЬНЫЙ** macro-vs-micro accounting seam, не обучающий градиент. |
| NEW-16.07 | относится к существующему DIF-03, но не совпадает с ним | **ДОПОЛНИТЕЛЬНАЯ** перестановочная нестабильность *общего* словаря после cutoff32, отдельно от per-prompt vocab. |
| NEW-16.08 | уже есть HNSW RNG DIF-14.04, но не pruning asymmetry | **НОВЫЙ КАНДИДАТ**, низкий приоритет и без доказанного качества. |

Не потерять **новые исследования других авторов**, которые здесь не переоткрываются:
- [16 translation DPO bridge token erasure](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md) и [model-visible pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md): разные literal chosen/rejected могут стать неразличимыми для legacy bridge; **это не current RP01 exact-v2**.
- [16 live encoder/index drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md): изменение live embeddings без перестройки KNN index и 64-token cutoff.
- [16 tier balance / zero weight / KV cache](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md): energy balance text-vs-law, zero DPO weight, full-vs-cache parity.
- [17 historical E13 train-boundary](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md): исторические PASSED без сопоставимого heldout.
- [18 ProductHost route bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md): обычный GenerateAsync не обязательно использует learned KNN state — **перед repair извлечь точный customer path**.

**Реестр именно новых причин**, а не количества файлов: FIFO = 1 confirmed seam, unknown-unit accounting = 1 confirmed seam. Приоритет нового покрытия для следующих CODEX runs — *known Unit no-op*, *trained-flag reset*, *RU→EN-only evaluator*, *target-filter-after-cap*, *first32 shared vocab*. Все обоснованы source; **ни одно пока не подтверждено причиной деградации neural RP01**. Canonical roles автора/выбора/судьи остаются в `11` этого документа.

## 14. Дополнительный **actual cross-branch code comparison**: многие seams — исторически наследуемые, а не новые регрессии RP01

Чтобы не создавать ложную историю «рабочий алгоритм потерял новейшую функцию» был напрямую открыт `llm_mvp_core/DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548` наряду с current:

| Файл и механизм | Historical DesignToCode | Current codex/dpo-implementation | Статус именно *исторического изменения* |
| --- | --- | --- | --- |
| `27.TranslationDomain/TranslationUnitContracts.cs` / FIFO KNN | [old AddPair и exact.Take(k)](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationUnitContracts.cs#L201-L230), [old cascade](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationUnitContracts.cs#L236-L282) **уже имеют append и Take(k)** | [current AddPair/Take(k)](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L202-L292) | **LEGACY LATENT DEFECT, НЕ newly introduced regression.** Может проявиться после накопления `>k` исправлений, но не объясняет прежний успех vs RP01 одним изменением source-файла. |
| `27.TranslationDomain/TranslationMachine.cs` / trained flag, evaluator | [old TrainStructuredBatch](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationMachine.cs#L86-L139): flag assigned per batch; [old routing/evaluation](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationMachine.cs#L507-L542) | [current flag](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L124-L130), [current hard-coded evaluator](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L686-L705) | Существенные **аналогичные конструкции уже были** в исторической ветке; это evaluator/metric weakness и coverage-limited behavior, не показанная newly introduced RP01 regression. |
| `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs` / shared root first 32 | [old BuildRootVocabulary](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L49-L57) использует `Distinct().Take(RootSlotCount)` | [current first32](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L54-L61) | **LEGACY LATENT ENCODER LIMIT**, не source regression. Изменённые corpus order/capacity or recipient call-path — пока гипотезы. |

Ветка `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` содержит `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs` со **старым** first32, но три exact path `27.TranslationDomain/Translation*.cs` **не найдены по этим именам** (404); из этого нельзя заключать, что branch не имела никакой переводческой памяти: вероятны иные файлы / структура. Чтобы доказать буквальный matrix→KNN transfer, нужны commit parent, caller и same-family AB, а не только похожие термины.

**Пересмотр причинной гипотезы:** NEW-16.01 и NEW-16.07 не доказаны как «утрачены при переносе». Они могут быть **давней неисправностью, ранее замаскированной малым количеством записей, точным совпадением в _exactSourceMap или иной системой оценки**. Микроопыт обязан воспроизвести threshold `k=5 → 6` и проверить использование ordinary consumer при росте банка. Отдельный основной кандидат для непосредственного RP01 остаётся documented *recipient mismatch*: random B0 q/v LoRA-only не имеет автоматически свойства learned translation Unit/KNN. Для доказательства изменения качества нужен matched causal experiment, не статическое сравнение двух branches.
