# N54 — короткие domain-first эксперименты: вернуть Gold DesignToCode-clean, а не повысить число тестов

**Назначение:** исполнимый пакет для CODEX/Kiro при следующей кодовой сессии. Все опыты **NOT_RUN** в этом аудите. В текущем проходе выполнены лишь read-only Git SHA/source checks. Не менять исходные 500 DPO пар, чужие frozen verdicts, checkpoint и corpus, пока опыт не укажет точный дефект. Править только разрешённый repo и только после отдельного согласованного implementation work order. Публиковать evidence в `xiigen-planning/llm_plan3/.../replanning/possibleDifs/`; `llm_mvp_core/docs/plans/replanning-possible-diffs/**` **read-only**.

## P0-FIRST: GDC-A0 и GDC-A1 — прямой gold/current сигнал обучения DesignConversion

**Это найдено в параллельной работе, не новое открытие этого пакета.** [GDC gold→current source crosswalk](54-DESIGNTOCODE-CLEAN-SOURCE-PARITY-AND-NEW-GAPS-2026-10-19.md) и [исходный полноправный GDC-A0–A7 protocol](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) имеют приоритет для продукта Design-to-Code. Самостоятельно проверено по [gold source](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L1487-L1520) и [current source](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2937-L3003): раньше `visual_score` был в chosen/rejected, теперь только html/css, а local learner при отсутствующем поле ставит +1.0 для любых разных сторон.

**GDC-A0 — короткий однофакторный негативный/положительный опыт:** два дизайна с известной высокой/низкой визуальной оценкой, инверсия trusted scores без смены HTML/CSS, equal-score контроль и exact-equality контроль. A current `ToCanonicalTrainingItem`→authorized local Machine; B same HTML/CSS + **server-owned typed admitted `chosenScore/rejectedScore` sidecar**, невидимый DPO Student prompt; C B cold checkpoint restore → обычный DesignConversion consumer; D no training state. Freeze dataset IDs, source SHA, feature+checkpoint schema and real caller. Измерить `preferenceMargin`, signed local deltas, correction category and **blind visual defect fix rate** на 4–8 source-disjoint HTML/CSS designs. Если current A всё ещё использует прямой allowed score sidecar по другому caller, основной отрицательный forecast опровергнут; trace first. **No native run yet.**

**Встроенный falsifier:** сравнить одинаковый `LearnedWeightCheckpoint` после B(+0.8) и B(-0.8) *при неизменном chosen/rejected HTML/CSS и feature map*. Исходный код как gold, так и current делает `preferenceMargin` отдельным отчётным полем; `AddDelta` от него прямо не зависит. Если sign-swap меняет только `RewardMargin`, а не сами feature contributions и cold output, **score-sidecar repair недостаточен по определению**, даже если renderer отчёта теперь показывает честные числа. Это унаследованная ограниченность old local-weight learner, **не branch regression**. Дальше необходим **authorized feature-level learning formulation** с проверяемым signed signal, а не адаптация теста под report. См. [peer analysis](54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md).

**GDC-A1 — конкретный Improver:** добавить две честно размеченные категории `contrast` и `overflow` *только* через legal typed metadata, а не text label leakage; выбранный `AddCorrectionCategoryDeltas` должен получить ровно нужную категорию в correct Unit ID. Проверить одно- и двухдефектные страницы и отказ при отсутствующих категориях. Дальнейшие проблемы 16D vector truncation, DOM parent relationship, learned top-1 instead of all matching defects описаны в чужом GDC-A2–A4 и не приписаны себе.

**Сохранить защищающие нововведения:** current `TrainBatchAsync` требует server-owned authority, `TrainAdmittedBatchAsync` является реальным внутренним разрешённым путём; не откатывать в небезопасный gold API. Видимая смена local margin **не** равна эффекту `11.DPO` или free B0 Student. Перейти к GOLD-DC-01/02 только после установления реально вызываемого local recipient и accepted authority.

---

## P0: GOLD-DC-01 — самый быстрый опыт оркестрации, у которого есть реальный положительный исторический score

**Домен:** формирование многошагового плана/Design-to-Code/ProjectWiki. **Получатель:** `OrchestrationPlanMachine` с реально загруженным `models/orchestration/orchestration-v1/orchestration-v1-checkpoint.json`, затем обычный production-like caller. Исторический [report](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/orchestration/orchestration-v1/runner-report.json): learned-flow-grounding 1.0, unseen-style 0.8 — прошлое измерение, **не** обещанный текущий результат. Старый и новый assets имеют одинаковый Git blob SHA.

**Панель:** 12 заранее выписанных задач; 6 разных новых формулировок известных цепочек client→API→storage и 6 новых комбинаций, которых не было среди train/source examples. Для каждой заранее независимый judge помечает обязательные роли и порядок; контроли — поменять стек, изменить причинное условие, удалить данные из контекста, добавить несущественное переименование. Нельзя проверять только `checkpoint_loaded=true`.

**A/B/C/D при одном asset и одних case IDs:** A gold direct machine; B current direct machine; C current ordinary consumer; D пустая машина без подгрузки — отрицательная граница. Замер: checkpoint path SHA, stage `load / select flow / unit / output`, learned state pattern IDs, exact final text, semantic accuracy for edges/roles, hallucinations, latency, источник каждого перехода. Если A=B, C≠B и D≈C — подозрение на **lost serving recipient**, а не на невозможность учиться. Если A≠B — локализовать input/tokenizer/config/restore. Если A=B=C — эту часть gold **не ремонтировать**.

**Good:** сохранённый learned pattern участвует в конечном плане на новых входах. **Bad:** детерминистический дефолт построил такой же план и его ошибочно сочли обучением. **Stop:** источник gold trace неизвестен; нужны два actual caller paths, а не долгое CI.

## P0: GOLD-DC-02 / N54-M1 — восстановление переводческого asset и типизированный manifest

**Домен:** code-edit instruction RU→EN (содержит `ValidateToken`), второй контроль EN→RU. Это маленькая способность обучения конкретного `ExecutorUnit`, не обобщённый ENRU score. Эталон [gold report](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/translation/translation-v1/runner-report.json) хранит exact + non-exact KNN-контекст, `rows_trained=1` и `runtime_product_accuracy=1/3`.

**Входы и независимые оценки:**
- E1 exact: `add null check ValidateToken` → допустимое действие добавить null check к методу `ValidateToken`.
- E2 paraphrase: `add a null check before ValidateToken` → то же требование, но НЕ exact-source memorization.
- E3 changed intent: `remove null check from ValidateToken` → **не должен** возвращать прежний `Add a null check...`.
- E4 changed identifier: `add null check ValidateOtherToken` → прежнее имя функции должно НЕ протечь.
- E5 different language direction: EN→RU и русский ввод при правильном `SourceLanguage/TargetLanguage`. Никакого перевода на 100% из EN→RU seed без реальной памяти не ожидать.

**Плечи с одинаковыми E1–E5:** A `DesignToCode-clean`: прямой `TranslationMachine.LoadCheckpointAsync`; B current: прямой `LoadCheckpointAsync`; C current: `TranslationModelAssetStore.LoadAndValidateManifestForPublishAsync` на замороженном legacy manifest + реальном verified training artifact; D current: C после законной миграции старого `schema_version=1` с доказательством происхождения без изменения checkpoint bytes, потом `PublishAsync` + ordinary `TranslationRoutingFlow`. Если C отказал `TRANSLATION_MODEL_MANIFEST_INVALID`, **не исправлять отключением валидации**. Нужна миграция/отдельное grandfathered read-only legacy restore с ясным статусом `unverified for new publication`.

**Дополнительный контролируемый 2×2:** old stored learnt correction present/absent × valid current manifest present/absent. Это позволяет отделить **научное улучшение state** от **безопасной доступности asset**. Сохранять `Negative DPO side` / legal source lineage даже если gold Unit записывал только `chosen`.

**Pass:** B совпадает с A по exact + paraphrase + scope-negative либо есть локализованная причина; D действительно загружает *те же* обученные bytes и вызывается. **Fail:** unchanged bytes+policy hash дают иной ответ без source trace; отказ C/D без объяснения; старая correction превращена в rule hardcode.

## P0: N54-M2 — за 15–30 минут доказать/опровергнуть checkpoint↔corpus provenance

**Домен:** перевод, metadata/learning provenance; этот эксперимент специально **без новых training steps**. Два различных разрешённых набора A/B (в каждом настоящий legal/training metadata + собственная SHA): A содержит seed «add null check ValidateToken», B содержит «remove null check ValidateOtherToken». Один и тот же frozen checkpoint C.

**Шаги:** (1) Подготовить A и B в isolated temporary roots, используя существующие настоящие corpus writer/validator; не менять tracked training files. (2) `LoadAndValidateArtifactAsync` обоих. (3) `TranslationModelAssetStore.ExportAsync(options.CheckpointPath=C, options.TrainingDataArtifactRoot=A)` и аналогично B, каждый в отдельный temp output. (4) Сравнить `TrainingDataEvidence.CorpusSha256`, `checkpoint_sha256`, machine outputs. (5) проверить, существует ли **дополнительный** наружный caller/admission step, который отклоняет B как не источник C.

**Предсказание из текущего ExportAsync body:** оба независимо валидных legal corpora могут получить валидный manifest вокруг одинакового C — это **conditional source-level concern**, не обвинение в том, что случилось в настоящем run. **Фальсификатор:** существующий upstream signed training receipt обязателен для *всех* production callers и проверяет bind C↔A; или текущий writer физически запрещает такую пару. Зафиксировать доказательство caller guard. Положительный результат эксперимента должен привести к требованию `checkpoint_sha ↔ actual training input sha ↔ gradient journal ↔ legal admission ID`; не к «добавим ещё поле для галочки».

**Good:** несоответствующее происхождение блокируется до публикации. **Bad:** произвольный подтверждённый корпус объявлен доказательством обучения произвольного checkpoint.

## P0: N54-M3 — за 5 минут разобраться с двумя corpus hashes в gold

**Домен:** архивная воспроизводимость перевода. **Никакого learning claim из совпавшего хеша**.

~~~bash
sha256sum training-data/translation/translation-v1/corpus.jsonl.gz
gzip -cd training-data/translation/translation-v1/corpus.jsonl.gz | sha256sum
~~~

**Два опубликованных значения:** runner-report = `53559e02f6f0a5eb54d85e6e424afb97c8bd1d364f5a7c8aa10c891430abd9ac`; training-manifest = `dac67097f8f4231122644c2b8e2a97e9ab857e3cd80fa5d0d47a620df02642bf`. Дополнительно вычислить SHA того *прошлого исходного набора*, который читал конкретный regen runner, если первые два не совпали. Назвать, какая длина/байтовое представление хешировалось, либо `UNKNOWN`. Не редактировать historical receipts. Если hash scopes различны — не классифицировать как corruption; документировать scope и проверять происхождение перед новой миграцией.

## P0: GOLD-DC-04 — только предметно применимые алгоритмы

| Domain / опыт | Инпуты, набор | Сравниваемый единственный фактор | Замер и допустимое улучшение |
| --- | --- | --- | --- |
| **C# / AST structural kNN** | 10–20 methods из **разных файлов/проектов**, unseen SyntaxKind combinations, rename/negation/nullable | kNN без AST / с AST (общий stable kind-codec и одна split, identical k/compute), затем actual code/documentation consumer | Paired correct structural prediction, edge-cases, actual C# explanation/build if relevant, latency. Нельзя переносить per-file lexical enum индексы в общий feature-vector |
| **RU→EN / POS-context memory** | 10–15 неоднозначных лексем; до/после контекст, роль subject/object, парафраза; unseen morphology | POS/role/context off/on в *learned* store, отдельно old `BuildDefault` deterministic E26 контроль | Значение соответствует контексту; перестановка слов приводит к правильной смене либо сохранению смысла, no wrong hard veto при UNKNOWN. E26 40→100 на 15 заранее заданных примерах не доказывает этот outcome |
| **EN→RU / порядок+отрицание** | 12 matched минимум: `box is empty` / `box is not empty`, `room was empty` / `room is empty`; unknown nouns | learned kNN without law / law-aware filter (current `TranslationMemorySourceLaw` + cache consistent) | Opposite polarity not retrieved, coverage/abstain before/after, semantic accuracy. Law должен ограничиваться поддержанной грамматикой |
| **SQL / predicate law** | 10 независимых запросов: `IS NULL` vs `= NULL`, JOIN grouping, correlated subquery, `NOT EXISTS` | learned retrieval: bag-of-tokens vs predicate tree/AST-aware features; no hard SQL veto outside supported dialect | Executable SQL oracle for 4 small fixtures + meaningful explanation on others, false positive and compute budget |
| **Design-to-Code / cross-stack dataflow** | 8 задач с client/server+API+DTO+db+UI return + 4 changed one-node counterfactuals | learned Flow correction off/on (frozen Unit state), deterministic planner unchanged | Correct mandatory edges, propagated contract changes, whether learned result actually chosen by ordinary graph/consumer |
| **Orchestration / graph+replay** | 2 коротких batch по 6 учтённых разных task families, 6 старых и 6 новых evaluation tasks | replay off/on with same old parameters, same budgets and chronological training; compare gold Unit memory separately from neural LoRA | Old-retention, new-acquisition, cross-domain negative transfer, exact checkpoint restore, latency |
| **Mixed domains / Student RP01** | 8–32 обучающих contrast pairs + sealed 3–6 unseen families; legal teacher provenance | random DPO vs **warm-base SFT** vs warm-base+DPO; exact steps/seed/grad mask | Token overfit **и** free semantic output отдельно; если 8–32 train tokens не учатся, не запускать следующие 500/1000 pairs |

Для kNN использовать также factorial **neither / deterministic law only / learned similarity only / both** на одних и тех же данных: «было больше правильных ответов» не должно объясняться только фильтрацией без обученного вклада. **Доменные признаки не глобальны**: AST-дерево C# не канон русского перевода; порядок/POS важны в переводе, но bag of words может быть достаточен для узких tag-based задач; NULL-логика SQL отличается от C# nullability. При неизвестном контексте закон должен возвращать UNKNOWN/abstain, а не выдуманную метку.

## Архитектурная мини-матрица: DPO → recipient → checkpoint → actual answer

Перед любым Train/Measure работник заполняет эти строки — **не дополняет наугад**:

| Слой | Проверить | Неприемлемый вариант |
| --- | --- | --- |
| Teacher и pair | обе стороны с законным происхождением; независимый human/CODEX choice; chosen семантически лучше rejected, неизменённые pair_id и source | две одинаковые стороны; только более длинный ответ chosen; запрещённый корпус |
| Tokenization | `Prompt/Chosen/Rejected` bytes, BPE language segmentation, EOS ID, unambiguous answer-only mask; local vs shared vocab mapping; freeze tokenizer hash | нарезать старую пару Compact480, считать truncation harmless, переучить tokenizer между плечами |
| Neural LoRA | один базовый checkpoint, frozen reference, exact DPO sign and gradient, named trainable param keys, consistent optimizer+RNG+dropout replay | потерять optimizer state или изменить reference во время continuation; считать Δweights доказательством gain |
| Typed Unit | `pair_id→UnitId/Level→learned keyed state`, evidence of accepted negative/warning; actual one→many / many→many consumer | копировать первый nearest Output и объявить обучением всех named Units |
| Flow / Orchestrator / DynamicGraph | input roles→candidate edges→multi-branch/authority→typed output, learned correction used vs deterministic baseline | pass through hardwired default и присвоить quality эффект LoRA |
| Publish / restore | original checkpoint sha, model revision, tokenizer/adapter/schema manifest, legal trace, exact served version, cold restart | успешная file-copy без фактического загрузившегося learned state |
| Evaluation | paired heldout by source family, ordinary output, independent semantic rubric, coverage/abstain/critical harm, latency+RAM | оценить training loss на тех же 500 template variants как «обучение на новых законах» |

## Формат конкретного work order исполнителю (одно изменение за раз)

**Задача:** GOLD-DC-02 + N54-M1. **Read-first:** старая manifest/schema, текущий `TranslationModelManifestContract`, `TranslationModelAssetStore`, `TranslationMachine.LoadCheckpointAsync`, actual published-index consumer; caller/consumer list до tests. **Skills:** architect-behavior-classifier, phase-planning, plan-review, generated-code-review, experiment-design, code-examination, prompt-patch-authoring, human-readable-architecture-audit, human-communication. **Method:** (i) frozen gold/current direct compare; (ii) legal artifact validator; (iii) schema mismatch evidence; (iv) v1 migration plan with cryptographic identity; (v) only if quality regression proven one narrowly scoped code patch; (vi) rerun same 5 case IDs and publish raw evidence. **WRONG:** отключить `IsValidTrainingDataEvidence` и назвать старый файл совместимым. **CORRECT:** подтвердить исходную model/corpus/teacher identity, мигрировать схему с тестами negative tamper и загружать старое обученное состояние только под проверяемой legal authority. **Acceptance:** unchanged original SHA, restored actual method response, two source-disjoint unseen outputs, cold-load exact model revision, no unsupported provenance, explicit FAIL if not possible.

**Time/risk stop:** один микропилот = небольшой кодовый harness и до 20 заранее объявленных примеров, короткий run, без долгого Windows workflow. Если мини-опыт не может установить настоящий consumer/этический источник — **BLOCKED_BY_SOURCE**, не «0 quality». При отсутствии gold/current разницы пометить `FALSIFIED_AS_BRANCH_REGRESSION` и переходить к следующему фактору. Исторические результаты не переписывать. **Проверка:** архитектор, отдельный plan reviewer и code reviewer читают одни и те же actual SHA; имитация независимого AI-review запрещена.

**Принадлежность каталогу:** [золотая первая глава](00-DESIGNTOCODE-CLEAN-GOLDEN-RECOVERY-FIRST-2026-10-19.md), [direct evidence+новые предположения](54-DESIGNTOCODE-CLEAN-DIRECT-DIFFS-AND-NOVEL-BOUNDARIES-2026-10-19.md), [source/review](54-DESIGNTOCODE-CLEAN-EVIDENCE-AND-SKILL-REVIEW-2026-10-19.md).
