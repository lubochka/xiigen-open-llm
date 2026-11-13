# 20 — Новая гипотеза: рост обученной памяти меняет решение гибридного KNN без изменения старых уроков

16.10.2026 · SOURCE-VERIFIED MECHANISM / EXECUTED NUMERIC PROXY / REAL LEARNING IMPACT NOT MEASURED. Новый самостоятельный source-pass, а не дублирование существующих 44 DIF. Исследован core codex/dpo-implementation @ 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, planning llm_plan3 @ ba56fae4da47a52b870af0c022f733c7d7d230c8. Не менялись код, frozen C#500/ENRU500, результаты или экзамены.

## Объяснение для человека

Старая успешная структура KNN могла работать на небольшом банке. После добавления новых правильных уроков результат старого запроса может измениться не потому, что новое знание ошибочно, а потому, что смешанный ранжировщик **пересчитывает масштаб признаков на каждом новом списке кандидатов**. Дальнейшее увеличение памяти дополнительно усложняет запрос: данные многократно токенизируются при сравнении каждого кандидата.

Это **не** означает, что динамическая нормировка является доказанной причиной нулевого RP01 Student gain: offline RP01 тренировал отдельный DPO/LoRA и не использовал данный KNN scorer в самом Student free exam. Возможный эффект относится к гибридному retrieval/ordinary consumer и должен быть проверен на таком маршруте отдельно.

## N20.01 — кандидатная нормировка BM25 может инвертировать выученный ответ

**Прямое доказательство реализации.** В [LlmIngredientLogicalUnits.cs, строки 215–245](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L215-L245) метод ScoreCandidates для каждого найденного кандидата собирает vectorScore, lexicalScore и patternScore. Затем `maxLexical = Max(rawScores.lexicalScore)` по **текущему** candidate pool, `normalizedLexical = lexicalScore / maxLexical`, после чего `fused = vector*0.45 + lexical*0.35 + pattern*0.20`. Выбирается верхний score, а при равенстве меньший ExampleId. Это C — факт исходного кода.

**Новый causal seam (H).** При неизменных признаках двух ранее обученных кандидатов A/B третий C, оказавшийся в pool с большим raw BM25, понижает относительный вклад lexical для обоих A/B; конкурирующее vector-рейтинг может сменить победителя B на A. Корректная обученная метка может исчезнуть из ответа после добавления постороннего опыта. Такое поведение раньше не было выделено отдельным N20 в просмотренных `possibleDifs`. Нельзя называть его новым относительно старых веток: тот же `maxLexical` найден в [DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L179-L202) и language_translate (та же версия файла). Это **давняя возможная зависимость от размера памяти**, а не доказанная регрессия при merge.

**Выполненная малая математическая проба:** [исходный MJS](20-FUSION-CANDIDATE-NORMALIZATION-PROXY.mjs), [фактический JSON](20-FUSION-CANDIDATE-NORMALIZATION-PROXY-RESULT.json). Frozen A: cosine=0.7, raw BM25=0.2, pattern=0, неправильная метка; B: cosine=0.1, BM25=1, pattern=0, правильная. B побеждает со score 0.5975. Добавляется C: cosine=-1, BM25=10, pattern=0. A теперь побеждает 0.3895, B получает 0.2825. Контроль C с BM25=0.1 сохраняет B. Shadow с фиксированным lexical scale=1 тоже сохраняет B. Numeric calculation executed в Node 22.16.0; это **арифметический proxy**, а не настоящий BM25/HNSW, C# запуск или наблюдение качества обучения. Полный BM25 IDF и средняя длина корпуса здесь намеренно заморожены и в реальном Train изменятся.

**Различающий фактический тест:** единственный source-disjoint heldout с ранее корректным B; A/B кандидаты, векторы, модель, код и оценка frozen; добавить один легитимный учебный C с иной темой. Сначала сравнить две выдачи на настоящем ExecuteAsync, затем replay одинакового frozen A/B/C candidate pool и отдельно заморозить/отпустить maxLexical. Публиковать точные raw vector/BM25/pattern, maxLexical, ranked hit IDs, ответ, закон, latencies. Если в живом scorer не возникает инверсии, гипотеза данного эффекта на этой панели опровергнута.

## N20.02 — повторная токенизация может делать KNN с ростом памяти слишком дорогим

**Прямой путь.** [ExecuteAsync строки 185–206](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L185-L206) запрашивает у индекса `topK = _examples.Count`. [ScoreCandidates + ComputeBm25LikeScore строки 215–271](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L215-L271) на каждого кандидата вычисляют `averageLength` заново полным перебором словаря с `Tokenize(example.Text)`; для каждого совпавшего query term заново считают documentFrequency перебором всего словаря с `Tokenize`. [HnswIndex.Search](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/06.VectorSearch/HnswIndex.cs#L99-L110) при таком topK задаёт ef не меньше topK. Таким образом при N вернувшихся кандидатах и Q совпавших query terms source-код имеет O(N²·(1+Q)) повторной обработки обученных текстов без учёта стоимости отдельной токенизации и ANN. Это code-level complexity, не измеренное wall-clock.

**Прокси-count:** при допущении N кандидатов и 6 совпавших терминов число вызовов Tokenize, приходящихся на lexical processing, составляет 456 для N=8, 1808 для 16, 7200 для 32, 28736 для 64 (по выражению N + N²(1+Q)); это расчёт операций, не тайминги. Нужен живой 8/16/32/64 memory-scale benchmark с неизменным heldout, warmup и измерением p50/p95/allocations; отдельно оценивать, переводится ли рост latency в timeout, fallback/abstain и user-quality loss. В простом offline RP01 этот scorer не вызывается; нельзя объяснять им 6272 секунды DPO training.

## Линии доказательств и статус

- **C:** ScoreCandidates реализует текущий maxLexical и повторный full-memory BM25 scan; старые ветки содержат этот scorer.
- **P (PROXY):** точная арифметика fusion допускает B→A на заданных фиксированных raw features; упрощённая оценка сложности растёт квадратично.
- **H:** добавление реального урока изменяет качество обычного consumer или вызывает timeout; требуется реальное исполнение.
- **U:** не изучены фактическая частота такого случая, обобщение по доменам, значение для RP01 standalone free generation, влияние на SLA.
- **НЕ ПРОВЕДЕНО:** обучение/Unit.ExecuteAsync, независимая CODEX/Astra 6 UltraCode оценка, fresh blind semantic grade, Windows CI.

## Связь с существующими исследованиями

[09 — отличие исторического E34V2 scorer](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [12 — matrix→KNN и некорректные historical сравнения](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [16 — tier imbalance](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md), [18 — real ProductHost route split](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md). N20.01 исследует **новую конкретную зависимость score от наличия другого кандидата**, а N20.02 — **стоимость текущей реализации scorer на большом банке**, не притворяясь независимыми повторениями старых E34 испытаний.

**Исполняемый live work order и stop rules:** [20-MICRO-FUSION-POOL-INVARIANCE-EXPERIMENT.md](20-MICRO-FUSION-POOL-INVARIANCE-EXPERIMENT.md).

## Непереопределяемые DPO-роли

**CODEX с GPT-6 Astra 6 UltraCode выполняет человеческий выбор учебного урока; внешняя разрешённая открытая китайская модель создаёт ОБЕ финальные стороны DPO (chosen + rejected); CODEX с той же Astra 6 UltraCode независимо оценивает качество ОБЕИХ сторон и всей пары.** Оценка downstream Student после обучения отдельна. Эти обязанности не передаются арбитру KNN, Student или детерминистическому генератору. В настоящем N20 KNN-proxy новые DPO-пары вообще не создавались. [WorkLeft-02](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-02-teaching-chinese-model.md#L6-L18).

**Skill coverage:** human-readable-architecture-audit, phase-planning, plan-review, generated-code-review, prompt-patch-authoring, experiment-design, learning-path-audit, systematic-debugging (core docs/skills); независимая model-based review не вызывалась.
