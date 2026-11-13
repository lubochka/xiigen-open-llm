# N30 — независимая карта возможных причин провала RP01, исторических успешных механизмов и проверки качества

**Дата:** 16.10.2026. **Источники:** xiigen-planning/llm_plan3; llm_mvp_core/codex/dpo-implementation, наблюдаемый SHA 79082340f01b744cdf08185616f2bd818dbd06b8; первичные записи экспериментов из соседних веток, RAG и текущие C# callers. **Режим:** source/document audit; native learning и независимая предметная оценка НЕ выполнялись. Данный файл — уточнение/маршрутизатор, не утверждение о полном прочтении всех RAG и всех параллельных аудитов.

## Принцип исследования

Нельзя объединять в одном «проценте обучения» четыре разных получателя изменений:
1. **RP01 neural Student:** случайная B0 → masked preference pairs → DPO/LoRA q/v → weights → free token decoding.
2. **Discrete trainable Units:** DPO chosen → typed semantic tokenizer / Executor word+phrase maps / KNN / learned verifier.
3. **Гибридные функции:** E26 POS/соседство, source/AST structural features, lexical/semantic KNN, BM25 fusion, law guard, replay и разрешение конфликтов.
4. **Actual user consumer:** какой Flow/Generator действительно прочитал сохранённое знание после fresh restore.

Старый успешный KNN и нынешняя свободная генерация измеряли разные способности. И наоборот: в соседней ветке [DesignToCode](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) были настоящие обновления нейронных параметров StepMany, а не только словари; высокие train показатели сами по себе не доказывают unseen response correctness.

Прямо прочитанные исходные RAG: [алгоритм создания DPO](../../Алгоритм%20создания%20ДПО.md), [Master RAG](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) и [usage guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md). Они фиксируют ILearningUnit→Flow→Orchestrator→DynamicGraph, каноническую идентичность пары, source/legal provenance, checkpoint и запрет путать запись с обучением. RAG — указатель, а не runtime evidence.

## Реестр категорий и главных владельцев доказательств

| ID / контур | Возможный пробел | Документальное или исходное основание | Где и как проверять |
| --- | --- | --- | --- |
| 01 Student B0 | Random untrained vocab629 и мало опыта для связного языка; DPO не заменяет обучение базовой способности | [RP01 causal pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) | P0 neural, matched chosen-only warmup vs DPO-only |
| 02 DPO pairs | Неверная chosen/rejected, mismatch target, плохой contrast, shared prefix, family leakage | [Data/Objective](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) | 4–8 pair IDs, actual model token IDs, независимый domain judge |
| 03 DPO math | Mask/offset, reference, beta, sum-vs-mean, знаки градиента, AdamW | [DpoTrainer L832–940](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/11.DPO/DpoTrainer.cs#L832-L940) | Tiny directional derivative; не объявлять ошибку без counterexample |
| 04 LoRA | q/v-only rank8 alpha16, optimizer state, freeze/restore, малый полезный gradient | [RP01 mechanism review](../RP01-LEARNING-MECHANISM-REVIEW-2026-10-16.md), [LoRA](04-BASE-LORA-CHECKPOINT.md) | Real delta logits + same-budget capacity arm только после корректной math |
| 05 Tokenizer | BPE/vocab retrain, EOS, 64-token truncation, hashed bag вместо порядка, смешение версии encoder | [BPE](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [prefix alias](23-TRANSFORMER-KNN-64TOKEN-PREFIX-ALIAS-2026-10-16.md) | Vocab SHA, vector query/index parity; syntax-specific queries |
| 06 Subunit | Оригинальный DPO распался на self-labelled incorrect correction, ложный WordMap/RuleTag, неверный UnitId | [Structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N29 annotation](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md), [dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) | Путь raw pair → каждый unit training call → learned fields |
| 07 Learned feature mismatch | Trained SemanticTokenizer строит KNN key отличный от training Executor new tokenizer | [N28](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md) | Same before/after correction+index key; source-disjoint retrieval |
| 08 Syntax-specific signal | Утеря E26 left/right POS, RU cases, order, negation, quote и C# AST semantics | [E26](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [E27](26-NEW-E27-STRUCTURAL-REORDER-COVERAGE-LOSS-2026-10-16.md), [N29 word alignment](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md) | Только релевантные домены; single-axis syntax order/AST contrast |
| 09 KNN candidate reach | FIFO top-k скрывает новые corrections, HNSW prune, identity collisions, empty top due source-law | [N16 FIFO](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [HNSW](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) | Native top-k, before/after late correction, newest-wrong control |
| 10 KNN score | Fixed/variable BM25 fusion, pool-growth rank inversion, доверие score без domain-law correctness | [N20](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md), [confidence](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | Same heldout, old/new candidate scores and answer |
| 11 KNN topic | Domain/topic passed but ignored in several keys and Executor | [N16](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md) | Bank finance vs shore; negative control |
| **12 N30-A NEW** | Пять из шести translation directions не обращаются к ordinary KNN без semantic concept, хотя память обучена | [N30 source trace](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) | EN_ES source-disjoint ordinary KNN gate A/B |
| **13 N30-B NEW** | Общая KNN для шести направлений без direction key; default target validation допускает испанский ответ на EN_HE | [N30 source trace](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) | EN_ES → EN_HE negative-transfer с direction-partitioned shadow |
| 14 Unit verifier | Накопление правил, негативно-ориентированная проверка, false positive по lexical marker | [N23](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md), [product semantic](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md) | Balanced semantic correct/wrong + abstention |
| 15 Checkpoint | Metadata-only export, неоткрытая learned state, cache alias, несовместимость tokenizer/index/optimizer | [restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [alias](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | Fresh process, same query, before/after state hashes |
| 16 Route adoption | Trained Unit обновлён, обычный ProductHost/Flow обходит его или использует другой Generator | [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | Source → trainer → Unit → user flow full trace |
| 17 History lineage | Старую неудачную матрицу ошибочно признали плохим KNN feature; E09/E34 валидированы несопоставимо | [transfer](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [historical validity](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) | Exact commit, formula, feature schema, same split, production caller |
| 18 Measurement | PASS/accuracy на обучении, Unit coverage, retrieval hit или контракт принимают за генеративный перенос | [historical](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [Unit metric](28-NEW-N28-UNIT-HELDOUT-OBJECTIVE-NOT-LIVE-QUALITY-2026-10-16.md) | Independent domain gold, matched source-family-disjoint, per-direction |

## Что реально было успешным, а что неверно приравнено к generalization

| Архив | Зафиксированный результат | Оговорка |
| --- | --- | --- |
| E26 contextual dictionary | disambiguation 0.40→1.00 на 15 cases | Узкая контекстная функция, сохранить POS/context как полезного донора |
| E43 replay | old retention 0.3226→0.3978 | Относится к bigram/replay, а не напрямую RP01 DPO |
| E14 context-action KNN Tier2 | 0.75→1.00 на четырёх validation | Tier3 PASSED=false; не переносить на широкое распределение |
| E09 structural KNN | .2413→.3507 reported | Baseline/treated оценены на разных validation subsets |
| E13B expression prefix | known recall, unknown abstain100% | Не доказан generalization to unseen expression |
| V191/V194/V210 DesignToCode | Реальные StepMany/веса и движение training accuracy | Не equal genuine heldout user answer |
| May repeat10 | minibatch train accuracy 1.0 при 500 rows | 50 исходных пар ×10, не 500 independent laws |

Прямые receipt links и оговорки: [historical donor registry](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [cross-branch real learning](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [misread matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md). За рамками нового source scan точная генеалогия того самого matrix experiment → KNN успешного helper остаётся OPEN.

## Самый короткий полезный порядок

**P0 Student:** RP01 REC02 tiny model-visible DPO gradient → warm-base vs DPO-only, fixed source-disjoint gold. Stop if unlearnable B0 rather than train another 500. Факты об уже выполненных C#500/ENRU500 сохраняются: q/v меняется, semantic free 0/3 и 0/4.

**P0 Translation Unit:** [MICRO-N30](30-MICRO-SIX-DIRECTION-KNN-LEARNING-QUALITY-EXPERIMENT-2026-10-16.md) в двух независимых L0→L1→L2 arms: ordinary KNN eligibility и cross-direction partition. Нативный in-process C# без Windows CI; затем только в случае эффекта end-to-end обычный consumer и cold restore. Не путать выяснение механики с доказанным улучшением.

**P1 исторический donor:** сравнить E26/E09/E14 и реальное применение внутри language_translate, DesignToCode, external_domains и current; подтверждение requires formula+branch commit+actual consumer+matched validation. Перенос признака из матрицы в KNN может быть верным даже после отрицательного матричного опыта, но нельзя догадку выдавать за найденный точный source commit.

**P1 следующие Units:** только после измеренного bottleneck отдельными arm проверить language/TopicDomain namespace, tokenizer/index state, syntax/AST/word order, replay и verifier. Не насаждать русский синтаксис на испанский или C# AST на обычную речь.

## Evidence и методика review

- **Source-confirmed:** N30-A и N30-B — пути, ключи, отсутствие direction, target predicates. Полное качество на живой модели пока неизвестно.
- **Archived observed:** frozen RP01 outcomes, historical runs. В этой сессии старые прогоны не переисполнялись и их метрики не переименованы.
- **Hypothesis:** каждая причинная стрелка «пробел → ухудшение качества» до matched native A/B остаётся недоказанной.
- **Skills:** сверены intent-to-plan, architect-behavior-classifier, code-examination, plan-review, plan-self-validation, experiment-design, human-readable-architecture-audit, claim-verification, prompt-patch-authoring и translation-domain-reality-check из core/docs/skills. Мера качества — читаемое объяснение, точный вызывающий метод, доказательства, falsifier и контроль публикационных claims, а не число документов.
- **Правила Git:** в core/docs/plans/replanning-possible-diffs ничего не изменено; эти документы добавлены только в xiigen-planning/llm_plan3/replanning/possibleDifs. Вновь изученные source RAG фрагменты не выдаются за полный RAG audit.
