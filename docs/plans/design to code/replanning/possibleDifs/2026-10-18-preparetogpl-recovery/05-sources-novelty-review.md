# Источники, проверка новизны и границы инженерного вывода

18 октября 2026. [Главная глава восстановления](README.md). Этот журнал позволяет отличить новое наблюдение по исходникам, сохранённый результат прежнего запуска, предыдущий разбор другого агента и предлагаемый эксперимент.

## 1. Зафиксированные версии

| Репозиторий / ветка | Commit | Что сопоставлялось |
|---|---|---|
| llm_mvp_core / PrepareToGPL | [31346d8928be4469b01aaf313780e8f950ff91e2][gpl] | Основной donor: neural код, TranslationMachine и её assets, E07/E09/E25/E26/E43, ProjectWiki receipt |
| llm_mvp_core / codex/dpo-implementation | [a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6][core] | Нынешние APIs/consumers, full/legacy restore, DPO/LoRA/tokenizer, RP01 receipts, защищённый каталог различий |
| xiigen-planning / llm_plan3 | [a10bbde116515b1453d9ec6bfb103725ad99d477][planning] | Снимок existing possibleDifs, RP01 pack, RAG и родительские документы |
| llm_mvp_core / DesignToCode | [8f20ceb8a9bfbadab2ad309226c47dd76e714548][dtc] | Состав дерева и retained ProjectWiki summary025 |
| llm_mvp_core / claude/sonnet-local-wiki | [93a46381758257b72ee158159caf26b7fc4390b0][wiki] | Состав дерева и тот же retained summary025 |
| llm_mvp_core / external_domains | [3036a01acec938f0b83fa4ffbda26202cc35d639][external] | Состав дерева; поздних проектов27/42/43 в этом snapshot нет |
| llm_mvp_core / language_translate | [f6032aff81f1f83de7c09cf9b9efe6b058dd12b9][language] | Проверка места исторической Translation ветки; текущий проект27 этому snapshot не приписывается |

Дополнительно зафиксированы refs `DesignToCode-clean=e346b88a3cc1fcce48aefa626bfc7b00509eefd6`, `orchestrator-extensions=b293045bd89b1eb96d3e32fedd341f1a4170adbc`. Их фиксация — ориентир истории, а не заявление о полном чтении исходников или измерениях качества этих веток.

### Сохранённый neural след действительно есть в соседних ветках

Один и тот же `project-wiki-dpo-metric-summary.json` серии025 прочитан в [PrepareToGPL][donor-gpl], [DesignToCode][donor-dtc] и [claude/sonnet-local-wiki][donor-wiki]. Во всех трёх Git blob:

`3ea15f35042548c57b412497b269e3d69af946ba`

Он содержит 50 train/50 heldout, initialization от policy023, heldout preference accuracy≈0.98 и margin≈0.00235567. **Это один сохранённый исторический результат в трёх ветках**, не три независимых подтверждающих запуска. Различение neural state и предметного planner consumer взято из ранее выполненного подробного [core25][prior25]; в этом исследовании заново прочитаны raw summary и donor runner, но binary weights не исполнялись.

Таким образом, поиск не ограничен списком экспериментов в одной ветке. Вместе с тем все соседние ветки и все их positive receipts полностью не обследованы. Из отсутствия нового найденного receipt не делается вывод об отсутствии прежнего успешного обучения.

## 2. Объём дедупликации

С обоих каталогов на приведённых SHA получены все файлы с расширениями `.md`, `.txt`, `.ndjson`, обнаруженные в рекурсивных trees. На завершении загрузки ошибок этих чтений —0.

| Каталог | Markdown | Другие выбранные тексты | Всего текстов | Объём локальных копий |
|---|---:|---:|---:|---:|
| core/docs/plans/replanning-possible-diffs | 260 | 12 | 272 | 6 740 178 bytes |
| planning/.../replanning/possibleDifs | 272 | 0 | 272 | 3 827 349 bytes |
| **Итого** | **532** | **12** | **544** | **10 567 527 bytes** |

Выполнен полнотекстовый поиск новых точных условий и смысловых соседей; близкие отчёты затем читались адресно. **544 — объём поиска, не число полностью семантически разобранных документов.** Code/JSON/binary/proxy файлы прежних каталогов в эту текстовую квоту не входят; выбранные первичные JSON и исходники читались отдельно.

Поэтому новизна сформулирована ограниченно: «такой точный mechanism+caller не найден в двух проверенных текстовых каталогах», а не «никто никогда об этом не писал». Уже известные neural cold-start, vector scalar, BPE identity, first-hit KNN, E09 opt-in, E25/E26 drift, replay, label leakage и general restore не объявлены новыми.

### Финальная сверка параллельных публикаций

Перед записью branch llm_plan3 обновился до `a4a3e709f8fb5310baab6daa16501b5d4394bd72`. Сравнение с первоначальным snapshot показало11 новых Markdown и изменённый README; все11 получены и включены в дополнительный полнотекстовый поиск, наиболее близкие N40/N41/N42 прочитаны адресно. Итого — **555 уникальных текстовых путей**, включая обновлённый README. Core HEAD повторно проверен и остался `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`.

Найдено прямое совпадение U-R1 с [N41-T1][parallel41]. Его статус во всём пакете изменён на независимое подтверждение. U-R2, U-N3 и N-L1 по точным условиям/callers не найдены и в11 новых документах. [N40 corpus/intrabatch][parallel40] рассматривает смену текста при прежних refs и отсутствие candidate-to-candidate сравнения; U-N3 касается другого стыка — raw PairId против derived Unit ExampleIds. General restore и side-derived lessons остаются общими известными семьями.

## 3. Новые наблюдения, параллельное совпадение и ближайшие разборы

| Новая карточка | По чему искали | Ближайшее прежнее семейство | Добавленное доказательство |
|---|---|---|---|
| U-R1 — подтверждение | EXECUTOR_DIRECTIONS_INCOMPLETE, topology4/6, old checkpoint + current loader | Параллельный [N41-T1][parallel41], [core78][prior78] | Дополнительные два exact artifacts и уточнённый no-training протокол; сам механизм уже опубликован другим агентом |
| U-R2 | TranslationModelTrainingDataEvidence, training_data_evidence, manifest schema/predicate | Общий checkpoint/manifest/provenance; [planning04][prior-base] | Обязательное поле при прежней schema1, отсутствующее в exact сохранённом manifest; отдельный typed publish caller |
| U-N3 | oldPairIdSet, noReplayProven, raw PairId vs derived ExampleId, builder suffixes | [Planning16: Units и KNN][prior16], [core60: signatures/budget][prior60] | Upstream composition general builder→checkpoint→continuation; P не совпадает с тремя Unit IDs |
| N-L1 | ParseLegacyCheckpoint, _baseParameters, legacy+attached adapters, complete restore intent, named legacy test | Общий base/LoRA/restore identity; [planning04][prior-base], [core25][prior25] | Намеренное сохранение A/B, явный существующий test и StartService fallback; предложен caller-level intent, а не исправление codec |

Пункты не являются независимыми доказанными причинами падения качества: U-R1/U-R2 условно блокируют старые assets в разных путях; U-N3 может повторно применять урок. N-L1 — новый проверяемый вопрос к смыслу restore/evidence, для которого обнаружено важное контрдоказательство ошибочной трактовке.

**Причинный semantic gain от предложенных изменений не измерялся.** Исторический phase38 receipt уже известен core78; новыми являются нынешние препятствия использованию его сохранённых файлов.

## 4. Первичные чтения, на которых стоят новые выводы

| Группа | Прочитанная цепочка |
|---|---|
| Translation restore | Обе TranslationMachine; runtime directions; old/new ImportCheckpoint; LoadCheckpointAsync; current CLI caller; оба retained checkpoint, manifest и phase38 report/stdout |
| Manifest | Старый/новый TranslationModelManifestContract; current TranslationModelAssetStore; обязательный evidence predicate и порядок corpus admission |
| DPO→Units | Обе TranslationStructuredBatchBuilder; typed contracts; TrainStructuredBatchAsync identity ledger; обе CLI continuation filters; Executor memory write и ordinary routing |
| Neural | Обе LlmModel, RmsNorm, attention/transformer layers, DpoTrainer, LoraApplicator/LoraLinear, BpeTokenizer; current exact material и declared full topology |
| Restore caller | StartService fallback; sealed loader/manifest; strict SimulationEnRuContinuation importer; исходник специального legacy-adapter test |
| История KNN | E07/E09 source и receipts; Git transfer commit; preserved StructuralKnn/facade/options; E25 learner, source и receipt; E26 old/new dictionaries; E43 source/receipt |
| RP01 | C# scientific-result; ENRU scientific-result, main-state-verification, analysis-proof и grades; RP01 recovery audit, runner/config |
| Родительские материалы | RAG-01/02/03, branch manifest, нужные разделы lifecycle plan, действующие локальные инструкции и skills |

Каждая содержательная глава содержит ссылки на полные Git SHA и точные участки, где они помогают проверке. Тексты предыдущих ассистентов используются как указатели и как явно атрибутированные прошлые разборы. Для новых механизмов основой служат исходники и сохранённые данные, а не согласие нескольких отчётов.

### Равенство Git objects как контрдоказательство переписывания алгоритма

| Объект | PrepareToGPL = current |
|---|---|
| Весь `06.VectorSearch` | tree `844e84623a75eff857227d5759de8eb73e986ef6` |
| Весь `12.SemanticCompiler` | tree `cae2d9355e803e5b46ec52b7d4fe0f00b524af0f` |
| `TranslationTrainedUnits.cs` | blob `1742abfc9fb617dfc66e4d618e0c169da01a6ccc` |
| `TranslationSemanticTokenizer.cs` | blob `ee6d81c7933fa33a9c57a2111990019ec55e5dab` |
| Исходный translation checkpoint | blob `a4cf054ba3f8a6ee6aa9818edc6c5705e3456c25` |
| Continued phase38 checkpoint | blob `11a19a5f49d27ef75043a252f255a2bb5c44a389` |
| Старый manifest | blob `9c79956c7c8bddbbc7afba37112c94f11648797b` |

Это серверные Git object IDs. Hash локальной текстовой копии с нормализованным окончанием файла не подменяет их. Одинаковый исходник не доказывает одинаковое runtime state/настройки; одинаковый сохранённый asset не гарантирует, что нынешний loader его принимает.

## 5. Применённые skills и независимая проверка

Для работы применены проектные skills архитектуры, планирования, проверки плана/кода, prompt authoring и человеческой коммуникации из [docs/skills][skills]. Практические требования, использованные здесь: назвать actual recipient и owner, показать before/after, сопоставить источник→состояние→consumer, отделить план от исполнения, сохранять положительные предметные результаты вместе с областью применимости, давать короткий опровергающий опыт.

Прочитаны `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`, инструкции сессии; применимые части больших planning/plan-review/architecture/code-review skills прочитаны адресно участниками исследования. Полная загрузка всех многосотстраничных skills не заявляется. Термины согласованы с glossary: домен — предмет обучения; направление языка — профиль; LoRA/KNN — механизм; Unit/Flow — архитектурный уровень.

Отдельный reviewer DPO/LoRA полностью прочитал все шесть документов до финального приёма параллельных добавлений; последующую сверку новизны и её переобозначение выполнил автор, сверил новые Source claims и точность ссылок. Его адресные замечания внесены:

- v2 migration U-R1 требует изменения schema dispatcher, который сейчас принимает только1;
- retained основной specimen для двух уроков — именно phase38 continued checkpoint, model0.1.2/schema1;
- пустые ES maps не обещают отказа общей KNN и не доказывают изоляцию направлений;
- нынешнее преобразование исторического reward0.03→0.0 сохраняется как явная нормализация отчётной метрики;
- сохранение A/B в legacy load намеренно и подтверждено существующим test; repair относится к caller intent;
- rank/alpha уже существовали в GPL config; новыми стали свойства/seed/mode, а не сами параметры LoRA;
- для E-POS нужен явно предложенный POS seam версии29 и mapping разных label sets;
- 16 изменённых объектов RP01 — **A/B tensors**, то есть8 q/v adapter modules в4 слоях;
- границы line anchors уточнены по реальным исходникам.

Проверка документов включает локальные relative links, reference definitions, UTF-8, pinned source paths/line ranges, разрешённый путь публикации и чтение опубликованного результата. Она не заменяет выполнение будущих экспериментов. Последний readback проверяется после создания коммита; его точный SHA сообщается в ответе пользователю.

## 6. Границы результата и следующий предметный шаг

- Новые experiments, model calls, inference, обучение, сборки и native tests здесь не выполнялись. Исходники tests читались для понимания контракта.
- Binary weights исторических neural donors и RP01 не загружались в модель. First-decision comparison.json через доступное чтение оказался LFS pointer; его полный payload заново не проверен.
- Прочитанные positive receipts имеют свою задачу и панель. Один и тот же файл в нескольких ветках не становится несколькими опытами.
- Переход «именно тот отрицательный matrix-run → тот успешный KNN-run» остаётся не полностью аттестованным. E09 port подтверждён Git; E-MATRIX-KNN выделяет недостающий matched contrast.
- Прямая причина neural RP01 0/3 и0/4 новыми restore-пунктами не установлена. Для этих запусков есть противоположное evidence об изменившихся LoRA и успешном strict full-state reopen.
- Публикация относится только к planning/possibleDifs. Защищённый core-каталог использован для чтения и дедупликации.

Следующий конкретный выбор — actual recipient: если нужно восстановить прежнюю TranslationMachine, начать с U-R1 и затем U-N3; если потеряна полезная структурная функция в ответе, начать с E-POS/E-AST-ROUTE; если нужен собственный нейронный ответ RP01, проверить E-DATA/E-HEAD/E-BASE с сохранением исправленной математики. Развёрнутые карточки уже подготовлены в соответствующих главах.

[gpl]: https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2
[core]: https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6
[planning]: https://github.com/lubochka/xiigen-planning/tree/a10bbde116515b1453d9ec6bfb103725ad99d477
[dtc]: https://github.com/lubochka/llm_mvp_core/tree/8f20ceb8a9bfbadab2ad309226c47dd76e714548
[wiki]: https://github.com/lubochka/llm_mvp_core/tree/93a46381758257b72ee158159caf26b7fc4390b0
[external]: https://github.com/lubochka/llm_mvp_core/tree/3036a01acec938f0b83fa4ffbda26202cc35d639
[language]: https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9
[donor-gpl]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/project-wiki-dpo-metric-summary.json
[donor-dtc]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/project-wiki-dpo-metric-summary.json
[donor-wiki]: https://github.com/lubochka/llm_mvp_core/blob/93a46381758257b72ee158159caf26b7fc4390b0/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/project-wiki-dpo-metric-summary.json
[prior25]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md
[prior78]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/78-translation-restored-memory-positive-receipt.md
[prior60]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/60-knn-signature-alias-and-candidate-budget.md
[prior-base]: ../04-BASE-LORA-CHECKPOINT.md
[prior16]: ../16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md
[skills]: https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills


[parallel41]: https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md
[parallel40]: https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md
