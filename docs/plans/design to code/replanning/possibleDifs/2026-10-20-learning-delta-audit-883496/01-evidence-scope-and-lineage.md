# kNN, обучаемые Units и историческая успешность: проверка на core 883496

**20.10.2026. Core:** 883496bca18113bf37e0afe683c75b05a0075048. **Planning:** 6bc4b6fba8f14870f5dc4966cf5a344754962d66. Read-only source audit; новых native learning/quality/cost runs нет. Production и protected docs/plans/replanning-possible-diffs не изменялись.

## 1. Что установлено

1. **Реальные положительные исторические результаты присутствуют.** E25 обучает POS-kNN память и получает 0.3438 → 0.875 на 32 синтетических проверочных примерах. E43 улучшает retention 0.3226 → 0.3978 на oldVal своей count-модели. E22 имеет малый положительный macro-F1: 0.4884 → 0.4915, несмотря на записанное passed:false. По нынешнему пользовательскому правилу это успех именно macro-F1 этого домена; сопутствующее ухудшение времени остаётся в отчёте.
2. **Сохранённый код найден в соседних ветках.** В 16 адресных чтениях четырёх файлов на четырёх branches не найдено потери рассматриваемых алгоритмов. Первые три ветки отличаются от current главным образом copyright header; PrepareToGPL у E09/E43 меняет comments/diagnostics. Это не доказывает, что текущий ordinary consumer использует успешно обученный механизм.
3. **Положительный E09 tier2 нельзя превратить в чистый matched quality delta.** Baseline считает весь val; enhanced берёт первые 500 узлов. Более короткий tier1 использует совпадающие 193 перехода и тоже содержит положительное наблюдение 0.3368 → 0.3472. Per-file ordinal targets тоже меняют значение. Положительные raw numbers сохраняются, но причины требуют отдельного выровненного опыта.
4. **Новый ограниченный кандидат SKNN-SIBLING-PREP-01** относится непосредственно к стоимости настоящего обучения StructuralKnn: повторное построение одного sibling list при каждом child. Проверяется whole TrainStructuralKnn, native state и consumer; не самостоятельная инфраструктурная проверка.
5. Большинство иных обнаруженных нами деталей уже описаны другими агентами. Они сохранены ниже как независимая source-проверка и сопоставление, а не выданы за новые гипотезы.

Источники всех чисел и точные ограничения — следующие разделы. Отсутствие runtime assembly SHA в архивном JSON означает, что здесь подтверждены **сохранённый run receipt и соответствующий source lineage**, а не заново воспроизведённый бинарник.

## 2. Исторические обучения: факты, собственная задача и регрессии

| Опыт | Архивный полезный показатель A→B | Сопутствующая стоимость | Что действительно менялось и читалось | Предел вывода |
|---|---|---|---|---|
| E25, 01.05.2026, tier 1 | top1_accuracy 0.3438 → 0.875, +53.12 п.п.; hard0.8571,28 cases |0,4 → 15,4 мс, ухудшение; это inference interval, не обучение | PatternClassifier.Train сохраняет 128 labelled POS rows; Predict читает память, Jaccard POS-bigram sets, k3 |160 synthetic examples,32 val. Это pattern classification; не +155% полного перевода, не LoRA |
| E22, 30.04.2026, run 002, tier 3 | macro-F1 0.4884 → 0.4915, +0.31 п.п. (+0.63% относительно baseline), old passed:false |0,1 → 102,4 мс, ухудшение; enhanced timer после Train | PredicateDatasetRouter.Train на 212 examples, Predict на 53 тех же val; baseline top3 frequent labels |Слабый положительный run выбранной auto-labelled панели; без утверждения статистической устойчивости или полезности downstream dataset selection |
| E43, 01.05.2026, tier 2 | retention_accuracy 0.3226 → 0.3978, +7.52 п.п.; initial old model.3440 |0,1 → 5,8 мс, ухудшение; intervals включают разную preparation+fit+eval |Новая таблица bigram counts по 414 new pairs против новой таблицы new+177 sampled old; одинаковый oldVal |Retention count-модели, не продолжение тех же neural weights. «Old/new domain» — половины файлов, не доказанные CODE_Q/SECURITY |
| E09, 30.04.2026, tier 1 | top1_accuracy 0.3368 → 0.3472, +1.04 п.п. по округлённым полям | 0.5 → 54 ms, ухудшение | 773 учебные пары; обе руки оценивают 193 одинаковых перехода | Полезное узкое наблюдение; общие проблемы per-file labels и split сохраняются. Не объявлять все E09 runs невалидными только из-за tier 2 |
| E09, 01.05.2026, tier 2 | сохранено 0.2413 → 0.3507, reported +45.33% relative |14.5→763.5ms; неравные числа eval queries |kNN Train на 4492 vector/label pairs; baseline count table |NONMATCHED_COMPARISON: baseline 1123 transitions, enhanced 499; local per-file target IDs. Не чистый эффект structured features |

### 2.1. E25: успешная память, которую нельзя заменить статическим условием и считать тем же learner

[Архивный JSON](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) и [runnerL22–48](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L22-L48) явно отделяют Train от двух eval intervals. [GlobalUsingsL11](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/GlobalUsings.cs#L11) связывает alias SentencePatternClassifier с SemanticCompiler.PatternClassifier.

[Native writerL39–43](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Translation/PatternClassifier.cs#L39-L43) очищает и наполняет _train; [readerL46–55](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Translation/PatternClassifier.cs#L46-L55) ранжирует эту память; [distanceL78–86](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Translation/PatternClassifier.cs#L78-L86) читает порядок соседних POS через биграммы. Отсюда learning link явный, хотя обучение memory-based, без gradient.

В current переводческом пути [TranslationSemanticTokenizer.ClassifyPatternL351–361](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationSemanticTokenizer.cs#L351-L361) остаются deterministic first-verb условия. [SemanticTokenizerUnit.TrainOnExampleL123–140](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationTrainedUnits.cs#L123-L140) учит точные/signature corrections; [TokenizeL89–120](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationTrainedUnits.cs#L89-L120) их читает. Это реальное иное обучаемое состояние, с иной генерализацией. Нельзя называть его «вообще не обучающимся», но нельзя считать автоматическим восстановлением E25.

Этот gap уже известен: [предыдущий N50](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-EXTERNAL-DOMAINS-E25-NOVEL-POS-SEAMS-2026-10-19.md). Его label ontology mismatch, короткие POS false-distance0 и Train.Clear forgetting тоже не переобъявляются новыми.

### 2.2. E22: положительный F1 при old failed — не повод удалить результат

[JSONrun002](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-04-30-002.json) содержит положительный delta вместе с passed:false и TUNE_OR_ABANDON. Эта старшая классификация не соответствует текущему правилу успеха по каждому полезному показателю. Сохранить положительное наблюдение и показать ущерб времени, не переписывая старый JSON.

[RunnerL45–76](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L45-L76) строит **top3-frequency baseline**, хотя headerL14 всё ещё говорит «все datasets». [EnhancedL79–103](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L79-L103) читает ту же val; train вне enhanced interval. [Native routerL25–47](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L25-L47) связывает writer Clear/AddRange с reader kNN voting.

Показатели по labels в receipt: Generic1.00,CSharp.99,BugFix.96; TypeScript,Orchestration,Tests0.00. Это предупреждает против переноса положительной macro-F1 на каждый домен. AutoLabel source rules — отдельная известная ось качества данных; здесь source-label procedure не исправлялась. Старый run001 той же серии.5→.5 не обладает F1-улучшением; нельзя выбирать только удачный run и замалчивать нулевые соседние результаты.

### 2.3. E43: реальная полезная retention, но другая модель и способ продолжения

[JSON](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json), [split/learningL23–56](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L23-L56), [count writer+readerL89–109](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L89-L109) показывают настоящую обучаемую таблицу и actual accuracy. Разница полезна, даже если initial old model был другим.

Обе руки **строятся заново**, а не начинают дальнейший gradient update из baseModel. У enhanced больше учебных предъявлений:414+177. Поэтому результат не разделяет эффект старых знаний, общего числа пар и конкретного sampling. [ExtractPairsL78–86](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L78-L86) конкатенирует predicates всех файлов до Zip и создаёт межфайловые переходы — уже известный источник загрязнения. Не распространять успех на current AutonomousTrainer DpoPairs: там отдельный learner/route и replay aperture.

### 2.4. E09: что сохраняем и что требуется перемерить

[Архивный tier 1](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json) содержит 968 узлов, 773 учебные пары и 0.3368 → 0.3472. При таком размере validation194, обе оценки используют те же 193 перехода: ограничение Take(500) ещё не сокращает панель. Это отдельное положительное наблюдение, которое нельзя удалить общим выводом о tier 2. Оно всё равно не устраняет общие ограничения labels/split и не доказывает улучшение законченного кода.

[JSON](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) фиксирует 5617 nodes и 4492 training pairs. [СравнениеL30–78](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L78) отсюда даёт train 4493 nodes, val 1124 nodes, baseline 1123 transitions, enhanced 499. [BuildTokenVectorsL107–120](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L107-L120) нумерует SyntaxKind по отдельному словарю каждого файла, хотя имя поля BpeId вводит в заблуждение: это не обучение text-BPE.

**Исторический перенос подтверждён первично:** [commit27ae032](https://github.com/lubochka/llm_mvp_core/commit/27ae03266ae2f7300d63e915795c0ff62369fa67) добавляет native StructuralKnn.cs (+225lines), удаляет experiment StructuralKnnPredictor и StructuralVectorExtractor. Квитанция: [e09-transfer-commit-receipt.json](evidence/e09-transfer-commit-receipt.json). Это перенос **экспериментального kNN в production**, не доказательство того, что некогда неудачная neural matrix сама превратилась в kNN.

Current [TrainStructuralKnn](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompilerFacade.cs#L206-L225) использует within-file pairs и NodeTypeId%200. [ConsumerL299–311](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompilerFacade.cs#L299-L311) opt-in и требует≥10rows. [Options](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompilerOptions.cs#L14-L15) defaultfalse. В данном аудите actual RP01 consumer этих predictions до законченного ответа не установлен; нельзя обвинять этим компонентом отдельный DPO Student.

Обязательная будущая quality-проверка E09: одинаковая file-disjoint exam panel, одинаковые labels, count/abstention denominators, split без cross-file transitions, native Train и собственные predictions. Это **уже существующая** ось восстановления, не новая карточка SKNN.

## 3. Что найдено в четырёх соседних ветках

Проверены exactrefs:

- external_domains3036a01acec938f0b83fa4ffbda26202cc35d639;
- language_translatef6032aff81f1f83de7c09cf9b9efe6b058dd12b9;
- local_wiki_planning219d7ba89dc3ef43e03e17f1461ad96154d3dd7a;
- PrepareToGPL31346d8928be4469b01aaf313780e8f950ff91e2.

Каждая branch: E25PatternClassifier.cs, semantic PatternClassifier.cs, E09StructuralKnn.cs, E43ReplayBuffer.cs. Git blob SHA из connector metadata сохранены в [knn-history-branch-source-receipt.json](evidence/knn-history-branch-source-receipt.json). Первые три branches имеют одинаковые blobs: E25cf37fb7…, classifier7d57255…, E09a11af3d…, E435de76a2…. PrepareToGPL E25/classifier побайтно равны current blobs246d265…/4d7a4c6…, E09/E43 отличаются текстом комментариев/сообщений. Сравнивались полные materialized тексты; локальный завершающий LF не выдаётся за исходный Git blob hash.

[Donor E25](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E25PatternClassifier.cs), [donor classifier](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs), [wiki branch E43](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs), [PrepareToGPL E09](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E09StructuralKnn.cs).

Эта выборка подтверждает сохранность этих четырёх алгоритмов. Она не равна полному поиску всех веток, checkpoint, удачных trainruns или каждого domain. Восстанавливать надо recipient/representation/consumer path и измеряемую задачу, а не просто копировать тот же файл обратно.

## 4. Дедуп других обнаруженных деталей: известные, не новые

| Source-confirmed область на current pin | Learning link и ограничение | Уже существующий разбор |
|---|---|---|
| Tokenizer exact corrections keyed raw text без SourceLang; corrected signatures теряют часть rules | TrainOnExampleL123–140 → maps → TokenizeL89–120; shared directions дают условный cross-language риск |26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY |
| Executor re-tokenizes new rule tokenizer для prefix/structure при обучении |TrainedUnitsL420–422 пишет keys, отличные от trained tokenizer |28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT |
| Exact/phrase/word maps предшествуют kNN |ExecuteL264–297; исправленная memory может не стать выбранным ответом |56-01-TRANSLATION-KNN-CONSUMER-NEW-CAUSAL-SEAMS |
| Context verifier rule writer хранит src+ctx, reader проверяет только src; parser roles зависят от enum order |TrainL1095–1145 → rules; VerifyL1056–1080; parseL1216–1246 |23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE,28-NEW-VERIFIER-ISSUE-ROLE-PARSER |
| Gender alternate обучает -л/-ла без POS guard; phrase fallback использует несжатый индекс |Executor writerL427–450, genderL774–795, phraseL798–858 |27-ROLE-GENDER и ранее описанный compacted-token index, не новая находка |
| kNN take before direction filtering / insertion-order correction |TranslationUnitContractsL246–292; ExecutorL333–364 |37 cascade starvation,16 correction visibility |
| Method train gate смотрит count methods, а память хранит только valid adjacent same-file pairs |FacadeL178–190 → MethodKnn.TrainL120–133; sparse replacement может очистить старое |LT-N02 / KH-EXT-01 |
| FileAction дважды разбирает тот же source перед Train |FacadeL194–203 → FeatureVector+Label → native Train |KH-COST-01, не новый SKNN owner |
| E09 SiblingCount не используется в distance; ordinal cap255; position внутри ArgumentSyntax не есть argument ordinal |StructuralKnnL15–35,52–59,rolemapper; native Train/Predict |N35/P26.4,N53 NEW-POSITION,N57 ARG |
| PromptFlowGroundingModel._weights[FlowRef] last-write и расхождение nearest pair/flow |TrainL17–24 → _weights/_knn → TryGround/FindLearnedPair |61-LEARNING-FIRST-AUDIT, не присваивается |
| ProjectWiki request router key unigram/bigram collision, repeated featurize, snapshot label-order |Реальный perceptron Train → weights → Predict |WIKI-FEATURE-KEYS-01,WIKI-FEATURES-ONCE-01,WIKI-TAXONOMY |
| Replay global capacity и domain-only sampling |ReplayBuffer.Add/Sample → AutonomousTrainer → IncrementalTrainer |N63 replay-capacity и прежний replay aperture |

Это карта уже известных owners, а не утверждение, что каждый дефект причинно объясняет RP01. Особенно не допускается новая карточка, которая только меняет метаданные, unused weights или output не участвующего learner.

## 5. Подготовленный рецепт, дедуплицированный перед публикацией

**SKNN-SIBLING-PREP-01: REFERENCE_ONLY, тот же фактор, что существующий AST-SIB-CACHE-01 из свежего planning9d311.** Отдельного места или запуска нет. Ниже параметры сохранённого альтернативного design; canonical AST имеет другой бюджет4 TrainFromSource/128 Predict. [Согласование](18-concurrent-reconciliation.md). Исследуемое различие: один и тот же parent.ChildNodes список и линейный IndexOf восстанавливаются для каждого учебного child. Возможное снижение wall/CPU/allocations при сохранении exact trained state. Уже определены:

- полный future source-copy recipe, original blobs, минимальный host net9.0/Roslyn4.12.0;
- фиксированный будущий Windows workspace contract и короткие obj/bin;
- source-extracted whole TrainStructuralKnn и native reader, полная копия StructuralKnn;
- per-call ReferenceEqualityComparer cache с проверкой runtime equality contract, без persistence или cross-lesson reuse;
- fixed4 TRAIN + 8 EXAM, 2 Train total, 32 facade queries; no warmup/retry;
- no-update BEFORE, AFTER own predictions, ordered vector/label state hash, CPU/time/allocations;
- явные parity/failure/UNKNOWN, STOP/NEXT, отчёт/кандидат на русском и отдельный executor prompt.

Ссылки для публикации в новом пакете: 06-SKNN-SIBLING-PREP-01.md и 07-SKNN-SIBLING-PREP-01-executor-prompt.md. Рабочие документы этого агента имеют имена SKNN-SIBLING-PREP-01-card.md и SKNN-SIBLING-PREP-01-executor.md.

Причинная граница: настоящий feature preparation → memory mutation → own reader; **NOT_RUN**. Исторический E09 timing нельзя использовать как ожидаемую экономию: BuildTokenVectors вызывается **до обоих его timers**. Новый опыт специально охватывает целый prepare+Train.

## 6. Квитанция относительной новизны и связь с protected карточками

Проверены **747 planning Markdown и 172 materialized protected Markdown** на точных pins. Это полный полученный Markdown corpus в названных каталогах, а не все файлы GitHub/все private agents. Квитанция queries, точных matches и corpus hash: [knn-sibling-novelty-receipt.json](evidence/knn-sibling-novelty-receipt.json).

Поиск по механизму, не только ID:

| Поиск | Найдено | Семантическое решение |
|---|---:|---|
| Новое имя SKNN-SIBLING-PREP |0files |Само по себе не доказательство новизны |
| StructuralVectorExtractor / TrainStructuralKnn |19files,23lines |Ранее caller/label/domain/feature gaps; не sibling prep cache |
| ChildNodes / siblings.IndexOf |3files,5lines |N35 ignored feature,N53 position cap,N57 argument ordinal |
| sibling/childnodes/сосед рядом с cache/кэш |5files,5lines |Ложные соседи текста: другие caches, branch provenance, E30 и reference cache |
| quadratic/квадратичная preparation AST /O(d²) |0files |Нового cost-вмешательства не найдено |
| KH-COST / double AST parse |24files,46lines |Тот же класс полезной cost цели, **иной FileAction writer/feature pair и иной фактор** |

Protected P26.4 изменяет distance для SiblingCount; он не измеряет экономию той же native модели. Наличие широкой P26 карточки не покрывает наш exact factor/owner/budget. KH-COST-01 устраняет два Roslyn parse для FileAction training row; наш B сохраняет один parse каждого Structural training file и убирает повторную sibling enumeration внутри него. Первоначальный **BOUNDED_NEW относился только к этому раннему корпусу**. После чтения planning9d311 найден AST-SIB-CACHE-01 с тем же фактором; финальный статус SKNN — REFERENCE_ONLY, новизна и второй запуск отменены. [Сравнение](reviews/concurrent-SKNN-reconciliation.md).

Остальные строки раздела 4 в protected/possibleDifs уже представлены и не требуют повторных учебных прогонов ради этой сверки. Полный audit пригодности всех 153 карточек выполняется отдельно; данная записка не выдаёт локальную тематическую сверку за завершённую глобальную проверку.

## 7. Что не делалось и что остаётся решением пользователя

Ни обучения, ни native .NET build, ни новых probes, ни модели/данных/checkpoint изменений; только чтение GitHub sources/results и подготовка документов. Старые failed attempts не удалялись. Current выполненные первые 4 эксперимента не перезапускались, положительный BPE результат не заменялся старым рассказом об ошибке пути.

Новый эксперимент ещё не успешный candidate по измерению: это **готовая проверяемая гипотеза**. При будущем полезном улучшении любой метрики записать candidate со всеми регрессиями, оставить пользователю выбор полной product-цепочки. Архивные E09 tier 1 / E22 / E25 / E43 уже содержат полезные положительные показатели, которые следует сохранять в исторических кандидатах с их доменными и методическими границами.
