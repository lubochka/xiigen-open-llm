# История успешного обучения и две новые гипотезы о памяти Mode1

Дата аудита: 2026-10-20. Исследование для `possibleDifs`; защищённая папка `docs/plans/replanning-possible-diffs` не изменялась. Новые native эксперименты не запускались. Проверены исходники, архивные receipts, реальные raw outputs и замороженный scorer. Ниже отдельно обозначены измеренные архивные результаты, подтверждённая логика исходника и ожидаемые результаты ещё не выполненных опытов.

## 1. Основной результат

Успешное обучение в истории действительно есть. Память Unit/KNN является обучаемым состоянием; отсутствие neural updates не отменяет её обучения. Для 18-Oct translation ingress удалось независимо пересчитать **11/24 → 19/24** по 48 сохранённым native ответам и исходному scorer. Улучшение относится к композиции выученных частей EN→RU, одновременно выросло число неверно принятых ответов **1 → 3**. Это уже реализованное исправление доставки `phrase_mappings`, а не новая предлагаемая гипотеза.

Обнаружены два отличающихся от P67/P118 механизма в **Mode1**, который действительно читает выученный `Chosen.PlanText`:

1. **H-M1-RETENTION:** новый парафраз с новым PairId того же FlowRef добавляется в KNN, но полностью заменяет input feature weights старого урока. Может ухудшаться удержание ранее выученного маршрута, хотя число сохранённых уроков растёт.
2. **H-M1-IDENTITY:** grounding выбирает FlowRef по одной системе scores, а окончательный Chosen берётся из глобального Mode1 KNN без фильтра по этому FlowRef. План может содержать заголовок одного flow и тело другого. Это возможно даже при одном уроке на каждый flow, без перезаписи.

Оба механизма находятся на реальном `TrainOrchestrationBatchAsync → learned state → GeneratePlanAsync → RenderLearnedFlowPlan` пути. Первый — самостоятельная гипотеза continual learning; второй — доставка результатов обучения потребителю. Для каждого ниже дана конкретная небольшая учебная A/B постановка. Здесь **нет измеренного эффекта этих исправлений**: приведённые scores — ручной вывод из закреплённого исходника, требующий проверки native цепочкой.

## 2. Закреплённые версии и предел исследования

| Источник | Commit |
|---|---|
| core `codex/dpo-implementation` | `bf9b543e149ef197da85a767821d98c9b67916d2` |
| planning `llm_plan3` | `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0` |
| core `orchestrator-extensions` | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` |
| core `language_translate` | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` |
| core `DesignToCode` | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` |

**Свежесть:** финальная сверка обнаружила core HEAD `de4ef0f14172dada4daf142a4c66ba19e96bb6d4`. Два добавленных после `bf9b543` коммита меняют документацию; C#-исходники не менялись, поэтому закреплённые source-ссылки остаются применимыми. Более поздняя дельта active06/active23 и P67/P118 проверена отдельно; предел этой сверки указан в разделе7. [Сравнение версий][live-delta]

Использованы code-examination и architectural-decision-testing skills: caller→learner→state→consumer; отдельный статус факта/гипотезы; один фактор на контраст; неизменённые исторические результаты. Основные выводы про старые E09/E25/E43, потерянные restore поля и исторические 100%/80% **уже были описаны другими агентами** и ниже не названы новыми.

`PromptFlowGroundingModel.cs`, `OrchestrationKnnStore.cs`, `OrchestrationPlanMachine.cs` в current совпадают с `orchestrator-extensions` после исключения пятистрочного AGPL header. Например, [историческая модель][historical-grounding] содержит ту же запись per-flow weights и глобальный retrieval. Следовательно, H-M1-RETENTION и основной путь H-M1-IDENTITY — **унаследованные ограничения**, а не установленная регрессия, внесённая RP01. Зависимость конкретного RP01 run от этого OrchestrationPlanMachine дополнительно не доказана. Область находок — тот runtime, который действительно обучает и вызывает эту машину.

На данной машине `dotnet`, `csc`, `mcs` не найдены. Подменять native обучение JS/Python моделью не стали. JS использован только для повторного применения неизменённого оценщика к уже существующим архивным native outputs; это аудит измерений, а не новый learning experiment.

## 3. Что действительно работало в соседних ветках

| Источник | Реально наблюдаемый результат | Механизм и границы |
|---|---|---|
| language_translate E25 | `top1_accuracy` 0.3438 → 0.875; N=32; из округлённых значений и N выводятся 11/32 → 28/32. Hard cases 24/28. Время 0.4 → 15.4 ms | `Train(train)` получает 128 из 160 синтетических POS последовательностей; k=3, Jaccard по POS bigrams. Положительный результат классификации порядка; не end-to-end перевод. [Receipt][e25-result], [обучение и оценка][e25-code]. |
| language_translate E43 | `retention_accuracy` 0.3226 → 0.3978; oldPairs465, newPairs414, replay177. Из split выводится N=93 и примерно 30/93 → 37/93. Время0.1 → 5.8ms | Обучаются новые таблицы переходов на newPairs либо newPairs+replay. Начальный baseModel не продолжается: создаются независимые `BuildBigram(...)`. Это полезный эффект replay в count memory, **не доказательство восстановления весов прежней нейросети**. [Receipt][e43-result], [actual builds][e43-code]. |
| orchestrator-extensions E09 | Архив пишет0.2413 → 0.3507; 5617 tokens,4492 trainPairs; время14.5 → 763.5ms | Реальный StructuralKnnPredictor.Train есть, но baseline использует все val, KNN только Take(500). Для этих размеров знаменатели1123 и499, примерно271/1123 и175/499. Нельзя приписать всю дельту функции KNN без одинаковой панели. Дополнительно labels индексируются заново по каждому файлу. [Receipt][e09-result], [панели][e09-code], [labels][e09-labels]. |
| DesignToCode V210 | 134 DPO pairs,9 batches, final train accuracy0.833333, final loss0.690332, checkpoint1,189,921bytes | Это реальный `LlmModel → DpoTrainer.StepMany → SaveCheckpoint`, не только словарь. Но итоговый train metric не является before/after heldout quality. Старый materializer хеширует слова и добавляет разные `Chosen:`/`Rejected:` headers; известные confounders нельзя копировать в современный эталон. [Receipt][v210-result], [trainer][v210-code], [materializer][v210-materializer]. |
| orchestration-v1 | Архив: Mode1 accuracy1, unseen style0.8, explanation1, leakage0 | Сохранившаяся память и consumer; manifest содержит42+200+30=272 train и20+40+20=80 heldout. Поле `training_pairs_count=352` включает все строки; не следует называть352 обучающими. Архивные доли не доказывают semantic fidelity всех новых планов: соответствующие ограничения evaluator уже известны. [Report][orch-report], [manifest][orch-manifest]. |

### Доменная применимость матрица → функция KNN

AST topology и syntax kinds осмысленны для parseable source code, graph/plan structure — для orchestration, POS bigrams и роли/порядок — для языка. Само слово «структурный» не делает эти representations взаимозаменяемыми. E09 использует Roslyn C# AST; E25 использует синтетические POS sequence labels. Их числа нельзя переносить на EN→RU output fidelity либо design layout fidelity. Точно так же положительный memory/KNN результат нельзя объявить отсутствием обучения из-за неизменённых neural weights.

Эти receipts подтверждают полезные функции KNN/count memory и существование neural training, но сами по себе не являются чистой matched-panel проверкой «та же функция внутри матрицы не работает, вынесенная из матрицы работает»: E25 сравнивается с правилами, E09 — с bigram на другой панели. Для такого вывода нужен соответствующий сохранённый matched contrast, либо будущий маленький domain-specific native опыт. Уже существующие исторические разборы N11/N12/N52/N56/N60 должны сохранять авторство и ограничения.

## 4. Независимая проверка актуального положительного ingress результата

Источники: [18-Oct report][ingress-report], [raw before][ingress-before], [raw after][ingress-after], [rubric][ingress-rubric], [actual scoring wrapper][ingress-scorer], [evidence validation][ingress-validation]. Raw outputs обеих ветвей, gold и scorer прочитаны целиком. Повторное применение исходной `assess()` даёт:

| Панель | Correct до | Correct после | Accepted до | Accepted после | Wrong accepted до → после |
|---|---:|---:|---:|---:|---:|
| Exact training examples | 8/8 | 8/8 | 8 | 8 | 0 → 0 |
| Near paraphrases | 3/3 | 3/3 | 3 | 3 | 0 → 0 |
| New combinations | 0/8 | 8/8 | 0 | 8 | 0 → 0 |
| Edge cases | 0/5 | 0/5 | 1 | 3 | 1 → 3 |
| Все | 11/24 | 19/24 | 12 | 22 | 1 → 3 |

Correct/accepted падает с11/12=91.67% до19/22=86.36%. Результат является успехом по композиции известных частей и correct/all на данном EN→RU материале; регрессия wrong acceptance сохраняется рядом и не отменяет этот локальный успех. Пересчёт использует исходную функцию `assess()` и сохранённые ответы; модель не запускалась.

Фактические ошибки: `do not delete the file → удали файл` и `did you delete the file → удали файл` после исправления принимаются phrase-map адаптером; раньше они попадали в bootstrap. `validate the file and the cache` даёт «проверь файл» до и «проверь файл кэш» после; оба не проходят строгий прежний rubric. Это не новая догадка о риске, а конкретные архивные ответы.

**Что было обучено.** В двух независимых процессах одинаковые8 уроков проходят настоящие private reader/builder в `27.TranslationDomain.Tools/Program.cs`, затем `TranslationMachine.TrainStructuredBatchAsync`, затем обычный `GetOrchestratorAsync().RunAsync`. Во входе уже были16 `phrase_mappings`; старый transport передавал0, исправленный16. Domain DLL и сами алгоритмы Executor не менялись. Initial canonical state hashes совпали; в обеих ветвях8 identities,1batch,24KNN entries; neural updates0. Это полноценное обучение памяти, а не внешняя замена output. Полный CLI registry/manifest не запускался; RP01 ResearchCheckpointEnRuAdapter — отдельный consumer.

**Чего не следует утверждать.** P125 memory-acquisition0/24→19/24, ingress11/24→19/24 и cold restore P25 — разные сравнения. Их нельзя сливать. Наблюдаемое время24 ответов291.0535→52.9254ms, training38.5685→34.5068ms, checkpoint7515→8954bytes; одиночные запуски/JIT/порядок не доказывают стабильное ускорение. Process peak WS56,909,824→55,107,584bytes; sampled group maximum66,539,520→82,214,912bytes на5 против2 samples — это разные измерители, а не единая RAM дельта. Эти пределы уже честно указаны в исходном report.

`comparison.json` при чтении через GitHub оказался Git LFS pointer (oid sha256 d739be03eea3c79985898af3e1134200ec2e0fa348c25aec690d53346bc8dbd2, size8632), поэтому его содержимое не выдаётся за проверенное. Проверенные raw before/after, gold и scorer доступны и достаточны для воспроизведения приведённых счётчиков. Новых запусков ради этого аудита не требуется.

## 5. H-M1-RETENTION: новый урок того же FlowRef вытесняет старые входные признаки

### Подтверждённая цепочка

1. [TrainOrchestrationBatchAsync][machine-train] пропускает повторный PairId, но принимает разные PairId одного FlowRef и вызывает `_promptFlowGroundingModel.Train(pair)`.
2. [Train][grounding-train] сначала `_knn.Add(pair)`, затем безусловно `_weights[pair.FlowRef] = BuildWeights(pair)`. [BuildWeights][grounding-build] берёт input features только из текущего `pair.Prompt`. Старые KNN пары остаются, старые input feature keys этого flow исчезают.
3. [TryGround][grounding-predict] сначала выбирает FlowRef по `_weights` и лишь затем проверяет глобальный nearest score. Сохранённая старая пара не гарантирует прежний flow winner.
4. [FindLearnedPair][grounding-find] добавляет features уже выбранного grounding к query. Это может дополнительно переключить финальный nearest на конкурирующий flow.
5. [GeneratePlanAsync][machine-consumer] передаёт выбранную пару в [RenderLearnedFlowPlan][composer]. Тело результата содержит `Chosen.PlanText`; это реально потребляемое содержание обучения.
6. [Checkpoint export/import][machine-state] сохраняет все пары и только актуальные per-flow weights и импортирует обе структуры как есть. Простая cold reload не восстанавливает потерянные признаки.

Состояния могут расти в KnnSize и TrainedPairIds одновременно с потерей старой ассоциации. Это отличается от P67.2: там повторный **тот же PairId** блокируется; здесь все PairIds уникальны, обучение успешно допущено. Это отличается от P118: там Mode2/3 не читает chosen body; здесь Mode1 читает выбранное body, но обучение может изменить, какое body выбрано.

Исторический [AddMode1Pairs][mode1-corpus] создаёт ровно один train prompt на каждый из42 flows; heldout содержит те же flows. Поэтому сохранённый Mode1 success не проверяет два разных последовательно обученных парафраза одного FlowRef. Это конкретный пробел покрытия, не опровержение исторического успеха.

### Малый учебный A/B: RETENTION-M1-01

**Статус:** `DESIGN / NOT_RUN`. **Домен:** ProjectWiki orchestration, удержание ранее выученного плана после добавления второго входного описания. **Получатель:** реальная `OrchestrationPlanMachine`; путь всех экзаменов — `GeneratePlanAsync`, без обхода в standalone scoring helper.

**Материал — искусственная лабораторная спецификация.** Это не реальный Xiigen flow и не доказательство широкого языкового обобщения. IDs `LAB-A`/`LAB-B` намеренно не содержат цифр: deterministic flow-number parser не должен решить опыт вместо learned features. Все rows имеют Mode1, unique PairId, unique SourceBundleId, IsHeldOut=false, ExpectedRouteMode=LearnedFlowGrounding, ExpectedBoundarySource=Classifier, ExpectedMlGenerated=true, ExpectedIntentKind=ImplementationExecutionRequest. Outer FlowRef=Chosen.FlowRef. `SourcePath=simulation:...` явно обозначает происхождение. Chosen и rejected фиксируются до выходов; не загружаются из evaluation gold.

| Порядок | PairId | FlowRef | Prompt | Chosen body |
|---|---|---|---|---|
| 1 | `00-b` | LAB-B | `validate payment receipt email` | `Validate receipt, then email the receipt.` |
| 2 | `10-a1` | LAB-A | `validate payment approval ledger` | `Validate approval, then persist the ledger.` |
| 3 | `20-a2` | LAB-A | `check invoice consent journal` | Тот же правильный план LAB-A |

Для B rejected — отправка до проверки receipt; для A — запись ledger до проверки approval. Этот learner не читает RejectedText: не называть опыт neural DPO optimization.

**A current. B один фактор:** в `PromptFlowGroundingModel.Train` объединить старые и новые `InputFeatureWeights` того же FlowRef через token-wise max; остальные поля свежего `BuildWeights`, KNN, IDs, classifier, scorer, confidence и renderer оставить как в A. Это ограниченная проверка накопления альтернативных входных описаний; не универсальная политика конфликтующих corrections.

```csharp
_knn.Add(pair);
var fresh = BuildWeights(pair);
if (_weights.TryGetValue(pair.FlowRef, out var old))
{
    var input = old.InputFeatureWeights.ToDictionary(
        kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
    foreach (var kv in fresh.InputFeatureWeights)
        input[kv.Key] = Math.Max(input.GetValueOrDefault(kv.Key), kv.Value);
    fresh = fresh with { InputFeatureWeights = input };
}
_weights[pair.FlowRef] = fresh;
```

Оба arms создаются независимо с пустой памятью; одинаковые уроки, порядок, batch size1. После каждого single-pair `TrainOrchestrationBatchAsync` записывать RowsTrained, actual pair IDs, hashes/maps. Уроки и экзамен неизменны.

| Frozen query | Gold |
|---|---|
| `validate payment approval ledger audit` | LAB-A; approval перед ledger persist |
| `check invoice consent journal review` | LAB-A; тот же план |
| `validate payment receipt email summary` | LAB-B; проверка перед email |
| `schedule backups policy cron` | Не выдавать чужой learned plan; Mode2 fallback допустим |

**Этапы:** empty memory; послеB; послеA1; послеA2; cold load final checkpoint и тот же экзамен. Основной learning contrast — retention старого query между послеA1 и послеA2, а затем difference A/B. Всего2arms×3traincalls=6 реальных training calls;4queries×5stages×2arms=40 ordinary outputs. Cold restore — часть этого обучения, не отдельный zero-update опыт. Обратный порядок не обязателен; добавлять его только при конкретной неясности результата.

**Фактический cold path:** после финального урока вызвать `SaveCheckpointAsync` в отдельный файл каждого arm и сохранить его hash; завершить процесс этого arm. В новом процессе той же A- или B-сборки вызвать `LoadCheckpointAsync`, взять возвращённую machine и выполнить четыре frozen queries без новых уроков. Вызов `ImportCheckpoint` в прежнем процессе не является cold restore. Сравнить сохранённые/прочитанные pair IDs, weights и фактические ответы. [API сохранения и загрузки][machine-persistence]

### Почему stored KNN не обязан спасти старый урок

Следующий trace — **аналитическое ожидание, не результат исполнения**. [Tokenize][knn] делает lower-case distinct words, KNN использует Jaccard и lexical PairId tie-break.

| Состояние для old query | Weight scores | Финальный nearest после evidence | Ожидаемое тело |
|---|---|---|---|
| Оба arms послеA1 | LAB-A4, LAB-B2 | A1:4/7, B:2/9 | A |
| Current послеA2 | LAB-A0, LAB-B2 | B evidence расширяет query до9tokens; A1=4/9, B=4/9; `00-b` выигрывает tie | B, потеря старого плана |
| Merge послеA2 | LAB-A4, LAB-B2 | A evidence first8=`a,approval,check,consent,invoice,journal,lab,ledger`; A1=A2=4/11, B=2/13 | A, удержание |

До расширения query глобальный nearest A1 остаётся4/5. Поэтому этот fixture явно проверяет, спасает ли хранение старой пары итоговый ответ; одного доказательства её присутствия недостаточно. Confidence в этих точках по исходной формуле cap=1; не должен блокировать consumer. `review` без `architecture` не переключает classifier в ArchitectureReview.

**Нужные наблюдения:** raw PlanText и grade по body; FlowGrounding.FlowRef; Chosen.FlowRef; chosen PairId; score всех кандидатов; global nearest до evidence; evidence features; фактические classifier fields. Gold/scorer не может засчитать `FlowRef` из повторённого prompt или correct route flag без правильного порядка действий.

**Успех:** любое фактически измеренное улучшение полезной метрики в этом домене — успех по этой метрике. Рост accuracy не обязателен. Показать old-plan retention, new-plan acquisition, wrong-plan/unsupported acceptance, latency, bytes state, cost/update и все ухудшения рядом. Например, больший словарь weights — возможная цена удержания, а не причина скрыть положительный результат. Neural layers/VRAM N/A; learned entries/feature keys и executed consumer обязательны.

**Корректные ресурсы:** нынешний `OrchestrationTrainingBatchResult.CpuMs` получает `Environment.TickCount64` elapsed, а `PeakRamMb` — разовый `GC.GetTotalMemory(false)`, то есть managed heap; эти имена не доказывают CPU time или peak RSS. Для опыта вокруг реального Train/consumer отдельно измерить `Stopwatch` elapsed и дельту `Process.TotalProcessorTime`; peak RSS/working set брать из независимого измерителя с указанным методом и областью наблюдения. Если измерителя нет, честно писать elapsed/managed-heap snapshot и CPU/peak RSS=`NOT_MEASURED`. Переименование старого поля не является физической экономией. [Текущие присваивания][machine-cost-fields]

**Фальсификаторы/предел:** native chain сохраняет старое body в обоих arms; last-only replacement является документированным требованием для данного FlowRef; выбранные prompts не достигают Mode1; модификация меняет другие поля/данные/порядок. В первом случае подтверждена потеря features, но не потеря качества на панели; во втором механизм может быть ожидаемой revision policy. Ни отсутствие эффекта, ни полезный эффект не переносить автоматически на translation/AST domains.

## 6. H-M1-IDENTITY: grounding и полученный Chosen могут относиться к разным flow

### Подтверждённый код

[TryGround][grounding-predict] выбирает `best.FlowRef` по input+allowed scope scores. Global nearest другого flow может обеспечить confidence, поскольку `_knn.FindNearestWithScore(queryText, Mode1)` не фильтруется по best.FlowRef. [FindLearnedPair][grounding-find] тоже делает глобальный nearest по Mode1. [AllowsLearnedGrounding][machine-admission] проверяет scope/forbidden **только у grounding.FlowRef**. [Composer][composer] затем печатает `flow_ref={grounding.FlowRef}`, но title/body и `FLOW_REF={learned.Chosen.FlowRef}` берёт из другой структуры. Нормализации/проверки совпадения в этом consumer нет.

Это отдельный механизм от перезаписи: два scores используют разные метрики — пересечение weighted features и Jaccard с нормализацией длиной. Даже с одним уроком на FlowRef длинный релевантный урок может выиграть grounding, а короткий частично совпадающий — content retrieval.

### Малый учебный A/B: IDENTITY-M1-01

**Статус:** `DESIGN / NOT_RUN`. Домен/DTO metadata такие же, как в RETENTION; отдельные независимые машины. Число lessons2, одинаковая последовательностьB→A в обоих arms. Этот опыт не объединять с изменением update/merge: иначе невозможно различить причины.

LAB-B lesson:

```text
Prompt: validate payment ledger release
Chosen: Validate payment, persist the ledger, then release the receipt.
```

LAB-A lesson — контролируемое длинное описание,45 разных слов:

```text
Prompt: validate payment approval ledger archive audit compliance evidence journal record retention reconciliation balance account transaction customer invoice consent signature security trace backup version timestamp report settlement currency receipt notification queue pending review deadline exception checksum authorization persist recovery replica durability log history status monitoring integrity
Chosen: Validate approval before writing the ledger; preserve the approval record.
```

Rejected для A — запись без approval; для B — release до validation. Источник — laboratory specification, не реальный flow corpus. Query `validate payment approval ledger` требуетA; `validate payment ledger release` требуетB; `schedule backups policy cron` не должен порождать чужой learned plan.

**A current. B один фактор:** final candidate admission в `FindLearnedPair` допускает только pairs с `pair.FlowRef == grounding.FlowRef`. Реальная правка — добавить optional flowRef filter до ranking в `OrchestrationKnnStore.FindNearestWithScore/FindNearest` и передать grounding.FlowRef из `PromptFlowGroundingModel.FindLearnedPair`. Остальные nearest calls, confidence, weights, исходные уроки и их порядок не менять в этом опыте. Если valid same-flow candidate отсутствует, вернуть null и использовать существующий fallback; не внедрять expected answer. Отдельное изменение confidence на same-flow similarity сюда не включать.

**Этапы:** empty; afterB; afterA; cold restore.2arms×2trainingcalls=4 calls;3queries×4stages×2arms=24 final outputs. Измерение afterA проверяет, стала ли newly taught approval skill доступна обычному consumer. Оно связано с реально записанной learned memory, а не с отвлечённой строковой проверкой.

**Cold и ресурсы:** финальный state каждого arm записать через `SaveCheckpointAsync`, зафиксировать file hash, завершить процесс и открыть его через `LoadCheckpointAsync` в новом процессе той же A/B-сборки. Это даёт по три уже включённых в бюджет cold queries без нового Train; `ImportCheckpoint` в прежнем процессе не заменяет этап. [API][machine-persistence] Для Train и consumer раздельно измерить `Stopwatch` elapsed, дельту `Process.TotalProcessorTime` и peak RSS/working set независимым именованным измерителем. Нативный `CpuMs` фактически elapsed, `PeakRamMb` фактически snapshot managed heap; если настоящего измерителя нет, указать именно эти величины, а CPU/peak RSS=`NOT_MEASURED`. Исправление labels не называть ускорением. [Присваивания][machine-cost-fields]

**Аналитическое ожидание qA после обученияA (не measured):**

1. Query4tokens. LAB-A InputScore4, LAB-B3; grounding выбираетA.
2. Global KNN перед evidence выбираетB сJaccard3/5 противA4/45; confidence cap1.
3. EvidenceA first8=`a,account,approval,archive,audit,authorization,backup,balance`; expanded query11tokens.
4. A similarity10/46≈0.21739, B3/12=0.25. Current final KNN выбираетB.
5. Текст содержит `flow_ref=LAB-A`, но `FLOW_REF=LAB-B` и B body без требуемого approval. ФильтрB выбираетA. qB должен сохранятьB: scoreB4>scoreA3.

Не считать одно исправление `FLOW_REF` достаточным: эксперимент оценивает содержательное learned действие/порядок из actual PlanText. Измерять correct/all, answer coverage, correct/accepted, wrong accepted, flow-body disagreement, retentionB, costs/state размер. Любое измеренное полезное улучшение — успех по метрике/домену; все регрессии рядом. Если ожидаемое расхождение не воспроизводится native consumer, статус гипотезы корректируется; не подкручивать corpus после просмотра ответов ради PASS.

## 7. Покрытие существующими документами и следующий порядок

Точечный поиск завершён по всем **555 исходным Markdown planning** и **248 protected non-result Markdown** закреплённых версий из раздела2. Искались `PromptFlowGroundingModel`, `FindLearnedPair`, `BuildWeights`, `InputFeatureWeights`, `_weights[pair.FlowRef]`, FlowRef+overwrite/перезапись и retention/last lesson; совпадения прочитаны. Точные механизмы двух Mode1 гипотез в этом корпусе не обнаружены.

Дополнительно сверена дельта до `de4ef0f`: [active06][live06], [active23][live23] и [P67][live67]/[P118][live118]. В новых active23 E1/E2 и crosswalk, а также дополнениях active06 точных двух Mode1 recipes не обнаружено. Тела «Самостоятельных сравнений» P67/P118 совпадают с baseline; обновлены policy/паспорт/промпты, включая разрешение исследовать доказанно зависимый consumer/restore без лишнего Train. Поэтому точное покрытие двух механизмов в этой проверенной live-дельте не появилось. Это **полный поиск по указанному baseline плюс ограниченная сверка названных live-документов**, не заявление о независимом повторном чтении всей live-библиотеки.

| Существующее покрытие | Почему не покрывает новые механизмы |
|---|---|
| [Protected45 Mode1 positive consumer][coverage45] | Верно устанавливает, что Mode1 читает chosen. Не проверяет last per-flow input replacement и join selected flow→chosen flow. |
| [P67.2 correction identity][coverage67] | Повтор одного PairId/verified revision; здесь разные IDs, всё допускается trainer/store. |
| [P118.1–5][coverage118] | Mode2/3 content omission, memory on/off, scorer, zero-similarity; Mode1 только положительный контроль. Здесь Mode1 content реально читается, но может быть неверно выбран. |
| N28 ProjectWiki + N52 orchestrator-extensions | Consumer/scorer/admission/schema история известна; exact two-pass FlowRef mismatch и overwrite не обнаружены в прочитанных passages. |
| P15 retention/replay, P16 order, P26 structure | Общие гипотезы недостаточны для покрытия этого кода, способа изменения состояния, контролируемого fixture и final PlanText оценки. |

Статус новых пунктов: **SOURCE-CONFIRMED MECHANISM / QUALITY-HYPOTHESIS / NATIVE-NOT-RUN / PROTECTED-EXPERIMENT-COVERAGE-GAP**. При обнаружении раньше опубликованного точного механизма следует сохранить его авторство и считать этот документ уточнением; не создавать видимость новой находки.

Приоритет исполнения: сначала сохранить уже доказанную доставку `phrase_mappings` и проверить активную learning цепочку пользователя; затем отдельный минимальный RETENTION-M1-01, если текущая задача использует OrchestrationPlanMachine; далее IDENTITY-M1-01 как зависимую от выученной памяти доставку плана. Не запускать параллельно все исторические experiments, не повторять уже отозванный G55. Отдельные report/scorer/cold проверки вне обучения сюда не добавлены.

## 8. Два точных промпта исполнителю

### 8.1. RETENTION-M1-01

> Проверь удержание первого выученного Mode1 плана после второго парафраза того же FlowRef. Используй закреплённый native OrchestrationPlanMachine, три laboratory lessons и четыре frozen queries из раздела5. Два независимых пустых состояния; в обоих один порядок B→A1→A2 и single-pair batches. A — исходный код, B — только token-wise max merge InputFeatureWeights в PromptFlowGroundingModel.Train. Не меняй KNN, остальные поля weights, classifier, scorer, confidence или renderer. Выполни 6 реальных TrainOrchestrationBatchAsync calls суммарно. Экзамен только через GeneratePlanAsync: четыре запроса на empty, afterB, afterA1, afterA2, after cold load final checkpoint; ровно40 outputs суммарно. Cold: SaveCheckpointAsync в файл arm, hash, завершение процесса, LoadCheckpointAsync в новом процессе той же сборки arm, затем экзамен без нового Train; in-process ImportCheckpoint не считать cold. Удержание измеряй между afterA1 и afterA2, acquisition второго урока и все остальные результаты показывай рядом. Запиши PlanText, classifier fields, выбранные PairId/FlowRef, scores, evidence и state hashes до/после. Gold не поступает в learner или renderer. Любое фактически измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен. Публикуй одновременно ухудшения точности, wrong acceptance, latency, RAM и размера состояния. Для реального Train и consumer раздельно измерь Stopwatch elapsed, дельту Process.TotalProcessorTime и независимо peak RSS/working set с указанным методом. Нативный CpuMs — elapsed, PeakRamMb — snapshot managed heap; при отсутствии настоящего измерителя назови их честно, CPU/peak RSS пометь NOT_MEASURED. Аналитические Jaccard из документа не считать runtime результатом. Если native consumer недоступен или этот state не участвует в ответе, статус NOT_RUN/NOT_APPLICABLE с конкретной причиной; не подменяй обучение JS/Python lookup эмуляцией и не добавляй фиктивных уроков. Защищённую папку экспериментов не меняй.

### 8.2. IDENTITY-M1-01

> Проверь, доставляет ли ordinary Mode1 consumer содержимое действительно выбранного flow после обучения новому approval lesson. Используй два laboratory lessons и три frozen queries раздела6, отдельные от RETENTION machines. Два независимых пустых состояния, одинаковый порядок B→A. A — current; B — единственное изменение final FindLearnedPair: ограничить candidates pair.FlowRef==grounding.FlowRef до прежнего Jaccard ranking, при отсутствии кандидата вернуть null и существующий fallback. Не меняй update weights, confidence, classifier, corpus и renderer. Суммарно4 реальных TrainOrchestrationBatchAsync calls. Экзамен через GeneratePlanAsync на empty, afterB, afterA, after cold load: три запроса на каждый этап каждого arm, ровно24 final outputs. Cold: SaveCheckpointAsync и file hash, завершение процесса, LoadCheckpointAsync в новом процессе той же сборки arm без дополнительного Train; in-process ImportCheckpoint не считать cold. Оцени фактическое новое approval действие/порядок в body, а не только согласованность строк FLOW_REF; запиши сохранение B, unsupported fallback, все routing/nearest scores и выбранные identities. Gold не внедряется в consumer. Любое измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен. Покажи одновременно все ухудшения других показателей и цену исправления. Для реального Train и consumer раздельно измерь Stopwatch elapsed, дельту Process.TotalProcessorTime и независимо peak RSS/working set с указанным методом. Нативный CpuMs — elapsed, PeakRamMb — snapshot managed heap; при отсутствии настоящего измерителя назови их честно, CPU/peak RSS пометь NOT_MEASURED. Это обучение памяти с чтением его результата; отдельный zero-update serving опыт не создавай. Source-derived ожидание не выдавай за выполненную native проверку. Если путь недоступен, честно NOT_RUN; proxy эмуляцию и фиктивные уроки не запускай. Защищённую папку экспериментов не меняй.

[grounding-train]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L17-L24
[grounding-build]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L111-L161
[grounding-predict]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L26-L71
[grounding-find]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L74-L83
[knn]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationKnnStore.cs#L16-L66
[machine-train]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L172-L249
[machine-consumer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L99-L131
[machine-state]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L338-L380
[machine-persistence]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L383-L445
[machine-cost-fields]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L225-L245
[live-delta]: https://github.com/lubochka/llm_mvp_core/compare/bf9b543e149ef197da85a767821d98c9b67916d2...de4ef0f14172dada4daf142a4c66ba19e96bb6d4
[live06]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md
[live23]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md
[live67]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/cards/P67-correction-identity.md#L54-L64
[live118]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/cards/P118-projectwiki-content.md#L54-L79
[machine-admission]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L453-L462
[composer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L10-L89
[historical-grounding]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L12-L78
[mode1-corpus]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/OrchestrationCorpusBuilder.cs#L133-L170
[e25-result]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L4-L17
[e25-code]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L17-L43
[e43-result]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json#L4-L17
[e43-code]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L29-L50
[e09-result]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L4-L17
[e09-code]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L73
[e09-labels]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L115
[v210-result]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v210-good-external-plus-internal-guide-full-20260511-014712/internal-train-ready-dpo-summary.json#L10-L22
[v210-code]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L44-L97
[v210-materializer]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L192-L223
[orch-report]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/models/orchestration/orchestration-v1/runner-report.json#L2-L10
[orch-manifest]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/training-data/orchestration/orchestration-v1/training-manifest.json#L8-L17
[ingress-report]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md#L3-L79
[ingress-before]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/raw-outputs.json
[ingress-after]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json
[ingress-rubric]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/inputs/score.cjs#L6-L20
[ingress-scorer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/score.cjs#L1-L13
[ingress-validation]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/evidence-validation.json
[coverage45]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/45-memory-reachability-and-context-consumption.md#L79-L95
[coverage67]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/cards/P67-correction-identity.md#L59-L62
[coverage118]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/cards/P118-projectwiki-content.md#L44-L77


## 9. Дополнение: первичные neural receipts local_wiki_planning и DesignToCode-clean

Дополнительная ограниченная проверка сделана по первичным файлам в `local_wiki_planning@219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` и `DesignToCode-clean@e346b88a3cc1fcce48aefa626bfc7b00509eefd6`. Новых обучения, forward или restore запусков нет. Положительные neural preference результаты подтверждены отдельно от памяти и от общего verdict предметного runner.

### 9.1. Local Wiki: реальная учебная цепочка и точные знаменатели

[ProjectWikiDpoMetricRunner][wiki-native-runner] получает50 train и50 heldout rows; создаёт либо восстанавливает policy/reference; выполняет5 эпох по10 minibatches размера5, то есть50 вызовов `StepMany` и250 предъявлений учебных пар. Затем отдельно вызывает `EvaluateMany(heldOutRows)`, записывает pair diagnostics, `policy_final.bin` и `reference_base.bin`. [DpoTrainer][wiki-native-dpo] вызывает backward и optimizer step для policy; `Accuracy` определяется долей пар с положительным reference-relative reward margin. Это настоящее нейронное обучение preference scorer; метрика не означает правильность свободной генерации.

| Первичный run | Heldout DPO preference | Состояние и публикация | Что подтверждено |
|---|---:|---|---|
| [CodeTrace064 summary][ct064-summary], [diagnostics][ct064-diags] | **46/50 = 92%**,4 отрицательных margin; средний margin0.00442227 | Fresh policy, reference скопирован с неё, initial max delta0; report пишет published | Все100 diagnostics пересчитаны: после обучения train также46/50. Поле summary `finalAccuracy=0.856` — взвешенное среднее по250 предъявлениям на протяжении обучения, не итог46/50. |
| [QueryPlanning134 summary][qp134-summary], [diagnostics][qp134-diags] | **50/50 = 100%**; средний margin0.00172224 | Continuation: policy и original reference из run065; initial max delta0.02539038; report пишет published | Все100 diagnostics пересчитаны, train тоже50/50. Это positive preference на указанном материале; дообучение не следует считать fresh restart. |
| [RequestRouter043 result][rr043-result] | **44/50 = 88%**; heldout margin0.00917352 | Continuation из run034, initial max delta0.15278135; `publication=null` | Отдельный learned routing runner показывает routeAccuracy1 на50 heldout. Его метрика и DPO score — разные измерения; публикация не следует из route accuracy. |
| [RequestRouter092 summary][rr092-summary] и [result][rr092-result] | **33/50 = 66%**; heldout margin0.00094609 | Continuation из run050; `trained_not_published`, предупреждение о DPO accuracy ниже0.75 | Положительный learned preference signal есть; конкретный публикационный threshold не пройден.043 и092 используют разные material/run, их нельзя вычитать как падение одной и той же панели. |

**Отдельное обученное состояние Router043 тоже сохранено.** Первичный `trainingRun` содержит50 cases и snapshot `averaged-token-perceptron-v1`:18 эпох,746 token-weight entries и15 labels/biases; `unitTrace.rowsTrained=50`, `heldOutRows=50`. Это второй реальный recipient — обучаемый lexical router, не DPO neural policy. Он не сводится к одинаковому static keyword switch. В то же время `publication=null` не позволяет считать доказанным применение snapshot продуктовым endpoint. [Snapshot и параметры][rr043-router].

**Source/pair cardinality проверена заново.** Два полных100-строчных JSONL прочитаны и разобраны. В [CodeTrace064 pairs][ct064-pairs]50 уникальных prompts/triples в train и50 в heldout; всего100 разных chosen и100 rejected; exact prompt overlap0. В [QueryPlanning134 pairs][qp134-pairs] —8 уникальных prompts в каждой половине, overlap0, но **один chosen и один rejected на все100 rows**. Следовательно100% QueryPlanning — реальное сохранённое предпочтение одной повторяющейся пары ответов; оно ещё не показывает построение разных планов по содержанию вопроса. Это проверка прежнего наблюдения других агентов, не новая гипотеза.

Для трёх поздних run затраты зафиксированы отдельно: CodeTrace064 —213154.4367ms и peak working set108.8711MiB; QueryPlanning134 —292357.0491ms и102.6992MiB; Router092 —140162.273ms и92.5430MiB. Это разные задачи; не сравнивать их времена как ускорение алгоритма.

**Checkpoint identity.** В CodeTrace064 [policy pointer][ct064-policy] имеет LFS object SHA256 `dfb8c51e0914457eb4635d7408f8ce111f6d1793e7a7d3e814eb31b5fc4de9fe`, а [reference pointer][ct064-reference] — `7c3bfdacb17667843c7b9bc65feb380489106059a3e5eb1d7a89fe4b55b5d77c`. В QueryPlanning134 [policy pointer][qp134-policy] — `2eb4b3d4f13fac67b38571259952a634599b6d33232811de54e8e9259a756f4b`, reference имеет тот же original-reference object. Во всех этих pointers записан размер1,188,744bytes. Проверены сами132-byte LFS pointers и registry metadata, не скачанные tensor payloads. Не заявляется разбор конкретных tensor deltas или runtime-аттестация сборки. Различные object hashes согласуются с разными сохранёнными trained states, но не заменяют проверку реального serving consumer.

В этих дополнительных receipts не найден сопоставимый **отдельно измеренный initial heldout score**. Не выдавать общий `baselinePrimary=0` из orchestration summary за такой score. Первый CodeTrace minibatch действительно сохраняет margin0/accuracy0 при совпадающих policy/reference, но это5 training rows до первого update, а не те же50 heldout. Сохранённый после обучения46/50 — положительный neural результат с указанной областью, без искусственно созданной пары «0/50→46/50».

### 9.2. DesignToCode-clean repeat10: положительная preference и неизменившиеся поздние checkpoints

Первичные [JSON summary][clean-repeat-json] и [таблица всех32 batches][clean-repeat-md] относятся к `base-wiki-session-01-batch-004-repeat10-500-20260515-1100`:500 предъявлений, batch size16,32 minibatches, context96, vocabulary2048; total elapsed102735.5645ms. Source SHA256 — `570FDDE23618EC5FC838D5094E4D7B7B22143A2855AA265C7E4AC9474F87606C`.

`FinalAccuracy=1` — **4/4 последнего minibatch**, строки047–050 десятого повторения; не500/500 и не heldout. `AverageAccuracy=0.86328125` — среднее32 batch accuracy, последнее маленькое batch имеет тот же вес, что16-строчные. Это прямо задано [summary writer][clean-summary-writer]. Final loss0.6813643, margin0.02374749 — сохранённые положительные preference наблюдения.

Сверка metadata двух настоящих model artifacts [step25][clean-step25] и [step32][clean-step32] показывает **один Git blob `bddbe029abcc5f11c5ea70078a4d0ae6aad79b52`**, размер каждого1,188,744bytes. Значит checkpoints на этих двух стадиях побайтово совпадают; различие метрик разных minibatches не доказывает изменение весов между25 и32. Это не исключает более ранних обновлений, не отменяет сохранённый preference score и не переносится на CodeTrace064. Для утверждения «реальное обучение изменило полезное neural поведение» этот clean receipt сам по себе недостаточен; рядом сохраняется более сильный CodeTrace источник, а не стирается положительная история.

### 9.3. Обновлённый дедуп

Корневой агент завершил точечный поиск по **всем555 исходным planning Markdown и248 protected Markdown** закреплённого baseline. Точные механизмы H-M1-RETENTION и H-M1-IDENTITY ранее не обнаружены. Связанные P67/P118/protected45 остаются смежным покрытием; их нельзя считать уже готовыми экспериментами на эти два конкретных механизма.

## 10. Source-only проверка classifier и токенов лабораторных fixtures

Для **всех** lesson prompts и frozen queries обоих новых рецептов [HumanIntentBoundaryClassifier][fixture-classifier] по исходнику возвращает одинаковую существенную форму:

```text
Kind = ImplementationExecutionRequest
AllowedFlowScope = []
ForbiddenFlowScope = []
PhaseScope = []
RequestedFlowTextSpans = []
HumanConstraints = []
RequiresApprovalGate = false
RequiresSaveState = false
```

Причины проверены адресно: prompts не содержат цифр, поэтому flow/phase regex не срабатывают; слово `approval` само по себе отсутствует в списке multiword approval triggers; `review` без `architecture` не даёт ArchitectureReview. Нет остальных skill-load/explanation/migration/phase/testing фраз. Outer/Chosen FlowRef не передаются в `Classify(prompt)`; `LAB-A`/`LAB-B` сами не являются текстом исходного query.

Следовательно `BuildQueryText` не добавляет к этим queries hidden human constraints или scope tokens. Label tokens `lab,a` либо `lab,b` появляются только в том evidence expansion, который явно учтён в таблицах. `OrchestrationKnnStore.Tokenize` извлекает distinct lower-case Unicode letter/number words; все использованные здесь слова латинские. В H-M1-RETENTION исходные lessons имеют4tokens, probes —5, unrelated —4. В H-M1-IDENTITY A содержит45 уникальных tokens, B4, qA4, qB4; расширенный qA содержит11tokens. Это проверка исходника и арифметики fixture, **не native результат выполнения**. Именно эти classifier поля нужно записать в фактическом native receipt, чтобы обнаружить расхождение с исходным ожиданием.

[wiki-native-runner]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L94-L221
[wiki-native-dpo]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/11.DPO/DpoTrainer.cs#L72-L166
[ct064-summary]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/project-wiki-dpo-metric-summary.json#L8-L26
[ct064-diags]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/dpo-pair-diagnostics.jsonl
[ct064-pairs]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/dpo-pairs.jsonl
[qp134-summary]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/project-wiki-dpo-metric-summary.json#L8-L26
[qp134-diags]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pair-diagnostics.jsonl
[qp134-pairs]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pairs.jsonl
[rr043-result]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json#L14129-L14160
[rr092-summary]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-request-router-post-registry-selector-promotion-confirmation-092/dpo-runs/project-wiki-request-router-unit/project-wiki-request-router-post-registry-selector-batch-092-project-wiki-request-router-unit-20260524141757/project-wiki-dpo-metric-summary.json#L8-L26
[rr092-result]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-request-router-post-registry-selector-promotion-confirmation-092/project-wiki-training-loop-result.json#L25-L57
[ct064-policy]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/checkpoints/policy_final.bin
[ct064-reference]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/checkpoints/reference_base.bin
[qp134-policy]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/checkpoints/policy_final.bin
[clean-repeat-json]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-repeat10-500-20260515-1100/internal-train-ready-dpo-summary.json#L10-L22
[clean-repeat-md]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-repeat10-500-20260515-1100/internal-train-ready-dpo-summary.md#L18-L51
[clean-summary-writer]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L133-L154
[clean-step25]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-repeat10-500-20260515-1100/checkpoints/policy_step_000025.bin
[clean-step32]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-repeat10-500-20260515-1100/checkpoints/policy_step_000032.bin
[fixture-classifier]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/42.ProjectWikiDomain/HumanIntentBoundaryClassifier.cs#L12-L92
[rr043-router]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json#L1405-L14127
