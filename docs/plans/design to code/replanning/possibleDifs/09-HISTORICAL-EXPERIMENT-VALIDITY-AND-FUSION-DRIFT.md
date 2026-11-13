# 09 — Важный пропуск: неэквивалентные historical experiments и drift механизма fusion

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — P30/RF03

У текущего P30 есть evaluator по observations, но постоянная ничья (1,1)/(1,1) ошибочно считается crossing. Проверять назначенные alpha A/B и выбранный observable, а не дополнение к strict dominance. Positive real crossing и negative identical alpha обязательны; reader пересчитывает saved data с Train0. Исторический E34 mismatch denominators не превращён в доказательство нового gain.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->
**16 октября 2026 · дополняет 01 и 04 · статус:** CODE_CONFIRMED для указанных формул / REPORTED_RUN для чисел; причинность деградации RP01 NOT_MEASURED.

## Главный вывод

Нельзя формулировать «успешная E34V2 hybrid-архитектура перестала работать из-за плохого port». **В исторической E34V2 сравниваются разные выборки и другая формула**, а текущая гибридная реализация использует иной encoder, калибровку, веса и selection. Следовательно, нет чистого A/B-доказательства, что эффект старого алгоритма действительно переносится на нынешний домен.

## X09-01 — E34V2 baseline и enhanced оцениваются на разных validation cases
- **CODE** [E34HybridMatcher.Run:18–22](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L18-L22): train80% и val20%.
- **CODE** [строки 32–41](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L32-L41): baseline BM25 считает hit@1 и hit@3 по **всей val**.
- **CODE** [строки 44–58](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L44-L58): bestAlpha выбирают на **первой половине val**, enhanced hit@1 считают только на **второй**.
- **ARTIFACT** [E34V2 result](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E34V2-HYBRID-MATCHER/results/run-2026-05-01-001.json): числа **0.7222→0.7778**, trainN72, valN18, bestAlpha .10, 10.5→106.7 мс.
- **Следствие:** baseline denominator=18, enhanced denominator=9 (при valN18). Число +7.69% — не unbiased paired treatment effect (хотя улучшенный arm мог реально быть лучше). При таком объёме изменение **одного** case меняет процент на 11.1 пункта enhanced. Скорости также охватывают разные объёмы работы, поэтому нельзя непосредственно сравнивать total milliseconds.
- **Исправление оценки:** выучить alpha на **train/internal tuning только** или на первой половине val, затем frozen alpha сравнивать **обе модели на одинаковой второй половине**; дополнительно независимый final test и paired case table, bootstrap по source families. Никакого leakage из final test в alpha tuning.

## X09-02 — E34V2 не совпадает с текущим transformer hybrid даже формулой
**Исторический алгоритм:** [FusedRank:80–112](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L80-L112) нормирует BM25; второй компонент — **Jaccard similarity по FamilyCollapser.CollapsedTags** и max по классу; `Score=alpha*BM25+(1-alpha)*Jaccard`. [TuneAlpha](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L115-L133) подбирает alpha из .1… .9.

**Нынешний другой алгоритм:** [TransformerEmbeddingKnnLogicalUnit.Train:114–165](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L114-L165) создаёт transformer embedding (`maxLen=min(64,contextLen)`) + HNSW. [ScoreCandidates:216–242](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L216-L242) использует **фиксированный** `0.45*vectorScore+0.35*normalizedLexical+0.20*patternScore`; pattern — equality classifier. Нет прямой зависимости текущих коэффициентов от bestAlpha=.10 исторического эксперимента. [Lexical score](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L246-L285) — упрощённая BM25-like формула поверх `TrainingVectorBuilder.NormalizeRoots`, **не** обязательная exact E15 preprocessing. [Infer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L181-L210) возвращает `example.Output` лучшего кандидата verbatim, а не новый генеративный ответ.

**Положительный факт:** гибридная сортировка **существует** в `TransformerEmbeddingKnnLogicalUnit`; нельзя объявлять BM25 полностью отсутствующей. **Отрицательная граница:** фактический RP01 offline `StepVector` не вызывает этот scorer. Наличие в `LlmIngredientManifest` слов `E15`, `E25`, `E34V2` [строки 138–153](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L138-L153) не доказывает bit-for-bit реализацию экспериментов.

**Falsifier:** одновременно запустить old E34 formula и current fixed 0.45/0.35/0.20 на **одном** frozen task set со стабильными embeddings/index и единой candidate pool; сравнить hit@1, hit@k, law-compliant downstream answer, latency и memory. Отдельно отключать lexical/pattern/vector; не тюнить на exam.

## X09-03 — E09 заявленные +45% KNN также не являются чистым paired результатом
[E09StructuralKnn.Run:30–79](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L79) baseline bigram считает accuracy на **всём val**, KNN — на `val.Take(500)`. [JSON E09](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) показывает 0.2413→0.3507, но эти числа имеют разные denominator и possibly difficulty distributions. **При этом структура E09 не BPE-text kNN:** [BuildTokenVectors:102–123](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L123) применяет Roslyn `SyntaxKind` и `StructuralVectorExtractor`, в отличие от hashed-bag-BPE KNN в проекте 21.

**Дополнительный source-risk:** E09 создаёт `kindMap` отдельно для каждого файла [строки 107–120](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L107-L120). Одна и та же Roslyn `SyntaxKind` может получить разные `BpeId` в файлах с различным множеством syntax kinds; это может загрязнять labels bigram и KNN. Не утверждать, что исторический запуск наверняка пострадал: требуется вывести соответствия и пересчитать с global map.

## X09-04 — E15/E17 проверяли другие objective/алфавит, не семантические токены LLM
[E15SemanticTokenizer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E15SemanticTokenizer.cs#L8-L16) сравнивал perplexity **bigram** по identifier words before/after stopword removal+stemming. [Код вычисления](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E15SemanticTokenizer.cs#L73-L94) делит token sequence 80/20 и учит маленький bigram с OOV fallback. RP01 использует `BpeTokenizer` с UTF-8 vocabulary, **другую единицу prediction** (next token LM). Поэтому исторические улучшения perplexity не являются evidence лучшего DPO/LoRA. Stopwords `not,no` и negative-meaning требуют law-preserving probe [R04](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/results/R04/FINDINGS.md#L52-L58).

## X09-05 — Positive controls и negative controls должны идти вместе
| Результат experiment | Что именно измеряет | Ограничение |
|---|---|---|
| [E34V2](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E34V2-HYBRID-MATCHER/results/run-2026-05-01-001.json): 0.7222→0.7778 | episode retrieval hit@1 | разные val subsets; current scorer другой |
| [E09](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): .2413→.3507 | next AST kind prediction | не совпадающая val length; global `kindMap` проверить |
| [E15](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E15-SEMANTIC-TOKENIZER/results/run-2026-05-01-001.json): 229.8→207.6 | bigram perplexity | смена токенов и objective |
| [E29V2](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E29V2-DPO-PAIR-VALUE/results/run-2026-05-01-001.json): .2308→.2308 | bigram/weighting proxy | улучшения **нет**; не DPO policy/reference |
| [E31V2](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E31V2-CONTINUOUS-LOOP/results/run-2026-05-01-001.json): .2807→.2807 | continuous loop proxy | улучшения **нет** |
| [E44V2](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E44V2-DATASET-WEIGHTER/results/run-2026-05-01-001.json): .1663→.0039 | weighted F1 | значительное ухудшение при naive reweight |
| [E43](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json): .3226→.3978 | old-task retention proxy | replay-gradient нарушает новый one-pair/no-repeat; retrieval memory отдельно |
| [E26](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json): .4→1.0 | disambiguation on 15 cases | tiny dictionary control; не DPO/LoRA |
| [E27](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E27-PATTERN-REORDERER/results/run-2026-05-01-001.json): .2857→1.0 | order exact on 7 cases | tiny rigid correction; не free translation |

## Проверка гипотезы «утраченная деталь эксперимента»
1. **Повторить правильную задачу** (например AST-next-kind) c paired eval и строго общим val.
2. **Перенести ровно механизм/признаки**, а не только имя E09/E34: Roslyn AST, Jaccard tag collapse, source-aware admissible pool и scorer version. Отделить это от neural Student.
3. **Оценить downstream utility** при неизменном Student; затем только рассмотреть обновление LoRA/Unit и новую held-out semantic family.
4. **Присвоить честный статус:** прежняя метрика «REPORTED_UNPAIRED», новая «PAIRED_REPRODUCED / NOT_REPRODUCED». Старые числа в отчёте не стирать, в новых графиках обозначить ось и denominator.

**Цена вмешательства:** сначала code-level calculation/pure function + набор существующих валидных cases, затем короткий read-only inference. GitHub Windows CI не является необходимым доказательством первых четырёх расхождений.
