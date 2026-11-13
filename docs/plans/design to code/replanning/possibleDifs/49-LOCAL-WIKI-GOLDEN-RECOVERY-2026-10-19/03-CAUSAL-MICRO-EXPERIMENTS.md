# 03 — Малые причинные эксперименты: сначала восстановить local_wiki_planning

**Статус:** `DESIGN_ONLY` — .NET native training and quality evaluation `NOT_RUN` на 19.10.2026. Эксперименты проектируются так, чтобы **одно отличие за раз** меняло обычный ответ, а не только статус теста. Счётчики и пороги ниже — *критерии принятия будущего результата, не заявленные результаты*. Фиксировать `code_sha, dataset_sha, tokenizer_sha, seed, model_id, unit_id, policy_sha, current_loaded_sha`; source-equivalent JS proxy может проверить лишь переключатели, не качество.

## E-W00-P0 — вернуть actual source-conditioned DPO вместо CODE_TRACE_DPO defect-type surrogate (**первый neural test**)

**Уже доказано другими агентами, не переписывать их работу:** [N49-01/N49-03](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md), [read-only projection receipt](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/projection-read-only-receipt.json). Для исторических 50 train и 50 heldout строк Router old `pair.Prompt` попадает в neural DPO; текущая `ToDpoMetricPair` использует surrogate `pair.RejectedType`/fixed tokens. Source-equivalent проекция тех же старых строк дала **50 исходных разных prompts → 4 model-visible triplets**, **46 схлопнувшихся** внутри каждого split, **6 различных корректных target routes** в группе `wrong_training_unit`. Это **не новый .NET training и не фактические counts текущего `metricTrain`**.

**Домен:** Wiki RequestRouting/оркестрация. **Второй отдельный профиль:** C# CodeTrace builder с реальными символами/call edges, а не языковая POS-translation. **Получатель:** neural `ProjectWikiDpoMetricRunner` policy и **отдельно** реально обслуживающий `ProjectWikiRequestRoutingTrainableModel/ProjectWikiTrainingOrchestrator`, не считать их одними весами.

**Native A/B/C:** A = gold source-conditioned `Prompt/Chosen/Rejected`, B = current defect-type projection, C = current код с **единственным изменением** на faithful model-visible `Prompt/Chosen/Rejected`, при одинаковых `pairId`-independent train/heldout, initialization, tokenizer, 50/50, seeds, optimizer, steps. Запускать в isolated worktree/scratch; teacher provenance и guardrail выполняются; публикация без semantic gate не разрешается.

**Перед обучением** сохранять `50 original prompts / distinct generated actual metric-triplets / collisions / target-label entropy per triplet / exact train-heldout metric input overlap`; вывести SHA входного `metricTrain`, не только архивной аналитической проекции. **После обучения** измерить pair preference, heldout **semantic route accuracy and rare-route confusion**, cold-loaded route into ordinary consumer (если consumer не wired — `NOT_ADOPTED`, а не SCORE IMPROVED), независимый user task outcome и latency. **Отрицательный контроль:** метки rejected-type shuffled but prompts/target changed, чтобы выявить обучение только дефектному слову; отдельно кодовый C# CodeTrace и недоменные ENRU примеры, где эффект не предполагается.

**PASS quality:** C корректнее B на заранее закрытом blind наборе новых prompt families и cold-restored consumer реально использует веса; если лишь cardinality выросла без лучшего ответа, `SIGNAL_RESTORED_QUALITY_NOT_ESTABLISHED`. **Falsifier:** B оказывается столь же хорош на независимом routed task score при matched budget и реальном consumer. Перед масштабированием — проверить P0-W2 семантический gate, **не** отменять его.

## E-W01-P0 — восстановить потерянный маршрутизатор клиент→сервер (самый первый)

**Домен:** проектная документация / программирование full-stack / оркестрация бизнес-процессов. Профили: Angular→.NET API→SQL→React/Angular UI; React→ASP.NET API. **Не применять** к RU→EN, AST-only или чистому SQL.

**Получатель:** `ProjectWikiQueryPlanningFlow` внутри обычного `ProjectLocalWikiOrchestrator.AskWithTraceAsync`, далее retrieval/evidence/answer. **Гипотеза:** исчезновение `ClientServerBusinessFlow` снижает покрытие evidence и полноту ответа. [Gold исходник](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiQueryPlanningFlow.cs#L128-L150); [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiQueryPlanningFlow.cs#L135-L177).

**A/B/C при одинаковом evidence index и baseline checkpoints:**
- A: gold `local_wiki_planning` ровно pinned commit; B: current `codex/dpo-implementation`; C: current **только с восстановленным intent и соответствующей цепью 15 шагов** (не менять DPO, LoRA, retriever, judge). У C сохранить нынешний `BuildCrossStackEntityImpactSteps`.
- Inputs: [оценочные случаи](05-CROSS-STACK-FIXTURE.json): 8–12 положительных клиент/сервер вопросов и 4+ отрицательных (field-change с новой улучшенной веткой, SQL-only, isolated workflow, missing evidence). Если независимых gold answers ещё нет, выбрать вручную по source files, blind до запуска.
- Реальная проверка: `intent accuracy`, `required step-family recall` для UI route/binding, client service, API contract, DTO, authorization, server caller, persistence, returned response/client redraw, missing evidence (9 классов), `exact source-citation accuracy`, `ordinary answer semantic correctness`, `wrong-project abstention`, `elapsed_ms`.
- Предварительный **адресный критерий**: C восстанавливает intent для ≥90% положительных cases и ≥8/9 evidence families там, где эти виды evidence существуют; ≥0 потерянных negative-control successes от B. Порог не является статистическим доказательством. Пользовательский выигрыш засчитывается только если C отвечает правильнее B по слепому судье на тех же исходниках; иначе **P0-W1 влияет на полноту плана, но quality gain не установлен**.
- **Falsifier:** на matched gold docs C не улучшает ни одной blind оценённой пары или новый "field-add" regression. **Остановка:** если Step branches не изменились — проблема в более раннем DetectIntent или consumer; не запускать крупный тренинг.

**Проверить, что именно сохранили:** prompt со сменой сущности клиента должен по-прежнему идти в *enhanced* `BuildCrossStackEntityImpactSteps`, а не в старый generalized flow из-за слишком широкой проверки.

## E-W02-P0 — обученная маршрутизация реально влияет на обычный Flow

**Домен:** документация и планирование разработки. **Получатель:** конкретный `ProjectWikiRequestRoutingTrainableModel` + `ProjectWikiTrainingOrchestrator` (проверять owner).

**База A:** текущий `ProjectWikiRequestRouterUnit` (deterministic ordinary consumer), trained unit не загружен. **B:** полностью обученная модель на небольшом **семантически размеченном** train (по 3–5 примеров на целевой класс, включая rare), но *без* подстановки в consumer; **C:** тот же snapshot через `FromSnapshot` и wiring в ordinary consumer после независимого held-out semantic gate. **D:** C после cold process restart. Намеренно оставить одни и те же вопросы, чтобы видеть: weight change vs loaded vs called.

**Метрики:** правильный `selected_target_unit_id` по blind 20–30 примерам; routing confidence vs true accuracy; `trained_route_invocations / ordinary_route_invocations`; published & consumed `model_sha`; abstention; wrong-route harm; latency. Отдельно считать minority classes. Выдача `PASS` только если C (и D) имеет лучше blind semantic accuracy, чем A, и native trace подтверждает использование того же artifact. Без independent semantic judge **не** делать `semanticQualityClaimed=true` и не отключать publication safety.

**Falsifier:** C = A по вызовам или после cold restart недоступна state, либо B preference score вырос, но C ordinary route не изменился. **Отдельный baseline:** источник `TrainAndEvaluateAsync` выставляет semanticQualityClaimed=false → публикация может быть заблокирована; это ожидаемый *безопасный* FAIL, а не просьба обойти gate.

## E-W03-P0 — тест QueryPlanningTrainingRunner проверяет необучаемый план или учебный эффект?

**Домен:** Wiki query planning / документация. **Получатель:** `_planner.PlanAsync`, отдельно `ProjectWikiDpoMetricRunner`. Из [source runner](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiQueryPlanningTrainingRunner.cs#L44-L100) видно, что `trainingRows` не обновляют `_planner`.

**A:** 12 корректных train DPO с одинаковым held-out из 12 независимых вопросов (на реальном runner для строго 50-row gate использовать 50/50). **B:** изменить train `Chosen/Rejected` так, чтобы пары оставались legal и syntactically valid, но другое смысловое правило стало «chosen»; *НЕ* менять held-out; **C:** train with shuffled PairId and projectId values; повторить в отдельном isolated scratch без публикации.
- Снять hashes `trainingRows`, actual `_planner` state before/after, `PlanAsync` outputs по held-out, preference changes в DPO scorer, ordinary `ProjectLocalWikiOrchestrator` ответы.
- Ожидаемый **source-predicted** результат: deterministic runner PlanAsync output A=B, когда меняется лишь chosen/rejected. Это не PASS обучения, а доказательство **нечувствительности gate**.
- **Если после настоящего training policy upgrade normal planner output меняется**, записать точный loader/consumer link — тогда source hypothesis опровергнута.
- **Защита от ложного эксперимента:** исходная система требует 50/50 и legal teacher; плохие chosen менять исключительно в экспериментальном shadow dataset, не в production corpus и не ставить его в state train.
- **Критерий достаточности:** independent adjudicator различает три величины `preference_score`, `deterministic_plan_quality`, `model_backed_answer_quality`.

## E-W04-P0 — точное повторение «успешного обучения» wiki, а не чтение старых receipts

**Домен:** проектная документация/wiki. **Эталон:** gold `promotion025` и base `batch003`, pinned receipts из [01](01-BASELINE-RECEIPTS-AND-CLAIMS.md). **Получатель:** именованный `project-wiki-query-planning-flow` или `ProjectLocalWikiModelUnit` (не смешивать).

**A:** cold gold checkpoint на историческом original loader; **B:** fresh gold branch retrain на исходном корпусе; **C:** cold current branch loaded donor weights с migration (если совместимо); **D:** same current fresh retrain (только один конфигурационный delta). Начать с 12 independent held-out natural questions 2 реальных проектов (не генерированных из evidence index), позже 3+ проекта; source paths and owner of evidence blind.
- Для каждого: `loaded_checkpoint_sha`, tokenizer identity, LoRA layer count, prompt+pair exact material, generated ordinary response, evidence citations, abstention, latency/RAM, **train-only vs held-out family distance**.
- **Success:** gold A/B воспроизводят историческую *конкретную* learned capability, C/D не хуже по blind semantic/citation evaluation, restoration survives restart. Если gold уже невозможно восстановить, записать `DONOR_ARTIFACT_UNAVAILABLE`; не выдавать 0.98 preference за ordinary Q/A.
- Контроль: `model_weights_suppressed`, `retrieval_only`, `shuffled_checkpoint`; если output инвариантен при отсутствии модели, отличить retrieval benefit от neural gain. Это не означает, что retrieval плох — просто иной результат.

## E-W05-P1 — метки snapshot и enum numeric migration

**Домен:** маршрутизация, документация. **A:** old snapshot `Labels=["RouteA","RouteB",...]`, `TokenWeights/Biases` однозначны; current `FromSnapshot` обычным путём. **B:** permute `Labels` вместе с позициями весов, сохранить сам смысл. **C:** предложенный name-keyed remap или explicit schema version rejection. Проверить, что `RouteForEvaluation(text)` и `Route(text)` выбирают **одинаковую именованную метку** в A/B/C, а старый numeric `ProjectWikiQueryIntent.ClientServerBusinessFlow` не превращается в соседнюю категорию. **Контроль:** сериализация именами enum без ordinal: если она используется, соответствующая часть гипотезы отвергается. Никаких опасных migrations production файлов.

## E-W06-P1 — margin vs tautological confidence

**Домен:** классификация/маршрутизация Wiki. Сгенерировать 8–12 score vectors: (1) tie, (2) чуть ниже `MinimumTopTwoMargin`, (3) чуть выше, (4) сильный winner, (5) OOD/unknown tokens. Вызвать current `DecideOrDefer` через test hook, вывести `top_two_margin, raw_unclamped_confidence, returned_confidence, decision, correct_label`. В A порог с нынешним `Math.Clamp(..., MinimumSelectionConfidence,...)`; B shadow-calibration без самозамыкающегося нижнего clamp, но прежний top-two gate. Слепые semantic labels, negative calibration `expected calibration error` и wrong-route rate. **Доказанный арифметический факт:** returned confidence не опустится ниже cutoff при прохождении margin. **Не доказано:** что это вызывает именно current RP01 quality gap. Отдельно сохранить conservative abstention.

## E-W07-P1 — доменные доноры E09/E26/E43 (не заявляем их новыми)

| Тема обучения / профиль | Что переносить | Быстрый A/B и необходимый контроль |
| --- | --- | --- |
| Язык программирования, C# AST | E09 структурные features→KNN; НЕ Roslyn для natural-language translation | Одинаковые file-disjoint examples/labels для unigram/bigram vs AST-KNN; одно и то же held-out denominator; warm/cold index reload, top1+latency. См. [N44 old donor](../44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md) |
| Перевод RU→EN, техническая омонимия | E26 словарь/POS и выученная typed memory, не «большая LoRA» автоматически | Word-only vs POS-context vs trainable memory с новыми контекстами, смысл/negation/word-order на фразах не из teach, accuracy + abstention. См. [N46](../46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md) |
| Непрерывное обучение (любой допустимый домен), исходный donor bigram | E43 rehearsal/replay сохранения старых законов, без claim LoRA | new-only vs new+replay на одинаковом update budget, old/new file-disjoint, old retention+new acquisition и cold restart. См. [N43](../43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/README.md) |
| Оркестрация разных обучаемых классов | E22 late mixed success, rare class failure | rare-class F1 per class, routing-specific confusion, old/current training vs consuming Flow; ранний FAIL и поздний PASS разные корпуса, поэтому не считать direct delta |

## Результаты: единая форма записи

```json
{
  "experiment_id": "E-W01-P0",
  "status": "NOT_RUN",
  "domain_subject": "project documentation",
  "profile": "Angular->.NET->SQL->UI",
  "branch_and_commit": {"gold": "PIN_SHA", "current": "PIN_SHA", "shadow": "PIN_SHA"},
  "dataset_sha256": "REQUIRED",
  "train_heldout_split": "independent-by-project-and-source-file",
  "recipient_unit_or_flow": "ProjectWikiQueryPlanningFlow",
  "weights_touched_sha256": "UNKNOWN",
  "loaded_checkpoint_sha256": "UNKNOWN",
  "ordinary_consumer_trace": "UNKNOWN",
  "gold_ordinary_response": "UNKNOWN",
  "baseline_ordinary_response": "UNKNOWN",
  "enhanced_ordinary_response": "UNKNOWN",
  "blind_semantic_score": "UNKNOWN",
  "delta": "UNKNOWN",
  "negative_controls": "UNKNOWN",
  "conclusion": "NOT_ESTABLISHED"
}
```

**Приёмка:** evidence before after; действительно изменённая обучаемая state и действительно используемый unit/flow, семантический выигрыш или недвусмысленное опровержение. Не подгонять метрику к обучающейся системе, не ждать Windows GitHub Actions: targeted local `dotnet` experiment и независимый judge, при необходимости отдельный test только на кодовую регрессию. Пользователь видит human-readable success/fail, изолированную гипотезу и версию исходников.
