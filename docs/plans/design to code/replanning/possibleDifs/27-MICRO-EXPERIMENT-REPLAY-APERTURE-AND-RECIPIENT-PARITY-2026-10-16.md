# MICRO-N27 — можно ли вернуть старый навык, изменив только состав replay/KNN памяти?

**16 октября 2026; executable study design, new native run NOT_EXECUTED.** Это план короткого фальсифицирующего исследования, а не новая реализация продукта. Код исходной ветки фиксировать на `llm_mvp_core@8c7d41611aecba98d3d1bbdbf30d69bd784949ae`. Главный источник доказательств: [N27 — окно памяти](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md). GitHub core/docs/plans/replanning-possible-diffs: READ-ONLY.

## Проверяемая, заранее опровергаемая гипотеза

**H1 / KNN:** новый обучаемый Unit не смог правильно ответить на ранее освоенный закон потому, что `BuildKnnTrainingCorpus` получает лишь ≤3 выбранных прежних manifest, а не все разрешённые уроки прежнего Unit. Контрфакт: вернуть требуемый старый пример в **тот же** корпус, не меняя модель/LoRA/токенизатор/encoder/метрику, и правильный ответ снова появится.

**H2 / DPO:** fusion выбирает семантически релевантный manifest в top3, но `AutonomousLearningReplayMixer` заменяет порядок на newest-first, а при 1 новом branch и repeatCount=2 подаёт в DPO лишь 1 старый manifest. Контрфакт: одинаковое количество DPO steps/tokens и прежний corpus, но replay example выбирается по релевантности с заранее определённым ranking order; gradient и предметный ответ должны различаться **после** настоящего one-step train, а не только по списку IDs.

**H3 / scope:** прежний старый Unit существует и маршрутизатор способен вызвать его; значит отсутствие примера только в новом Unit может **не ухудшать** весь продукт. Этот отрицательный результат является ценной фальсификацией H1 на реальном consumer.

## Малый тестовый материал

- 6 правомерно сохранённых, **аттестованных** прежних уроков одного `DomainTag`, из минимум 4 различимых предметных `SubPhase/semantic law`; в одном случае старый релевантный урок имеет более ранний `CreatedAtUtc`, чем три новые записи. Их реальные `ExampleId`, `TrainingText`, `OutputText`, `PredicateIds`, `DomainTags` и `ReplayAttestation` берутся из существующего законного sandbox registry. Не фабриковать подпись, license, replay seal или consent.
- 1 новый урок **того же** домена, но другого закона; не подменять `SubPhase` случайным `domainTag`. Если в фактическом registry нет 6 легитимных уроков, создать **только test-scope** deterministic fixtures через уже разрешённую harness path; они доказывают арифметику выбора, но не качество исторического обучения.
- 8 закрытых для выбора/ранжирования **source-family-disjoint** old-law задач и 4 new-law задачи; для EN→RU отдельно сохранять отрицание, субъект, грамматическую форму; для C# — исполнимый смысл кода. CODEX/Astra должен заранее установить юридически и предметно корректный oracle, либо `UNJUDGEABLE`.
- Единые `checkpointSha256`, `tokenizerSha256`, `index schema/encoder`, routing flags, seed, max tokens, temp, prompt и law rubric для сравниваемых arms.

## Фаза 0 — сверхдешёвый детерминистический proxy (уже выполнен отдельно)

[Источник JS](27-MICRO-REPLAY-APERTURE-SELECTION-PROXY.mjs) и [raw JSON](27-MICRO-REPLAY-APERTURE-SELECTION-PROXY-RESULT.json) моделируют именно выбор `top3`, `replayCount=Round(2×.30/.70)=1` и повторную сортировку по `CreatedAtUtc` на 6 синтетических manifest. Моделируемый fusion order задаётся **готовыми synthetic relevance scores**, а не C# BM25. Успешный proxy доказывает только **возможность исключения старого ID** при существующем кодовом правиле и уязвимость order-preservation; он **не** является настоящим .NET Unit, проверкой легальности урока или доказательством снижения качества.

## Фаза 1 — read-only native trace, без модели и без мутации

В имеющемся `Tests`/diagnostic harness вызвать test-only wrapper над реально используемым `SelectAndValidateReplaySnapshot`, включая существующие admission validators. Сначала проверить, что шесть старых manifest допущены **без ослабления проверок**; затем логировать ровно:
`inputDomain, manifestId, SubPhaseId, CreatedAtUtc, fusionScore/position, chosenTop3, replayDpoIds, builtKnnExampleIds, omittedOldIds, policyCheckpointSha, consumerFlowId`.

Для сравнения вызвать **фактический** `AutonomousLearningReplayMixer.BuildPreferences` и `BuildKnnTrainingCorpus` (можно через внутренний test-only seam; нельзя менять production visibility без code-review). Проверить условные ассерты:
1. `count(selected old) <= 3`;
2. `DPO old set ⊆ selected old`, но может не совпадать с KNN old set;
3. при одной текущей ветке и `currentRepeatCount=2`, `count(DPO old)=1`;
4. если релевантный top1 старше остальных top3, он **может быть исключён из DPO**, даже оставаясь в KNN;
5. история `manifest.KnnExamples` не добавляется транзитивно в `BuildKnnTrainingCorpus`;
6. при 257 проверяемых manifest safety gate выдаёт отказ, а не тихо обрезает/пропускает proof.

Если native pathway не позволяет проверить корректную attestation, результат **BLOCKED_ADMISSION**. Не заменять реальную проверку выдуманным fake manifest и не утверждать PASS.

## Фаза 2 — настоящая причинная оценка качества **на том же Student**

Не выполнять фазу, пока фаза 1 не дала различающихся `exampleIds`. При этом не нужно запускать большие 500-парные DPO кампании.

| Arm | DPO / policy weights | KNN corpus | Что изолирует |
| --- | --- | --- | --- |
| A — baseline | один и тот же checkpoint, **без** новых обновлений | текущий top3 + текущий урок | реальный latest Unit и его ошибки |
| B — memory-only | те же неизменные weights A | все 6 правомерных прежних уроков + текущий (отдельный test-only Unit, та же реализация encoder/index) | H1: потеря старых примеров при выборе корпуса |
| C — size control | те же неизменные weights A | top3, где one irrelevant заменён на заранее выбранный законный old-law, общая длина = A | релевантность vs простое увеличение корпуса |
| D — router control | наблюдение старого Unit: собственные прежние weights/индекс, **не matched с A** | вызов старого Unit/Flow на тот же old-law запрос, если он существует | H3: способен ли обычный consumer найти старую способность; отличия D от A нельзя приписывать только KNN corpus |

Для каждого запроса сравнить `candidateRecall@k`, выбранный `exampleId`, семантически правильный ответ, `lawViolation`, `abstain` и `latency`. Считать **paired old-law wins/losses** по отдельным законам и изменения **coverage при неизменном бюджете**; просто больше принятых ответов без правильности не является выигрышем.

**Только если H1/H3 разделены**, отдельная маленькая DPO-проверка H2: два изолированных разрешённых one-step training-кандидата с сопоставимыми фактически израсходованными response tokens, seed, policy/reference, loss и KNN corpus. Если одинаковые токенные бюджеты для двух правомерных старых пар обеспечить нельзя, разницу report отдельно и не приписывать весь effect одной replay selection. Отличается только **какой один старый релевантный пример** попал в replay. Регистрировать `actual chosen/rejected IDs, mask/token counts, gradient norm, preference margin, heldout old-law, new-law`; независимые новые lease/run identities, без повторного использования consumed one-shot dataset и без production publication. Не смешивать frozen weights arm B и trained weights H2 в одной статистике.

## Выходные решения и фальсификаторы

- `SOURCE_SELECTION_ONLY` — появились разные списки IDs; не заявлять качество.
- `RETRIEVAL_CAUSAL_GAIN` — один и тот же frozen policy с корпусом B/C исправил blind old-law ответы A при одинаковом consumer; прирост относится к памяти/KNN, **не DPO/LoRA**.
- `DPO_SELECTION_GAIN` — one-step H2 после CODEX/Astra judge улучшил независимо оценённые old-law задачи при matched budget; не считать только изменение loss.
- `ROUTER_RECOVERS_OLD_UNIT` — старый Unit правильно отвечает на обычном маршруте: гипотеза деградации всего продукта ослаблена/отклонена.
- `NO_QUALITY_GAIN` — список изменился, а paired quality нет; не переносить арифметическое свойство в causal claim.
- `NOT_EXERCISED_IN_RP01` — изолированный RP01 Student не вызывает capability-KNN: не приписывать ему найденный пробел.
- `NOT_INFORMATIVE` — old skill baseline уже 0, все gold cases совпали с train, нет законных manifest, или отсутствует вызываемый реальный consumer.

Фиксировать отдельно : `train-only gain`, `heldout gain`, `ordinary consumer gain`, `model weights unchanged/changed`, `memory-only gain`, `resource budget`, `loss/ref check`. Сохранить конфликты и отрицательные результаты.

## CODEX/Astra и внешний учитель — обязательное разделение полномочий

**Выбор выполняет CODEX на Astra 6 UltraCode**: правильное место обучения, domain law, контрфактический случай, learner recipient, admission acceptance и утверждение sealed heldout. **Внешняя открытая китайская модель создаёт обе стороны DPO** (`chosen` и `rejected`), а не выбирает урок и не оценивает качество. **CODEX на Astra 6 UltraCode отдельно проверяет обе стороны DPO и предметное качество после обучения** (второй независимый judgment, provenance и законный источник). В source-only proxy новые DPO не генерируются; запрещено выдавать фиктивные или Astra-авторские пары за фактические результаты китайской модели.

Обязательная подготовка исполнителя: `docs/skills/architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md` и `prompt-patch-authoring-SKILL.md`. Перед кодом — полное чтение caller+callee+consumer, schema+policy+tests, затем отрицательный контроль.

## Образец короткого рабочего задания CODEX

**CURRENT TASK:** выполнить `MICRO-N27` только после source mapping. **NOT THE TASK:** чинить весь RP01, увеличивать число зеленых тестов, подменять истории манифестов, реорганизовывать DPO pipeline или изменять core/doc/plans/replanning-possible-diffs. **EVIDENCE ONLY:** прежние N20/N22, E43 и proxy JSON. **Allowed changed files:** новый test-only diagnostic/fixture в разрешённом worktree core, плюс результаты только в planning/possibleDifs после независимой проверки. **Forbidden:** protected core docs, production adoption, обход admission, выдача synthetic tests за native learning. **Pass:** source-pinned trace IDs + paired actual Unit/Flow answers + независимая предметная оценка + verified checkpoint/tokenizer; **Fail:** только изменился manifest count, LoRA SHA, proxy или pass flag. **Return packet:** SHA; precise file/method/line references; old/new memory IDs; run commands; stdout/stderr; actual judge packet ID or `NOT_RUN`; negative controls; result status; unexpected side effects; cleanup; bounded next repair. Отрицательный результат публиковать.

**Состояние на публикации этого протокола:** source audit выполнен; JS proxy выполнен; .NET native H1/H2/H3 НЕ выполнен; новый DPO НЕ создан; CODEX/Astra judgment НЕ проведен; causal consumer improvement НЕ доказан.
