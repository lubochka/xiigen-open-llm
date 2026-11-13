# N38 — Replay fusion: lexical shadow вместо независимого KNN/pattern и отключение ranking на коротком списке

**16.10.2026 · NEW DELTA / SOURCE_CONFIRMED / END-TO-END QUALITY NOT_RUN / independent CODEX/Astra review NOT_RUN.**

**Scope:** историческое обучение generated capability через `DefaultLogicalCapabilityTrainer → ReplayFusionRanker → replay DPO/KNN corpus`; **не** прямой `RP01 SimulationDpoExperiment → DpoTrainer/LoRA → CodeDocumentationStudentGeneration`. Это унаследованный код: аналогичные строки прочитаны в `DesignToCode` и `language_translate`; утверждения «регрессия внесена PR RP01» нет. Проверены `llm_mvp_core:codex/dpo-implementation`, `possibleDifs` N09/N20/N27/N33/N37, и read-only core research. Полный дедуп всех >300 соседних файлов не заявляется.

**Результат:** обнаружены три проверяемых, но не тождественных эффекта в одном действующем path. В отличие от старых описаний дрейфа коэффициентов и replay aperture, здесь проверяется **природа самих трёх score-компонентов** и отдельный short-list control-flow.

## 1. Точная source→learner→consumer цепь

| Этап | Проверенный факт | Доказательство |
| --- | --- | --- |
| Протокол эксперимента | Historical E34 `FusedRank`: BM25 + Jaccard по `FamilyCollapser.CollapseString`, коэффициент `alpha` выбирается отдельным `TuneAlpha`. Это не готовый Transformer embedding. | [E34HybridMatcher.cs L80–135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L80-L135) |
| Реальный replay ранкер | `ReplayFusionRanker.Rank` заявлен как E34V2 BM25+KNN; фактически `vector=ComputePredicateOverlap(compiledText, TrainingText)`; `pattern=vector`; `fused=.45*vector+.35*lexical+.20*pattern`. | [ReplayFusionRanker.cs L8–37](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/ReplayFusionRanker.cs#L8-L37) |
| Что такое «vector» | `ComputePredicateOverlap` = Jaccard от `Tokenize(a)`, `Tokenize(b)`. `Tokenize` вызывает `TrainingVectorBuilder.NormalizeRoots` и строит dictionary текстовых слов; в этом методе нет TransformerModel embeddings/HNSW, AST, POS или `PredicateIds` manifest. | [ReplayFusionRanker.cs L39–89](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/ReplayFusionRanker.cs#L39-L89) |
| Флаг и реальный trainer | `UseReplayFusionRanking` приводит к `SelectAndValidateReplaySnapshot`; список verified manifests сортируется по `CreatedAtUtc`, затем `Rank(... topK:3)`; выбранное поступает в `AutonomousLearningReplayMixer`. | [DefaultLogicalCapabilityTrainer.cs L490–550](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L490-L550), [L1850–1883](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1850-L1883) |
| Второй recipient | Выбранные replay manifests также используются для построения нового KNN corpus/TransformerEmbeddingKnnLogicalUnit. **Здесь** уже другой, настоящий embedding-KNN механизм; смешивать его scoring с replay-ranker нельзя. | [DefaultLogicalCapabilityTrainer.cs L948–980](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L948-L980), [L3467–3504](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3504) |

### N38-A — «векторный» канал не содержит векторного embedding-сигнала

Условие: `UseReplayFusionRanking=true` и verified candidate manifests более трёх. Код вызывает `ComputePredicateOverlap`, который получает словарные roots из **тех же строк текста**, что читает BM25. Имя переменной `vector` и комментарий `BM25+KNN fusion` не добавляют математике learned vector. Это source-confirmed **lexical-shadow**. Нельзя утверждать, что во всём продукте нет KNN: `TransformerEmbeddingKnnLogicalUnit` действительно использует embedding + ANN; речь строго об отборе **старых учебных manifest до обучения**.

**Возможное следствие для качества (H, не доказано):** две записи со сходным root-bag, но разной ролью субъекта, порядком, scope отрицания или AST-вложенностью, получат одинаковый overlap. Более подходящий старый урок не попадёт в top3, хотя основной KNN способен различать эти роли. Старый результат E09 structural-KNN и E26 POS не возникает автоматически в этом replay selector.

### N38-B — независимый pattern-канал схлопнут в копию lexical Jaccard

В коде буквальное `pattern = vector`, поэтому формула точно сводится к

`score = 0.65 × Jaccard(NormalizeRoots(query), NormalizeRoots(trainingText)) + 0.35 × BM25Like(query, trainingText, corpus)`.

**C:** отдельный вес 0.20 НЕ выделяет независимому pattern/AST/POS/classifier ни одного параметра. Это не просто «коэффициент 0.45 не совпадает с E34»: компонент данных отсутствует. Следовательно, изменения лишь конфигурационного `pattern weight` без реального extractor математически не восстановят условно полезный structural transfer.

**Семантический контрпример без утверждения о runtime:** два C# урока с одинаковыми именами и словами, но различием `if (cancelled) await ...` против `await ...; if(cancelled)...`; два SQL-урока с перенесённым predicate у `LEFT JOIN` из `ON` в `WHERE`; EN→RU пары «Anna follows Boris»/«Boris follows Anna». Word bag одинаков, роль/порядок отличаются. Каким именно root set окажется запрос, проверять существующей функцией; этот пример не является native measured output.

**H:** source-aware structural feature в replay ranking увеличит `correct_law@1` или `retrieved_relevant_teacher_id@3` на source-family-disjoint задачах указанного домена при неизменном DPO/student/index. H0: после прочих фильтров правильные варианты и так попадают в top3; pattern-adapter не нужен/даёт регрессию.

### N38-C — при 1–3 manifest `Rank` вообще НЕ вычисляет релевантность

[ReplayFusionRanker.cs L14–23](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/ReplayFusionRanker.cs#L14-L23): если `manifests.Count <= topK`, возвращается `manifests.Take(topK)` без счёта `compiledText`, BM25, Jaccard, сортировки по score. Caller уже сортировал их по времени ([L1866–1878](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1866-L1878)). Значит при двух старых уроках флаг «fusion» имеет **нулевой эффект даже на порядок**; при четырёх тот же запрос включает ranking. Существующая граница `topK=3` создаёт дискретное изменение механизма при росте истории с 3 до 4.

**Очень важная причинная граница:** при 1–3 manifest все candidate IDs уже попали в returned set. Если downstream использует весь набор независимо от порядка, это **не теряет ни одного урока**; нельзя называть данный механизм standalone причиной ухудшения. Отдельно уже известно, что DPO replay-mixer снова сортирует по времени (см. N27). Проверять, кто реально обрезает/переупорядочивает, и как это отражается на gradient/KNN. Для direct RP01 этот selector не установлен.

## 2. Проверка истории и новизны

- [N09 — mismatch historical E34 and current fusion](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) уже сравнивает E34 и **другой** настоящий embedding-unit score; это не новая находка.
- [N20 — maxLexical candidate normalization/memory growth](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md) изучает изменение score при добавлении кандидата в `TransformerEmbeddingKnnLogicalUnit`, не подмену `ReplayFusionRanker.pattern = vector`.
- [N27 replay aperture and order reset](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) уже объясняет top3, повторную сортировку и отсутствие транзитивной KNN-истории; **не повторять это как новизну N38**.
- [N37 — E31 unconditional routing](37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md) — иной learner и другая задача.
- Старые `DesignToCode` и `language_translate` содержат те же строки `vector=ComputePredicateOverlap; pattern=vector; Count<=topK`. Следовательно, **унаследованный дефект/ограничение fidelity** относительно рекламируемой/исторической функции, а НЕ доказанное ухудшение от последнего merge.
- Claim novelty ограничен обследованными индексами и файлами; если найден более ранний текст про именно этот seam, перенести ownership ссылкой, НЕ удваивать finding.

## 3. Доменность и приоритет

| Область | Когда структурный pattern может отличить | Текущий вывод | Приоритет проверки |
| --- | --- | --- | --- |
| C#, Angular/TypeScript, SQL | AST parent/callsite, ветвление, `WHERE` vs `ON`, negation-scope | Без AST/POS feature в replay-score не видны; **не менять общий ANN без A/B** | P1 для generated capability replay |
| EN↔RU, HE/ES | subject/object, POS, порядок, морфология, scope `не` | Требует грамматического extractor; Roslyn AST неприменим к свободному тексту | P1 условно |
| Строгие regex/exact ID | точный ключ закона может быть важнее syntax-order | Дополнительный pattern может вредить; оставить control | P2 |
| Прямой RP01 C#500, ENRU500 | `SimulationDpoExperiment` exact-v2 → `DpoTrainer` → neural `TextGenerator` | **NOT_APPLICABLE** без конкретного caller trace | Не объясняет frozen 0/3 и 0/4 |

## 4. Быстрый falsifier вместо переписывания матрицы

[Исполнимый малый протокол](38-MICRO-REPLAY-FUSION-DOMAIN-CAUSAL-EXPERIMENT-2026-10-16.md): (1) 4–6 verified same-domain manifests; (2) trace actual candidate IDs, query roots, `lexical`, `vector`, `pattern`, selectedTop3, replayDpoIds, knnCorpusIds; (3) no-feature vs AST/POS/order-feature; (4) source-disjoint blind ordinary Flow correctness, ≤12 вопросов на домен; (5) controls 2–3 candidates для Count gate и shuffled order; (6) холодная загрузка того же learned state. **Новый .NET run, DPO, Student-call, CODEX/Astra judge в этом исследовании НЕ выполнялись.**

**Защищённая поверхность:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**` только читается. Плановые документы публикуются только в `xiigen-planning/llm_plan3/.../possibleDifs`.
