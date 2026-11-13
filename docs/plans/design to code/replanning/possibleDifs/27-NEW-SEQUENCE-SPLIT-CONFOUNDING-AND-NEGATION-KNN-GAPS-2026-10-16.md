# N27 — новые проверенные швы: разрушение учебных последовательностей и невидимое отрицание в translation KNN

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Различить содержание, личность строки и порядок

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

P127 показал влияние порядка кандидатов: в A изменились 5/16 ответов, в B — 0/16 на проверенной перестановке. SourceRowId при сопоставлении одинакового текста одной семьи сохраняется; порядок sources не выбирает личность через rows[0]. Положительный случай повторяющихся Train-строк одной семьи принимается; ложный ID и несколько возможных семей отвергаются. Семантический ordered output и обязательный finalizer связаны. Результат P127 не разрешает повторять PRIMARY и не приписывает способность к отрицанию, которую не учили.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/KNN-ORDER-PERMUTATION-01.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->

**Дата:** 2026-10-16. **Статус:** `DRAFT / CODE_CONFIRMED / HISTORICAL_RUN_READ / QUALITY_CAUSALITY_NOT_RUN / INDEPENDENT_REVIEW_PENDING`.
**Core exact snapshot:** [`codex/dpo-implementation@8c7d416`](https://github.com/lubochka/llm_mvp_core/tree/8c7d41611aecba98d3d1bbdbf30d69bd784949ae). **Единственное место записи**: `xiigen-planning/llm mvp/design to code session/replanning/possibleDifs`. Каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` — **READ ONLY**, не редактировался.

## Почему это важно

В RP01 наблюдается реальное изменение LoRA, но не обнаружено улучшения свободных ответов: C# **0/3→0/3**, EN→RU **0/4→0/4**. Исторические положительные KNN/структурные эксперименты не означают, что обучается этот же Student или что его ordinary consumer использует тот же код. Однако обнаружено **ещё одно семейство ошибок в интерпретации прежних НЕУДАЧНЫХ нейросетевых экспериментов**: преобразование набора отдельных предикатов в случайно переставленную последовательность *до* обучения отношений «предикат→следующий предикат». Это не просто другая гиперпараметризация — изменяется смысл supervised target. Отдельно узкий язык-предохранитель и нормализатор переводческого KNN могут терять отрицание.

### N27-01 · P0 для повторной интерпретации E29V2: stratification уничтожает соседство, которое ProbeNN должен предсказывать

**Факт исходника (C):**
1. [`StratifiedSplit.Split`, L19–36](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs#L19-L36) группирует **каждый элемент** `examples` по ключу, отдельно перемешивает каждую группу, затем перемешивает весь train и val. Код не получает границы предложения/файла или исходные индексы последовательности.
2. [`E29DpoPairValueV2.Run`, L23–34](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E29DpoPairValueV2.cs#L23-L34) сначала объединяет предикаты извлечённых файлов в `allPreds`, затем вызывает `StratifiedSplit.Split(allPreds, p => p, ...)` — **stratification по самому token ID**.
3. [Там же L45–65](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E29DpoPairValueV2.cs#L45-L65): и baseline, и treatment вызывают `TrainStep(trainPreds[i],trainPreds[i+1])` на уже переставленных значениях; оценка `Evaluate(valPreds)` также читает искусственную последовательность. `chosenIdx` означает первые появления типа предиката, *не* подтверждённые человеком chosen/rejected стороны DPO. Двойной `TrainStep` **не является DPO objective**.

**Следствие по алгоритму (C):** исходное отношение A→B не сохраняется под перестановкой элементов; измеряется обучение случайной последовательности частот предикатов. Даже настоящий достаточно обучаемый learner может дать нулевой результат на таком target; даже положительный результат не был бы доказательством переноса законов исходных файлов.

**Первичный измеренный архив (M):** [E29V2 Tier2, 01.05.2026](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E29V2-DPO-PAIR-VALUE/results/run-2026-05-01-001.json) содержит baseline **0.2308**, enhanced **0.2308**, PASS=false. Следовательно, **нельзя ретроспективно объявить эксперимент успешным или доказательством неправильности LoRA/DPO**. Новый вывод относится к валидности экспериментальной постановки, не переоценивает исходные цифры.

**Различающая проверка:** четыре плеча 2×2: `original-adjacent transitions / shuffled-token synthetic transitions` × `same # updates uniform / chosen-weighted`. Сохранить **одни и те же границы исходных файлов, train/holdout families, tokenizer, seed, learner**; train/holdout разделять по целым последовательностям **до** извлечения соседних пар. Отчётные метрики: original heldout transition accuracy и действительно независимая предметная law correctness, не `accuracy` на переставленном `valPreds`.

### N27-02 · P0 для lineage «матрица → KNN»: E44V2 смешивает два вмешательства плюс неверно называет метрику F1

**Факт исходника (C):** [E44V2 L40–74](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs#L40-L74) baseline объединяет `genTrain.Concat(secTrain)`, **перемешивает отдельные токены**, после чего берёт их новые смежные пары. Enhanced обучает отдельно последовательности `genTrain` и `secTrain` и повторяет security-пары **10×**. Таким образом baseline/treatment различаются *и весом, и самой парой (x,y), и количеством обновлений, и порядком*; это не изолированный тест «security weighting».

[`StratifiedSplit` L23–35](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs#L23-L35) дополнительно переставляет значения **внутри** gen/sec до создания этих пар. [E44V2 L52–82](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs#L52-L82) называет `weighted_f1` выражение `0.7 × secAccuracy + 0.3 × generalAccuracy`; **precision/recall/F1 здесь не считаются**. Требуется переименовать метрику в аналитическом отчёте (исторический JSON НЕ менять).

**Первичный архив (M):** [E44V2 Tier2, 01.05.2026](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E44V2-DATASET-WEIGHTER/results/run-2026-05-01-001.json): `genPreds=6`, `secPreds=875`, baseline 0.1663 → enhanced 0.0039, PASS=false; security 23.8%→0.6%. Данные **показывают провал именно сохранённого комплексного treatment**; не идентифицируют знак эффекта веса отдельно от повреждения переходов и 10-кратной нагрузки. Это потенциально принципиальное место неправильной исторической интерпретации как неуспеха «матричного обучения».

### N27-03 · P1 для исторического curriculum E45V2: random mixing формирует несуществующие переходы, «easy/hard» — порядок списка

[Код L27–45](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E45CurriculumOrganizerV2.cs#L27-L45) относит первые 70% **файлов в поданном порядке** к easy, последние 30% к hard и baseline перемешивает **сами элементы predicate-token stream** перед созданием соседних пар. [Enhanced L50–62](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E45CurriculumOrganizerV2.cs#L50-L62) берёт реальные смежности внутри уже склеенных (тоже без границ файлов) потоков easy и hard. Следовательно, различия измеряют одновременно порядок учебных примеров, содержание переходов и невалидированное обозначение сложности.

[Сохранённый E45V2 Tier2](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E45V2-CURRICULUM-ORGANIZER/results/run-2026-05-01-001.json): 0.3623→0.3478, PASS=false. Нельзя считать это доказательством бесполезности учебной программы или невозможности постепенного обучения доменам. Нужны fixed examples/fixed transitions, меняется **только порядок batch**; difficulty назначается по независимому критерию и сохраняется.

### N27-04 · INDEPENDENT RECONFIRMATION, NOT A NEW BUG: два фильтра схожести стирают полярность

**Проверенные факты (C):**
- [`TranslationKnnStore.LowInformationSourceWords` L194–200](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationUnitContracts.cs#L194-L200) включает `not` и `no`; [`SourceTokens` L308–323](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationUnitContracts.cs#L308-L323) удаляет их и считает **асимметричный** score `|intersection|/|queryTokens|`. Пары «approve the request» vs «do not approve the request» после этого фильтра могут иметь similarity=1, если остальные лексемы совпадают.
- [`ExecutorUnit.SourceContentTokens` L962–989](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L962-L989) **независимо** исключает `not`/`no` при дополнительной EN→RU source similarity; это *другая проверка*, а не гарантия сохранения полярности.
- [`TranslationMemorySourceLaw` L19–45](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMemorySourceLaw.cs#L19-L45) возвращает `Contradiction` только для небольшого закрытого поднабора простых английских copular clauses. Прочие («do not approve ...») получают `Unknown`, а [`FindExactWithSourceGuard` L274–292](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationUnitContracts.cs#L274-L292) допускает всё, что `!= Contradiction`.
- [`TranslationSemanticTokenizer.BuildRuleTags` L153–159](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationSemanticTokenizer.cs#L153-L159) сразу возвращает пустые tags для `sourceLang != "ru"`. Это **не доказывает**, что `not` не меняет POS-structure, но отсутствие отдельной EN-polarity law координаты здесь подтверждено.

**H — строгая условная гипотеза:** если два источника противоположной полярности имеют одну сохранённую полную/префиксную/структурную сигнатуру (либо route дал broad fallback), negative-source candidate проходит similarity и narrow-law как допустимый. Тогда на обычном потребителе `ExecutorUnit.TrySelectKnnCandidate` [L333–363](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L363) может выбирать target противоположного смысла с confidence 0.88. **Но** exact/phrase/word-map получают приоритет, структура POS иногда различает предложения, внешняя проверка может отклонить кандидата; значит **это не доказанный пользовательский дефект и не доказанное объяснение RP01 0/4**. Проверить реальный call path, кандидата и финальный verdict.

**Также ранее исследованная ось (см. N26):** score `intersection/queryTokens` меняется при перестановке query↔candidate, хотя cosine/Jaccard симметричны. Это может повышать recall для короткого запроса, но опасно для длинного кандидата со смысловыми дополнениями. Калибровать *вместе* с недетализированным negation scope, а не называть само по себе ошибкой.

## Проверка уникальности относительно чужих агентов

Не дублировать:
- [N16 FIFO/TopicDomain](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md): позднее исправление скрывается `Take(k)`. **N27-04 — НЕ НОВАЯ НАХОДКА:** удаление `not/no` уже установлено в [16 Independent Translation Unit](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md), а асимметричная `SourceTokenSimilarity` и потери других ролей — в [26 Role/Polarity](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md). Здесь лишь дополнительная сквозная перепроверка обеих функций и narrow copular law.
- [11A KNN token-order](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md): bag-of-BPE теряет порядок слов; **N27-01–03** — разрушение *последовательности учебных переходов самим experiment harness*.
- [N21 structured-learning](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md): self-label, key drift, novelty and verifier; **N27-01–03** — результаты старых `ExperimentsV2`, независимый learner.
- [Historical matrix/KNN](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md): перенос функций; N27 требует сохранять **целостность исходных связей**, прежде чем сравнивать их в neural matrix и KNN.
- [RP01 current audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): actual Student DPO path отличается от исторических E29/E44/E45 и structured TranslationKNN.

**Проверка соседних веток:** exact function body от `namespace ExperimentsV2;` по EOF у E29V2/E44V2/E45V2 одинаковый между `DesignToCode` и `codex/dpo-implementation` в просмотренных blob версиях; основной diff — вводный блок лицензии. Это подтверждает сохранение *этого экспериментального алгоритма*, но **не** независимую идентичность результатов/потребителей и не доказательство успешного обучения на ветке. На ветке `language_translate` компоненты `27.TranslationDomain/TranslationUnitContracts.cs` и `TranslationMachine.cs` по указанным путям отсутствуют; нельзя приписывать ей контракт нынешнего Translation Unit без полного сопоставления альтернативных путей.

## Приоритет и честная степень доказанности

| Находка | Исходник | Архивный исход | Влияние на нынешний RP01 | Действие |
|---|---|---|---|---|
| N27-01 shuffled predicate adjacency | CODE_CONFIRMED | E29V2 FAIL 0.2308→0.2308 | **НЕ ДОКАЗАНО;** историческая постановка | Tiny sequence-preservation A/B |
| N27-02 E44v2 mixed confounding / F1 mislabel | CODE_CONFIRMED | E44V2 FAIL 0.1663→0.0039 | **НЕ ДОКАЗАНО** | 2×2 weighting×order, fixed updates |
| N27-03 E45v2 curriculum/mixing | CODE_CONFIRMED | E45V2 FAIL 0.3623→0.3478 | **НЕ ДОКАЗАНО** | Fixed transition curriculum A/B |
| N27-04 repeated N16 translation negation filtering | RECONFIRMED, NOT NEW | Новый quality run НЕ проводился | **CONDITIONAL** отдельный KNN consumer | reuse existing N16 polarity fixture + ordinary adapter shadow |

**Граница утверждений:** ни одно из этих различий нельзя автоматически объявлять причиной 0/4 нейронного ENRU-Student: для этого сначала нужно доказать, что RP01 ordinary inference вызвал указанный KNN/Unit. Native .NET, blind independent heldout и review пока **NOT_PERFORMED**. Детальный быстрый протокол: [27 micro experiments](27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md).
