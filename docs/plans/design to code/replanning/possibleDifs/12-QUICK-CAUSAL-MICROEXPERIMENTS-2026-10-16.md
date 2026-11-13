# 12 — Быстрые причинные микроэксперименты: действительно ли найденные различия ухудшают обучение?

**Дата:** 2026-10-16 · **Статус:** DRAFT / WORK ORDER, **NOT RUN** · **Основной приоритет:** MX-01 · **Репозитории:** `xiigen-planning/llm_plan3`, `llm_mvp_core/codex/dpo-implementation` на источнике [83590a462b2f8aedc9d3ab4cb775193eb9c25ce5](https://github.com/lubochka/llm_mvp_core/commit/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5).

Связанная находка: [потеря ранее работающего trainable Unit/hybrid path](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [исходные успешные E51A/B/E52/P3](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md), [прямой кодовый аудит DIF-10.01–10.14](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md). Эта карточка **не является ещё одной фазой перепланирования или разрешением нового 500-batch**.

## 0. Когда можно сказать «различие влияет на качество»?

Достаточны четыре связи на **одном реальном consumer**: `identical parent/data split -> one allowed change -> actual trained state -> same frozen heldout outputs -> independent semantic scoring`. Наличие нового файла, уменьшение loss, повышение training margin, рост «accuracy» в метаданных или улучшение только на точных TRAIN-фразах недостаточны.

**Три разных вывода:**
- `MECHANISM_PRESENT`: код и trace показывают, что этот state реально используется. Это **не** прирост качества.
- `LOCAL_CAUSAL_SIGNAL`: при зафиксированных остальных факторах после **одной** учебной мутации ответы на ранее не обучавшихся источниках улучшились, а без включённого consumer этого изменения нет. Это **не** доказательство общей многодоменной генерализации.
- `QUALITY_GAIN_REPLICATED`: отдельная, заранее замороженная **вторая** независимая по источникам/семействам панель воспроизвела направленный gain без скрытого replay, source overlap, нового scorer или смены бюджета. Только после этого можно уверенно говорить о практическом влиянии для ограниченного домена.

Везде сравнивать **правильность смысла**, `critical_error`, `partial`, `abstain`, покрытие, нагрузку и provenance. **Качественная оценка выполняется CODEX с `astra 6 ultracode`; человеческую функцию выбора учебного случая выполняет также CODEX с `astra 6 ultracode`. Разрешённая открытая китайская внешняя модель создаёт обе финальные стороны DPO (chosen/rejected).** В эксперименте чисто дискретного Unit можно повторно использовать только разрешённые **ещё не обучавшие** train-ready пары с их **реальным** авторством, не называя их вызовом китайской модели или завершённым DPO; для проверки именно полного target teacher-контура требуются actual model/provider receipts, CODEX judge, права и точное авторство. `simulation` остаётся `simulation`, TIE/BOTH_BAD/UNJUDGEABLE не становится ложной предпочтительной парой.

## MX-01 — первый опыт: пропущенный обучаемый translation Unit/KNN действительно даёт пользу?

### Гипотеза и один фактор

**H:** часть качества майских E51/E52 и прежних переводческих циклов происходила из **настоящего обучения Unit-модели и сохранённой retrieval/correction memory**. RP01 проверил нейронную DPO/LoRA-модель, но не этот Unit-consumer. Разница может объяснять, почему исторически работавшие алгоритмы не перенеслись в RP01. **Это гипотеза о пропущенном обучаемом маршруте, не о «сломанных весах Student».**

**Меняем только:** состояние **одного** существующего `ExecutorUnit` EN→RU, вызвав `TranslationMachine.TrainStructuredBatchAsync` один раз. **Не меняем:** Student B0/C100, декодер, словарь, остальные Unit, параметры KNN, правила оценки, heldout, схему Flow, scheduler, routing, исходные checkpoints. Опыт намеренно сначала проверяет отдельно **discrete Unit learning**, а не DPO loss.

**Код, уже готовый к переиспользованию:**
- [TranslationTrainedUnits.cs:254–287](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L254-L287): общая обучаемая память примеров для Executor inference.
- [TranslationTrainedUnits.cs:403–456](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L456): TrainOnExample применяет chosen к exact source / KNN / word/phrase mappings.
- [TranslationMachine.cs:29–42,89–142](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L89-L142): единый training owner, реально dispatch по UnitId; **не** legacy `TrainBatchAsync` со скалярным `+0.01`.
- [TranslationStructuredBatchBuilder.cs:65–149](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L65-L149): `BuildGeneralBatch` поддерживает `EN_RU`, каждый pair преобразует в semantic/executor/verifier examples. Чтобы тестировать **один** Unit, выбрать только `UnitId == TranslationUnitIds.ExecutorFor(TranslationDirection.EN_RU)`; остальные исключить **до TrainStructuredBatchAsync**.
- [TranslationBatch029Runner.cs:155–215](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L155-L215): готовый образец `GetOrchestratorAsync → LogicalOrchestratorRequest → RunAsync` и source-disjoint проверки. Не копировать его RU_EN-only builder для EN_RU.

### Подготовка данных: одна маленькая, не игрушечная смысловая задача

- **TRAIN:** 6 новых, законно доступных EN→RU пар; по 2 на три предметных закона: **отрицание/модальность**, **число/порог**, **кто совершил действие и над кем**. Формат `TranslationTrainReadyPair` (`direction="EN_RU"`, `validation_outcome="train_ready"`, непустые exact chosen/rejected, `pair_family`, истинный `source`/provenance). Для target DPO final sides пишет китайская разрешённая модель и допускает отдельный CODEX Astra UltraCode judge; если используется историческое легальное synthetic source для unit-mechanism-only опыта, отражать настоящий источник и НЕ claim target teacher compliance. **6 pairs → 6 executor-unit examples** после фильтра, НЕ «18 разных уроков».
- **HELDOUT:** 8 совершенно новых EN→RU задач: 2 по каждому закону с **другими source templates / named entities / relation structures** + 2 law-negative/unknown/ambiguous случая с ожидаемым допустимым abstain. Зафиксировать before-training их ID, исходный текст, law clauses и независимую рубрику. Не допускать точного или близкого (>порог, выбранный до просмотра результатов) source overlap.
- **Иллюстрация, НЕ обучающие данные:** `Do not delete more than five rows` (сохранить НЕ и «больше пяти»), `Alice gave Bob a key` (сохранить инициатора/получателя). Запрещено считать простое присутствие слов качественным переводом; русская морфология, направление и порядок ролей оцениваются CODEX.

### Один проход / минимальный executor

1. Создать **один** новый `TranslationMachine()`. На 8 замороженных cases вызвать `GetOrchestratorAsync → RunAsync` с metadata `source_language=en,target_language=ru,topic_domain=general`. Сохранить сырой выход, route, version и latency (`BEFORE`).
2. Собрать через `TranslationStructuredBatchBuilder.BuildGeneralBatch` 6 train-ready пар с `signatureMode=Structural`; проверить train/heldout source-disjoint и фильтровать **только** executor UnitId. Никаких `HeldOut=true` в обучении.
3. Выполнить **один** `TrainStructuredBatchAsync(filteredBatch)`; сохранить фактические `RowsTrained`, `PromotedUnitIds`, export state hash/identity. `AccuracyAfter` из результата **НЕ** использовать как целевую метрику.
4. **Тот же экземпляр машины**, та же схема запроса: повторить ровно те же 8 heldout (`AFTER`). Снять actual response and trace of learned example/KNN versus fallback. Обучение изменило только Unit state, не scorer и не prompt.
5. Оценить `BEFORE` и `AFTER` вслепую по неизменным критериям независимым CODEX/Astra UltraCode. **Основная метрика** `Q = #полностью семантически корректных / 8`, плюс `net_wins = improvements - regressions`, `critical_errors`, `partial`, `abstain`, `coverage`, full inference time/RAM. Если прошло — на **втором новом** tiny heldout подтвердить; только тогда проверить `ExportCheckpoint → ImportCheckpoint` на свежем объекте и ordinary route identity.
6. **Дополнительный безобучающий диагностический readout:** теми же 8 входами измерить уже сохранённый RP01 `C100` через [ResearchCheckpointEnRuAdapter](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/ResearchCheckpointEnRuAdapter.cs), чтобы показать различие результатов/маршрутов. **Не считать этот несопоставимый исторически обученный Student третьим причинным treatment** и не менять его веса.

Псевдокод **по существующим API** (это не готовая команда CLI):

```csharp
var machine = new TranslationMachine();
var before = await EvaluateSameEightFrozenCases(machine); // only a small test helper
var all = TranslationStructuredBatchBuilder.BuildGeneralBatch(
    "mx01-new-unique-id", sixFreshEnRuPairs, heldOut: false,
    signatureMode: TranslationSignatureMode.Structural);
var executorId = TranslationUnitIds.ExecutorFor(TranslationDirection.EN_RU);
var onlyExecutor = new TranslationLearningBatch(
    all.BatchId, TranslationMachine.DomainTag,
    all.Examples.Where(x => x.UnitId == executorId).ToArray(), all.SourceIds);
var receipt = await machine.TrainStructuredBatchAsync(onlyExecutor);
if (!receipt.IsSuccess) { /* record actual refusal; no retry */ }
var after = await EvaluateSameEightFrozenCases(machine);
```

**Точка минимальной реализации:** маленький development-only runner **или** расширение существующего научного runner одним `before/after` readout, который вызывает эти фактические методы. Никакого нового trainer, DPO objective, API-производственного маршрута или Windows workflow. До публикации кода сверить constructor signatures, exact input validation и безопасное расположение новой временной команды. Имя `EvaluateSameEightFrozenCases` — псевдоним *нового крошечного helper*, **не существующий метод**.

### Заранее записанная интерпретация результата

| Наблюдение | Вердикт | Что реально доказывает / следующий шаг |
|---|---|---|
| На 8 **новых** случаях как минимум **2 net correct wins**, без regressions и новых critical law errors, с подтверждённым route в обновлённый ExecutorUnit | `LOCAL_CAUSAL_SIGNAL` | Обучаемая память/словарь Unit **может** улучшать отдельные ответы. Нужно второе source-disjoint подтверждение; нельзя объявить это доказанной причиной провала Transformer RP01. |
| Мутировал state, но output/semantic Q на 8 cases не изменился | `NO_GAIN_ON_PANEL` | Возможны fallback/нет consumer reachability, нерелевантные neighbors, недостаточное обучение или ceiling. Не объявлять Unit «бесполезным» без трассы. |
| Улучшились только **точные учебные фразы**, не независимые families | `MEMORIZATION_ONLY` | Реальное обучение памяти, **нет** доказательства обобщения. |
| Нет actual Unit update / выбрался static fallback / пустой input / неверный формат | `INVALID_EXPERIMENT` | Сначала исправить *one* missing seam/fixture, не объявлять гипотезу ложной. |
| AFTER хуже или появляются нарушения отрицания/ролей | `REGRESSION` | Записать вред и остановить promotion. |
| Второй независимый малый heldout тоже улучшился при том же changed Unit и frozen scorer | `REPLICATED_LOCAL_QUALITY_GAIN` | Разрешён локальный вывод для **этого** Unit/domain, не для всего RP01/Graph/LoRA. |

**Ограничение масштаба:** **1 мутация Unit**, 6 train-pairs, 8 before + 8 after inference calls; 0 новых Student DPO optimizer steps, 0 повторов старых 500, 0 Windows/GitHub Actions. Время/память записывать по часам реального процесса; можно объявить конечный local wall cap, превышение = `INCONCLUSIVE`, но не гарантировать фиктивное количество минут. Если для опыта невозможно создать законный train-ready набор/загрузить реальную модель — `BLOCKED_INPUT` с точной причиной, не симулировать чужой provider.

**Дополнительная проверка единственного фактора:** `TrainStructuredBatchAsync` также меняет version/history/`_hasStructuredTraining`. По актуальному [GetOrchestratorAsync 653–662](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L653-L662) inference orchestrator использует тот же `_flow`, а не `_hasStructuredTraining`; всё же зафиксировать, что metadata не меняет выбранный outcome. Если оказывается, что она влияет, маркировать intervention как **bundled**, а не «один Unit фактор».

**Смежный опыт (не дублировать):** [MICRO-KNN-01](13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md) уже исследует **другое** отличие — использование top-3 вместо `hits[0]` при **одном и том же обученном** `KnnLogicalUnit`, на C# action labels. MX-01 исследует **до/после одного обучения translation ExecutorUnit**; результаты нельзя складывать в единый процент RP01.

**Критичная граница:** такой опыт проверяет вклад **обучаемого Unit**; чтобы утверждать, что именно **его отсутствие в RP01 consumer** создало разницу, после локального gain нужен отдельный **read-only toggle existing trained Unit ON/OFF при идентичном Student C100** через реальный комбинированный product route. Если такого bridge нет, зафиксировать **NO_CONSUMER_BRIDGE**, а не выдавать тест одного Unit за прямое RP01 quality experiment. Toggle не требует повторного обучения и даёт однофакторную причинную проверку интеграции.

## MX-02 — конкурирующая гипотеза: DPO работал на необученной B0 без языковой компетенции

**Факт:** [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/config/code-documentation/generation-v1.json) = B0 с 629-token BPE, 4 слоя, q/v LoRA rank8; [C#500 и ENRU500](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/tools/Rp01SimulationDpo/README.md#L19-L29) изменили adapters, но не свободные ответы. **Гипотеза:** нужен supervised next-token/response warmup до preference tuning.

1. **Read-only preflight и deterministic math:** точный exact-v2 route, prompt/response маски и likelihood, один finite-difference vs analytic gradient, `reference` immutable. Не тестировать известный legacy `Compact` как дефект RP01. Источники: [DpoExactPairMaterial](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/Models/DpoExactPairMaterial.cs#L64-L90), [StepVector](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L832-L935).
2. **Только одна новая учебная последовательность**, например 6–8 новых supervised chosen-only TRAIN примеров по одному простому C# закону: `B0 → маленький warmup → 8 независимых frozen free prompts`. Замораживать те же trainable/frozen keys, tokenizer, seed, sampling и exact prompt framing. Снять baseline/после: train response CE, token decisions, **полезный текст** `Q`, time/RAM. Использовать уже имеющийся [Trainer.TrainSupervisedNextTokenStep](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/04.TrainingLoop/Trainer.cs#L62-L149) через безопасный [ProductionSupervisedTrainingStepExecutor](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L10-L32). **Важный конфаундер:** эта SFT-функция в текущем виде обучает все next-token позиции (prompt+response), **не** response-only CE. Не называть её эквивалентом DPO-response objective; при необходимости добавить маленькую маску в существующий owner с отдельной проверкой.
3. **Только если warmup и gradient работают:** 8 **других** новых exact китайских DPO-pairs (без повторного использования или overlap с SFT/holdout) → 1–2 разрешённых оптимизаторских updates через существующий `StepVector`; сравнить `B0`, `warm`, `warm+DPO` с одинаковым генератором и независимым CODEX/Astra assessor. **Это последовательный механизм-пробник, не полноценный clean DPO-only-vs-warm+DPO A/B.** Чтобы строго приписать разницу cold start, нужен отдельный сбалансированный, заранее зарегистрированный counterfactual A/B и допустимое соблюдение политики no-repeat — **не обучать две клонированные ветки одной и той же парой без отдельного разрешения**.
4. **Ограничение CLI:** [RP01 REC02](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC02-DPO-GRADIENT.md) уже фиксирует: текущий `Rp01SimulationDpo` поддерживает 1 calibration или 500 main, **не** произвольную `train 8 8`. Для tiny опыта сначала reuse существующего безопасного owner, иначе единственный узкий hash-pinned `diagnose-small` adapter; **не писать несуществующую команду как готовую**.
5. **Вердикты:** warmup улучшил train CE и frozen semantic Q → `WARMUP_HELPFUL_LOCAL`; train CE улучшилась без free gain → `MARGINS_NOT_ABILITY`; не меняется даже train CE при корректном gradient/permit → `CAPACITY_OR_ADMISSION_BLOCKED`; DPO после warmup улучшил Q относительно warm → `INCREMENTAL_PREFERENCE_SIGNAL`; DPO ухудшил Q → `NEGATIVE_TRANSFER`. Ни один маленький опыт не даёт production READY и не оправдывает перенос старых 0/3, 0/4.

**Не запускать MX-02 автоматически после MX-01.** Это другой учебный owner, другой causal question и бюджет; выбирать следующий по наблюдённому результату, не пытаться «сразу покрыть всё».

## Карточки других обнаруженных различий — один фактор, один короткий опыт

Каждая карточка является **самостоятельной опцией**, а не обязательным checklist перед первым MX-01. `Static` означает обнаружение структурного механизма; без `after-training Q` нельзя объявлять «влияние на обучение».

| DIF / причина | Самая маленькая проверка | Что было бы свидетельством именно качества | Ограничение |
|---|---|---|---|
| **DIF-10.02** качество DPO/teacher | 4–6 новых exact Chinese-authored пар: CODEX Astra UltraCode blind judgement + subject-law/compiler oracle; различение source-family | После **одного** разрешённого DPO update новые heldout cases улучшаются по этому закону | Просто 100% accepted pairs не означает improved Student |
| **DIF-10.03/04** semantic TeachingVector | Без тренировки взять два 60D вектора с одинаковым числом ненулевых слотов, переставить domain-law координаты; сравнить `VectorSignal` | Для claim quality нужен **новый learnable semantic recipient** и отдельное same-consumer quality A/B; одно равенство signal — только CODE-инвариантность | В RP01 StepVector координатные значения игнорируются; нельзя «чинить вектор» без downstream consumer |
| **DIF-10.05** LoRA capacity/placement | На одном сохранённом parent и тех же 8 prompts: adapter ON/OFF, same tokenizer/decoder, q/v Δlogits/top1 & semantic Q; без новых updates | Если adapter ON превращает ошибки в верные ответы — этот trained adapter полезен на данном panel | Нет разницы не доказывает, что LoRA не работает; rank/target test — отдельный expensive factor |
| **DIF-10.06** token framing | Compare actual exact prompt IDs, prefix+response masks и first decision при одинаковых bytes; 2 коротких prompts | Изменение free-answer Q после **однофакторного** исправления framing, подтверждённое actual chosen logP | Простой `roundtrip=true` не quality improvement |
| **DIF-10.07** EOS | Простая `TextGenerator.Generate` vs verified `GenerateDetailed(...,null)` на 2 case где last merge token может встретиться; без обучения | Stop-reason/no-premature-stop и Q отличаются при одинаковых logits | Только simple Generate; **не** прямой RP01 cause |
| **DIF-10.08/09** KNN hash/top1 | Один frozen trained index, exact-vs-ANN recall; fixed hits top1 vs законный reranker; 8 semantic near misses | Новые law-safe correct answers при неизменном pool, с trace выбора нужного соседа | Это retrieval/inference; learned ranking требует отдельного update/signal |
| **DIF-10.10** fake accuracy | На fresh `TranslationMachine` один вызов legacy TrainBatchAsync: сравнить real state/output до/после и reported +0.01 | Нет улучшения Q при изменении accuracy → доказан surrogate на этом API, но не деградация RP01 | Обязательно проверить actual caller, иначе root-cause claim запрещён |
| **DIF-10.11** DPO→Unit conversion | Для одного admitted typed lesson посмотреть, какие chosen/rejected поля действительно мутируют chosen-map/negative-state; потом MX-01 after-learning readout | Доменный negative counterexample исправлен на unseen input, верный recipient и сохранённая пара | Простой copy chosen в dictionary не является нейронным DPO |
| **DIF-10.12** restore/adoption | Тот же frozen state и prompt: before-save, fresh-load через **полный** ExportCheckpoint/ImportCheckpoint, ordinary caller; compare outputs and used SHA | Качество после load совпадает с до load; если не совпадает, конкретный restore bug | `ExportAsync/ImportAsync` в TranslationMachine metadata-only; нельзя ими тестировать full restore |
| **DIF-10.13** Flow/Graph learning | Frozen Units, одна реальная learned correction edge on/off, 4 новых composition cases | Лучше unseen semantic order/association после edge update, обычный consumer реально вызван | `DynamicGraphMachine.TrainBatchAsync` сейчас `NOT_ACTIVATED` — пока нет learner, quality experiment неприменим |
| **DIF-10.14** RP01 consumer disconnected | Один научно улучшившийся candidate: cold load ID → research Generate → ordinary product Generate с теми же входами | Same trained state produces same improved answers в обоих consumers | Пока отсутствует качественный candidate, измеряется reachability, не прирост |
| **E09/E34 историческая валидность** | Пересчитать baseline и treatment на **одних и тех же val indices** со stable global labels, без переобучения | Только сопоставимый gain можно использовать как мотиватор переноса признака | Не проверяет ни DPO, ни RP01 Student |

## Ограниченные затраты, stop и доказательство

| Ресурс | MX-01 | MX-02 |
|---|---|---|
| Данные | 6 новых train пар + 8 новых frozen heldout | 6–8 distinct SFT; DPO позднее максимум 8 других; 8 новых heldout |
| Training | ровно 1 structured Unit training call; **0 Student optimizer calls** | сначала 1–3 коротких SFT updates; только если это полезно — 1–2 новых DPO updates |
| Inference | 8 BEFORE + 8 AFTER (+8 frozen C100 optional) | Same 8 free cases at B0 / warm / warm+DPO |
| Сохранность | frozen old500 and checkpoint untouched; new run ID | same; single writer, admitted new material, exact rollback |
| Код | маленький before/after harness и один tiny scope review | reuse existing SFT owner; optional bounded `diagnose-small`; **не** второй trainer |
| Стоп | frozen heldout leakage, отсутствующий consumer, update не состоялся, nonfinite, resource cap; `INCONCLUSIVE` | initial grad invalid ⇒ stop DPO, SFT train CE stagnant ⇒ stop extended training, resource cap |

**Обязательный независимый артефакт** (сохраняется как один JSON + компактный human report **после реального исполнения**, не создаётся с выдуманными значениями):

```json
{
  "experiment": "MX-01",
  "status": "NOT_RUN",
  "core_sha": null,
  "planning_sha": null,
  "actual_executor_model": null,
  "actual_chosen_rejected_author_model": null,
  "actual_pair_judge_model": null,
  "input_sha256": null,
  "train_pair_ids": [],
  "train_source_families": [],
  "heldout_family_ids": [],
  "trained_unit_id": null,
  "unit_state_before_sha256": null,
  "unit_state_after_sha256": null,
  "mutation_calls": null,
  "student_dpo_optimizer_updates": 0,
  "consumer_trace_before": null,
  "consumer_trace_after": null,
  "semantic_q_before": null,
  "semantic_q_after": null,
  "paired_wins": null,
  "paired_regressions": null,
  "new_critical_violations": null,
  "abstain_before_after": null,
  "owned_process_seconds": null,
  "owned_process_peak_ram_bytes": null,
  "falsifier": null,
  "independent_review": "NOT_RUN",
  "verdict": "NOT_RUN"
}
```

### Короткий исполнительский промпт (CODEX Astra 6 UltraCode)

> **CURRENT TASK:** провести **MX-01 и только MX-01** на code snapshot: одна разрешённая подготовка шести EN_RU source-disjoint pairs, один обучаемый ExecutorUnit, неизменный Flow/декодер/heldout, восемь paired actual outputs before/after, один честный CODEX Astra UltraCode semantic review. **NOT THE TASK:** 500 DPO, full Windows CI, переписывание всех Unit, создание нового trainer, claim production training. Read real skills: `architect-behavior-classifier`, `phase-planning`, `plan-review`, `generated-code-review`, `experiment-design`, `prompt-patch-authoring`, `human-readable-architecture-audit`, `learning-path-audit`. Use methods `TranslationStructuredBatchBuilder.BuildGeneralBatch`, `TranslationMachine.TrainStructuredBatchAsync`, `GetOrchestratorAsync`, `RunAsync`; executor-only filtering and source-disjoint heldout are mandatory. Before coding trace ownership, generation roles and exact source lines. Positive example: correct polarity, quantity and actor transfer to new English source; negative example: dictionary memorizes the exact TRAIN sentence while independent heldout remains wrong. Edge examples: unknown domain, zero neighbors, fallback route, source overlap, same output before/after, critical role inversion, saved state not adopted. **Output:** actual command only once implemented; source SHA, unique train families, tested law rubrics, true changed recipient, paired Q, coverage, provenance, PC costs, blockers and `LOCAL_CAUSAL_SIGNAL | NO_GAIN_ON_PANEL | MEMORIZATION_ONLY | REGRESSION | INVALID_EXPERIMENT | INCONCLUSIVE`. Code reviewer must separately read actual changed lines and compare receipts; no self-asserted PASS.

**SKILL / REVIEW NOTE:** изученные [architect](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills), [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [phase-planning](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/phase-planning-SKILL.md), [plan-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/generated-code-review-SKILL.md), [prompt-patch](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md), [human-readable](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md). Публикация этого плана — **только DRAFT**, без выполнения, independent Astra review и изменения модели; перед implementation сверить SHA, model authorization и actual route.
