# N30 MASTER — покрытие возможных причин ухудшения качества RP01, исторические победы и новые falsifiers

**Дата:** 16.10.2026. **Индекс и независимая дельта к N01–N29, не перепись чужих находок.** \`xiigen-planning@llm_plan3\`, исходники проверялись на \`llm_mvp_core@79082340f01b744cdf08185616f2bd818dbd06b8\`. Сверены четыре входа RAG (\`original task.md\`, \`09.09.26WorkLeft-RAG-MASTER-INDEX.md\`, \`09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md\`, RP01 current diagnostic); существующие crosswalk и ряд прямых C# source/исторических результатов. **НЕ** выполнялось полное чтение каждого RAG/каждого коммита/каждого файла всех соседних веток. **НЕ** новый native ML run, **НЕ** CODEX/Astra independent review. За пределами указанного planning каталога ничего не меняли.

## Самое важное причинное разделение

\`\`\`text
CODEX/Astra 6 UltraCode = human-role selector (какой урок учить)
        ↓
разрешённая external OPEN CHINESE model = final chosen + rejected author
        ↓
CODEX/Astra 6 UltraCode = independent exact pair quality judge
        ↓  provenance/admission + exact bytes
┌─────────── neural RP01 ───────────┐  ┌───── typed memory / structured Unit ──────┐
│ B0 random → DPO → q/v LoRA        │  │ DPO law → tokenizer, Executor, Verifier  │
│ → checkpoint → TextGenerator     │  │ → KNN/phrase/wordmap → Flow / Adapter     │
└────────────────────────────────────┘  └──────────────────────────────────────────┘
        ↓ ordinary free output                      ↓ ordinary free output
              CODEX/Astra 6 UltraCode independently judges each and retention
\`\`\`

Провал одного learner не равен провалу другого. Сохранённые RP01 пары C#500 и ENRU500 изменили веса, но свободная semantic панель осталась 0/3→0/3 и 0/4→0/4. Нарушение domain-learning может возникать как на \`pair→gradient\`, так и на \`law→Unit→inference\`, и в наблюдателе качества.

Уровни: **C** реальный source contract; **M** исторический результат, ограничен тем runner; **S** историческое требование/архитектурное расхождение; **H** causal hypothesis ещё не проверена; **U** не было достаточного доступа/нового опыта. Тип P0/P1/P2 означает **приоритет изучения**, не установленную вину.

## 26 точек возможной потери качества: где уже есть доказательства

| № | Различие / адрес возможной потери | Лучший существующий evidence owner | Что измерить, доменная применимость |
|---|---|---|---|
| **01 P0** | Random B0 vocab629, hidden512/4 layers без самостоятельной компетенции (C/M) | [RP01 causal](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) | Can overfit 8–32 SFT; teacher-forced и free-generate. **Neural any generative domain** |
| **02 P0** | Псевдопары / не тот автор final \`chosen/rejected\` / неверно выбранный опыт (S/H) | [DPO conformance](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md), [02 DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md) | CODEX selected→Chinese authored both→CODEX independently judged; exact provenance and semantic contrast |
| **03 P0** | 500 строк лишь 25 semantic families, source-family leakage, duplicates (M/C) | [02 DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [RP01 diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) | Distinct lawful families, negative quality, disjoint holdout; **neural** |
| **04 P0** | DPO chosen/rejected token masks, sign, reference, prompt leakage, alignment, EOS/truncation (C/H) | [02 DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) | 4 pairs raw→ids→mask; finite-difference native; confirm **actual RP01 exact path**, not legacy hashed bridge |
| **05 P1** | TeachingVector semantics collapse to scalar and per-batch optimizer normalization (C/H) | [N26 AdamW](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) | Positive/negative pair gradients under matched optimizer step; **vector DPO path only** |
| **06 P1** | LoRA q/v-only, rank8, alpha16, base frozen, target capacity/adapter routing (C/H) | [03 LoRA/vector](03-LORA-TOKENIZATION-VECTOR-SPACE.md) | Rank/target one-factor after B0 train-overfit; current RP01 dropout0, **не обвинять default dropout0.05** |
| **07 P1** | BPE undercoverage, saved vocab/hash mismatch, BOS/EOS/length decoding (C/H) | [03 LoRA/vector](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [21 BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md) | same raw string train/test/cold restore IDs; ENRU/HE/mixed script |
| **08 P1** | Logit/probability меняются, но argmax/stop/decoder не меняют смысл (M/H) | [10 stop](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [RP01 diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) | first+last token and forced-prefix vs free, fixed sampling |
| **09 P1** | Неверный checkpoint/reference/optimizer/RNG ID; storage без behavior restore (C/H) | [06 restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [04 base LoRA](04-BASE-LORA-CHECKPOINT.md) | cold new process, identical tokenizer/model/reference/optimizer + actual answer |
| **10 P0** | RP01 \`TextGenerator\` vs separate structured learned KNN Unit: learner/consumer mismatch (C) | [26 lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [ProductHost bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | 2×2 Student/Unit+memory at same input; **cross-architecture** |
| **11 P1** | DPO→3 typed sub-lessons может не доставить chosen/rejected в правильный Unit, silent dispatch (C/H) | [21 structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [17 dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) | exact \`UnitId\`, emitted lessons, state hashes, actual downstream |
| **12 P1** | Tokenizer обучается самоподтверждению \`SemanticSignature\` / Verifier только negative examples (C) | [27 heldout bias](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md) | balanced positives/negatives, 2 source languages; **structured** |
| **13 P1** | Словарь E26 context/POS превращён в surface/(lemma,concept), cross-language keys и last write (C/H) | [23 E26 Unit](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [N26](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md) | same words, changed left/right POS, masked/exact vs new input; **RU/EN grammar** |
| **14 P1** | POS tree/AST/ordinal position, E27 reorder, prevConcept between clauses, sourceLang \`RU\` vs \`ru\` (C/H) | [23 language/POS](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), [E07 misread](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | isolate syntax tree vs word order; **code/translation**, not uniform |
| **15 P1** | KNN ID aliases, ANN/HNSW recall, predicate vector omitted, index rebuild changed (C/H) | [14 KNN identity](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [N28 HNSW](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) | exact brute force vs ANN recall and ID, after restore; **retrieval** |
| **16 P1** | Prefix/full/structure, shared memory language/domain, cutoff k **before** direction check (C/H) | [16 new KNN](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md), [29 annotation](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) | same source 2 directions, candidate #6, no foreign topK; **multilingual** |
| **17 P1** | \`not/no\` ignored, \`Unknown\` compatibility accepted, topic/counterexample scope (C/H) | [N27 polarity](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | negative source-law candidate gate and calibrated abstain; **English KNN** |
| **18 P1** | kNN \`hits[0]\` vs ranking law/domain/context, BM25 hybrid and per-pool normalization (C/H) | [04 KNN hybrid](04-KNN-HYBRID-RETRIEVAL.md), [20 nonlocal](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md) | matched memory adaptive k + hard law gate, monotonic old-case retention |
| **19 P1** | Различный смысл positive, rejected, warning; verifier accumulated AND, parsing roles wrong and cross-direction rules (C/H) | [23 verifier](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md), [28 parser](28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK-2026-10-16.md) | good old answer after adding new lawful correction; user route with verifier |
| **20 P1** | Phrase composer выбрасывает неизвестные критические слова, а confidence bypass verifier (C/H) | [21 phrase dropout](21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md) | \`never/must/may\` source truth retention; **translation clauses** |
| **21 NEW** | **N30-01:** leftmost phrase greedy не генерирует альтернативу при **100% coverage обеих** (C/JS) | [N30 source audit](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md) | choose among alternative spans, independent domain rule, **translation/domain phrase** |
| **22 NEW** | **N30-02:** \`can't/can\` same KNN guard set, 2nd source check 2/3 passes (C/JS) | [N30 source audit](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md) | original raw polarity vs collapsed; **EN→RU/HE** |
| **23 NEW** | **N30-03:** loss of \`->\` creates fake adjacent trained phrase with full coverage (C/JS) | [N30 source audit](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md) | shadow source-span operator barrier; **AST/DSL/graph**, not all domains |
| **24 P1** | Replay aperture/DPO vs KNN recipients, continual forgetting and false \`no-op\` readiness (C/M/H) | [27 replay](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md), [07 replay](07-CONTINUAL-REPLAY.md), [27 reset](27-NEW-STRUCTURED-READINESS-RESET-AFTER-NOOP-BATCH-2026-10-16.md) | before/after old+new blind, one-pair/no-repeat contract, **after proven initial skill** |
| **25 P1** | Unit vs Flow vs async Orchestrator vs higher-order Graph и learned memory not consumed / hidden deterministic fallback (S/C) | [05 Units/Flow/Graph](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [26 lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) | caller→actual state mutation→final answer trace for each **typed recipient**, not name-only |
| **26 P0 observability** | training metric / Unit accuracy / product heldout scorer не проверяют independently semantic ordinary answer (C) | [27 metric](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md), [28 product](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md) | new-family, chosen and rejected, false positives/negatives, retention, target/domain, cold restore |

**Не считать 26 пунктов доказанными causes.** Некоторые взаимозависимы, некоторые наследованы старой архитектурой, многие не достигаются текущим RP01 \`TextGenerator\`. Перечень покрывает **семейства** возможных причин и даёт ссылки на существующие detailed DIF, а не логически все возможные баги в неизвестных коммитах.

## Реально успешные старые механизмы — подтверждение из сохранённых run JSON, не по названию эксперимента

| Архив/ветка | Зафиксировано | Ограничение переноса |
|---|---|---|
| [E26 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) | disambiguation accuracy **0.4→1.0**, 15 cases, 6 hard, \`passed=true\` | pretrained/default hardcoded mappings possible; это **не** matched generalization текущего translation Unit |
| [E14 Tier2 2026-04-30](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-001.json) | 16 train/4 val, top1 **.75→1.00**, PASSED | four val и 2 classes, ограниченная мощность вывода |
| [E14 Tier3](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-002.json) | 220 train/56 val, top1 **.8393→.875**, \`passed=false\`, \`balance=.0179\` | другой набор/сильно несбалансированный, не «опровергнут kNN» |
| [E43](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | retention **.3226→.3978**, old465/new414/replay177, passed | биграмный learner и другой protocol; не применять незаметный replay к текущей уникальной DPO pair |
| [E09](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | archived top1 **.2413→.3507**, \`passed=true\` | [код](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L78) baseline/enhanced сравнивает **разные val subsets** |
| [DesignToCode V210](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v210-good-external-plus-internal-guide-full-20260511-014712/internal-train-ready-dpo-summary.json) | **134 pair, 9 batches, train accuracy .8333, margin +.005703**, \`StepMany\`+checkpoint in [runner](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L45-L100) | без независимой free-generation; иной context/vocab/model и иной source |
| [E13B record](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) | 100% известные train expression keys, 0% новых heldout, abstain100% | полезная exact memory + abstention, **не** генерализация |

**Кандидат, который описала Люба как «неудачная матрица → очень успешный kNN»: пока не идентифицирован однозначно.** Existing [12 misreads](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) доказывает, что E07 «добавление AST_parent» не обучало заявленные дополнительные поля, а [12 provenance](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) и [26 lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) дают donor candidates E09/E14/E40V2/E38V2. Чтобы назвать exact migration, необходимы все **4**: старый source+result, commit переноса feature/mapping, новый KNN caller, same-panel gain. Нельзя записать эту причинность из похожих имён.

Рядом по истории присутствуют ветки \`DesignToCode\`, \`DesignToCode-clean\`, \`language_translate\`, \`external_domains\`, \`orchestrator-extensions\` (существование через GitHub branch listing). Прямо прочитан V210 исходный DPO runner; **проверка полного lineage всех веток не завершена**, поэтому ни полная реконструкция оригинальной matrix-success функции, ни полное отсутствие нужного кода в ветке не утверждаются.

## Доменные оговорки, обязательные перед копированием алгоритмов

- **EN/RU/HE/ES:** polarity scope, subject/verb agreement, pronouns, contextual noun heads, order, morphology, mixed script; классический raw bag-of-words/ANN недостаточен, но AST/code-scoped filter не всегда применим.
- **C# / design-to-code:** AST parent, method role, null safety, semantically typed operands, operator precedence, dependency graphs; перенос word-order scorer с EN→RU может ухудшить. Compiler/build не заменяет выученный law.
- **SQL:** NULL трехзначная логика, dialect, operator binding, alias/joins, AST roles; обрезка \`=\` / \`IS\` и order может поменять закон, даже если embeddings похожи. SQL eval отдельно.
- **Graph/workflows/DSL:** \`->\`, \`=>\`, scope and edge direction are data; specific operator-preserving matching. Not global for natural punctuation.
- **General episodic/task routing:** contextual kNN, trust/skill scorer and replay may help, but pure translation morphology or SQL AST cost unnecessary.
- **Все домены:** correct source-law representation, train/serve fingerprint, candidate→consumer reachability, real cold reload, independent domain law judgment and retention.

## Решение для исполнителя: максимальная информация при минимуме повторных runs

1. **Сначала RP01 P0 learner gate:** 4–8 real authorised DPO pairs with CODEX-selected cases and independent CODEX/Astra review of *both* Chinese-authored sides → exact masks/finite difference → tiny 8–32 chosen-only overfit vs DPO-only with matched budget → free generation with separate holdout. Если overfit даже train не работает, диагностировать B0/gradient/target/optimizer; **никакого Main500 restart**.
2. **Одновременно 3 новых маленьких Unit pilots** [MICRO-N30](30-MICRO-NATIVE-CAUSAL-PILOTS-AND-CODEX-EXECUTOR-WORK-ORDER-2026-10-16.md), вначале prove reachability. \`N30.01\` alternatives/reranking; \`N30.02\` contraction polarity; \`N30.03\` operator barrier. Изолировать, не смешивать, source family disjoint, cold restore.
3. **Исторический donor trace** из веток: E07/E09/E14/E26/E40V2; восстановить concrete feature/vector weights, copied code and consumer caller. Подтвердить на одинаковых исходных input/gold, иначе написать \`DONOR_UNRESOLVED\`.
4. **Только если P0 neural learner доказан:** 2×2 learned Student vs Units and combined; измерить semantic correctness, critical-law violations, calibration/abstain, old-law retention и расходы, не счётчики tests. Не сливать владельцев RP01/RP02/RP03/RP04.
5. После native A/B только при подтверждённой причины — минимальный source patch **на correct owner** с exact independent code/plan review; заменить неверные RAG-интерпретации по факту, исторические receipts не изменять.

## Проверка полноты, честность и ограничения

- **Source verified:** точные C# caller/source seams N30, основные RP01 masks/LoRA forward/actual decode, archived E26/E14/E43/E09 run JSON и DesignToCode DPO runner.
- **Source-only analytical test actually executed:** \`node 30-SOURCE-LOGIC-PROXY.mjs\`; numeric output в [JSON](30-SOURCE-LOGIC-PROXY-RESULT.json). Первичная неверная догадка про order-of-lessons **отвергнута**, остаётся missing alternative composition.
- **Not executed:** native C# probes, new Chinese-authored DPO, CODEX/Astra review of this package, independent blinded heldout and actual Student improvement. No silent claims of skill/model invocation.
- **Hard write boundary:** все новые файлы **только** \`xiigen-planning/llm_plan3/.../replanning/possibleDifs\`; \`llm_mvp_core/docs/plans/replanning-possible-diffs\` **READ_ONLY**.
- **Future correction:** если найден N30 duplicate в ином старом файле, downgrade novelty and link original; если caller unreachable, downgrade quality impact to \`ROUTE_NOT_REACHED\`; если source изменился, freeze new SHA.
