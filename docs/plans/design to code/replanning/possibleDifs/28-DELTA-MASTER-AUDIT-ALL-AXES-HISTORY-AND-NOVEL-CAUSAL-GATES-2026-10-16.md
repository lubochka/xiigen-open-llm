# RP01 / N28 — карта ВСЕХ направлений риска и новые причинные проверки по первичным источникам

**16 октября 2026.** Публикация — новое **delta-аудитное дополнение**, НЕ переписывающее другие `possibleDifs`. Core: `codex/dpo-implementation@d5c2409b24292d361593b8262b0c54997b15f539`; sibling `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`; planning `llm_plan3`. Удалённые документы `llm_mvp_core/docs/plans/replanning-possible-diffs` — **READ-ONLY; не изменялись**.

**Граница проверки:** первичные C# файлы DpoTrainer, LoRA, Tokenizer, HNSW, generic/transformer kNN, TranslationStructuredBatchBuilder, GraphRAG, RP01 loader; первичные сохранённые JSON E09/E14/E26/E43; RAG WorkLeft Master; E14/E26/E43 отчёты **не переисполнялись**. Большой существующий каталог был просмотрен выборочно (индекс/основные synthesis и смежные hypotheses), не утверждается полная ручная построчная проверка каждого файла или каждого экспериментального запуска. Native .NET, реальный PC Student exam, external Chinese teacher и независимый CODEX/Astra 6 UltraCode не запускались.

## 1. Проблема не в одном параметре — это разные обучающиеся машины

Сравниваются четыре разных адресата:

- **S (Standalone Student)**: `DpoExactPairMaterial → DpoTrainer → LoRA q/v → decoder`. Результат C#500: 50 optimizer updates, 16 изменившихся LoRA tensors, 39 frozen base tensors, *free-response 0/3→0/3*; EN→RU500: следующие 50 updates, *free-response 0/4→0/4*. [RP01 первичный recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
- **U (Learned Unit / memory)**: `KNN`, словарь, correction-map, typed sub-Unit, собственный training+inference; `correctOutput` может быть сохранён без обновления нейронных весов.
- **G (Learned graph/flow)**: обучение связей typed input/output, маршрута, higher-order association, replay, conflict policy, возможно гибридное использование детерминированного закона.
- **C (Ordinary consumer)**: пользователь реально вызывает отдельный путь. `S` free-generation не обязана вызывать `U`; `U.Train` не доказывает, что `C` его выбрал, а след `G` не доказывает качество ответа.

Правильная цель сравнения: `source & license → learner selection → authored chosen/rejected → CODEX grade → exact pair → unit decomposition & feature version → gradient OR memory update → checkpoint & restore → active route → ordinary consumer → blind domain quality`.

## 2. Сквозная матрица возможных причин (E = источник, H = причинная гипотеза; P = порядок проверки)

| # / подсистема | Что конкретно может быть искажено, потеряно или не вызвано | Где исходные подробности | Область / быстрый gate |
|---|---|---|---|
| 01 **S P0 базовая компетенция** | B0 случайная и маленькая; DPO предпочтение не создаёт из ничего широкие языковые/кодовые навыки | [Base/LoRA](04-BASE-LORA-CHECKPOINT.md), [cold-start experiment](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | Все generative domains; 8–32 source-disjoint cases SFT→DPO vs DPO-only |
| 02 **DPO authoring P0** | chosen/rejected формально разные, но оба плохи, равны по задаче, различаются length/style вместо закона | [DPO data](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [pair contrast](18-E16-PAIR-CONTRAST-SURVIVAL-MICROEXPERIMENT.md), [N27 prefix](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md) | Доменный judge по финальным pair bytes |
| 03 **DPO task diversity P0** | 500 записей = 25 templates×20, не 500 независимых laws; lexical train exposure создаёт ложную полноту | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [historical splits](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md) | group/source-disjoint split, heldout transfer |
| 04 **DPO tokenizer target** | exact-v2, legacy masks, prompt-response boundary, BOS/EOS, token ID совпадения и реальный градиент могут расходиться | [DPO gradient](03-DPO-PAIR-AND-GRADIENT.md), [target stop](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md) | same actual ids + finite difference |
| 05 **DPO vector→gradient** | TeachingVector — не полноценное векторное обучение, в StepVector часть семантики сведена к scalar | [LoRA/tokenization](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [AdamW scalar](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) | one scalar intervention / small batched contrast |
| 06 **LoRA rank/topology** | q/v-only rank8 alpha16 и target coverage могут быть недостаточны; это capacity hypothesis, не доказанный баг реализации | [LoRA](04-BASE-LORA-CHECKPOINT.md), [01 matrix](01-PRIORITIZED-DIFFERENCES.md) | same budget rank/target isolate; zero-dropout native |
| 07 **Optimizer/RNG/masks** | optimizer scale cancellation, stochastic dropout (в RP01 dropout=0), restore moments, possible mismatch reference | [AdamW audit](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md), [DPO code](03-DPO-PAIR-AND-GRADIENT.md) | finite difference, optimizer-state readback |
| 08 **BPE/vocab identity** | BPE re-train changes vocabulary/IDs and former hashed KNN neighborhoods; EOS id not special | [BPE train](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [stop target](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md) | tokenizer SHA & decode/mask parity |
| 09 **Embeddings+shape** | dim mismatch, 64-token transformer prefix truncation, wrong vector normalization | [shape](23-MICRO-SHAPE-AND-SAMPLER-CAUSAL-PILOT.md), [prefix](23-TRANSFORMER-KNN-64TOKEN-PREFIX-ALIAS-2026-10-16.md) | exact vector/query shape and discriminating prefix |
| 10 **KNN candidate visibility — NEW H1** | reverse-edge prune делает вставленный correct example потенциально недостижимым; reranker его не получит | [**новое N28-H1**](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) | [native micro](28-MICRO-HNSW-LESSON-REACHABILITY-EXPERIMENT-2026-10-16.md); HNSW vs brute on frozen vectors |
| 11 **KNN lookup & rank** | first-hit, `k`, confidence floor, conflicting training labels, tie semantics | [kNN gap](04-KNN-HYBRID-RETRIEVAL.md), [identity scope](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) | domain-law-aware rerank; false confident errors |
| 12 **Hybrid fusion/memory growth** | fixed fusion coefficients вместо contextual weights; pool-normalized BM25 переворачивает прежний answer после чужого lesson | [N20 lexical](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md) | frozen candidate A/B/C; score→answer |
| 13 **AST/order/POS** | bag/vector без синтаксического дерева, позиции, `NOT`, rule tags, clause boundaries, actor/target ordering | [language routing](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), [E26 learned POS](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) | syntax-specific gate per domain; не применять C# AST к обычной речи |
| 14 **Матрица→KNN historical transfer** | формула, отклонённая в неудачном matrix experiment, могла стать полезным scoring feature в kNN; не потерять обучаемые коэффициенты | [transfer lineage](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [experiment misreads](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | exact algorithm/commit/consumer lineage; same split |
| 15 **Semantic text→sub-Units** | one DPO pair преобразуется в набор tokenizer/executor/verifier lessons, semantic chosen/rejected могут схлопнуться | [translation DPO bridge](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md), [structured batch](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) | source pair, every unit input/output, digest before/after |
| 16 **Unit dispatch/atomicity** | UnitId mismatch, partial batch train, same source case admitted twice different casing | [silent dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), [unit identity](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) | assert actually-called TrainOnExample by role |
| 17 **Verifier/gates** | negative-only verification, false PASS on trivial invalid outputs; different languages scored through same function | [verifier bias](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md), [verifier conflict](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) | balanced positive/negative law source-disjoint |
| 18 **Domain key/scope** | direction/SourceLang, role gender, tense, negation, quotes and operator identity incorrectly normalized | [language key](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md), [role quotes](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md) | RU/EN/HE separate tied negative cases |
| 19 **Graph/flow associations** | typed many→many output/input provenance, async race, premature graph closure, learned decision supplanted by deterministic route | [graph](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [closure](27-NEW-E50-PREMATURE-GRAPH-CLOSURE-FALSE-PASS-2026-10-16.md) | trace producer→resolver→consumer ids/versions |
| 20 **Checkpoint restore/adoption** | weights restored but unit dictionary/KNN index missing; old mutable cache alias, wrong adopted artifact, silent fallback | [restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [mutable cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | same heldout before/after *cold process* |
| 21 **Replay/retention** | old learned law displaced by selective replay top3, graph version, incompatible checkpoint | [replay](07-CONTINUAL-REPLAY.md), [aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) | measure old+new task on same consumer |
| 22 **RAG context loss — NEW H2** | ranker найденное доказательство отражает в Results, но `BuildContext` early break скрывает последующие короткие chunks | [**новое N28-H2**](28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md) | 3-snippet native micro, then only if real RP01 consumer |
| 23 **Evaluation and sampling** | changed LoRA ≠ changed generated first token; EOS/stop/KV/sampler calibration, meta gate false green | [stop/sampler](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [historical validity](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) | C0→T logits + blind ordinary output |
| 24 **Consumer bypass** | neural RP01 не вызывает learned correction/graph/kNN, хотя старый workflow их использовал | [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md) | **первый** consumer call-chain, затем Student-only/Unit-only/Hybrid |
| 25 **Teacher/judge/license authority** | внешний creator / reviewer неправильно атрибутирован, замена genuine contrast graded synthetic by textual heuristic | [DPO conformance](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md), [dpo guidance](../../Алгоритм%20создания%20ДПО.md) | source provenance & exact role+grade, not merely provider name |

Часть строк — **возможные** причины, а не подтверждённая конкретная регрессия. Приоритет P0/P1 не означает измеренную тяжесть; назначение любого изменения допускается лишь после положительного дешёвого causal gate.

## 3. Ранние УСПЕШНЫЕ результаты — сохранить и правильно атрибутировать

Первичные неизменённые JSON прочитаны прямо из `35.ExperimentsV2/results`:

| Опыт | Фактический записанный эффект | Почему нельзя превратить в proof нынешнего Student |
|---|---|---|
| [E14 CONTEXT-ACTION-KNN Tier2, 2026-04-30](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-001.json) | top1 `0.75→1.00`, `trainFiles=16`, `valFiles=4`, `k=3`, `passed=true` | Лишь 4 validation files; классы неравномерны. Успех targeted routing, не free response |
| [E26 CONTEXTUAL-DICTIONARY Tier1, 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) | disambiguation `0.40→1.00`, `testCases=15`, `hard=6`, `passed=true` | Позитивный контекстный выбор на 15 случаях, не universal translator; нужен POS-context consumer |
| [E43 REPLAY-BUFFER Tier2, 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | retention `0.3226→0.3978`, old 465/new 414/replay 177 | Старое bigram/replay, а не 500-pair current LoRA |
| [E09 STRUCTURAL-KNN Tier2, 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | reported top1 `0.2413→0.3507`, trainPairs4492 | Baseline и enhanced используют неидентичные evaluation subsets; causal delta нуждается в matched rerun |

**Historical hypothesis donor ≠ licensed DPO teacher ≠ real learned consumer.** Не переписывать старые FALSE/FAILED в PASS: интересную функцию извлекать из кода и проверять в новом KNN на тех же законах. У E13B корректно интерпретировать `unknown→abstain` как safety, а не успех генерации неизвестной формулы. См. [full lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [E13 held-out](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md).

## 4. Доменная область: не переносить функцию механически

| Условный feature | C#/SQL/code | RU↔EN / HE↔EN translation | Design→HTML/CSS | Orchestrator/flow |
|---|---|---|---|---|
| Roslyn AST/structural role ordering | **важно**, если C# source известен | **не применять Roslyn AST**; POS/dependency/order alternatives | UI hierarchy, DOM/CSS tree | graph transition identity |
| Rule-tag/POS/gender/predicate context | code operators, negation, method roles | **критично условно**: direction, actor, gender, tense | style intent, component relation | order/temporal guard |
| Dynamic lexical/BM25+kNN | retrieval docs and examples | useful lexical/phrase memory but may harm syntax | page snippets/design labels | prior domain skill routing |
| HNSW reverse-prune N28 | только если learned HNSW called | только learned KNN; **не** exact-source map | только proven model consumer | only learned graph/index |
| RAG context N28 | only GraphRAG consumer | only retrieval context explicitly used | only design spec RAG chain | only prompt-grounded router |
| SFT/LoRA/attention | generative response | generative language output | generated markup | learned prompt/decision only if model serves |

### Ближайшая строгая последовательность, без новых 500 пар

**0.** Зафиксировать actual `ordinary consumer` для каждой задачи. Если S без U, запрещено говорить что U вызвал free-generation 0/4.  
**1.** Проверить актуальные exact chosen/rejected bytes/role/license через CODEX Astra с семантическим критерием; выделить 4 мини пары.  
**2.** Native one-pair finite-difference + tiny SFT warm start vs DPO-only на том же минимальном бюджете, затем *same blind generation output*.  
**3.** Параллельно, как отдельный learner: 11-point native HNSW counterexample/12+4+16 Unit pilot ([micro N28-H1](28-MICRO-HNSW-LESSON-REACHABILITY-EXPERIMENT-2026-10-16.md)); лишь после актуального call-chain.  
**4.** Отдельно проверить 3-snippet RAG budget, **только** если конкретный consumer читает `ContextText`.  
**5.** Для лучших прежних E14/E26/E43 функций — перенести **одну** feature в допустимый Unit, сравнить unit-only vs student-only vs hybrid, не смешивая домены или источники.  
**6.** После первого реального улучшения сравнить checkpoint before/after *нового процесса*, 8 старых + 8 новых cases, no-repeat и rollback, без promotion по одному `passed=true`.

## 5. Роли, обучение, независимая оценка

**Human-role selector = CODEX на GPT-6 Astra 6 UltraCode**: выбирает конкретные полезные cases, а не подменяется пользователем или китайским teacher. **DPO author = разрешённая внешняя открытая китайская модель**: генерирует **ОБЕ стороны** final `chosen` и `rejected`; модель Student/base может давать входной weak контекст, но не объявляется автором внешней пары. **Independent pair-and-Student quality judge = CODEX на GPT-6 Astra 6 UltraCode**: проверяет точную видимую пару, соответствие предметному закону и изменение обычного ответа. *Наш документарный аудит не совершал этих вызовов*. Сохранять `licensing, provider, model version, exact material digest, refusal/tie`. Grader output не является коэффициентом DPO log-ratio.

## 6. Reviewer verdict и limits

- **SOURCE_CONFIRMED_NEW_CONTROL_FLOW:** N28-H1 HNSW reverse pruning может удалять inbound; N28-H2 GraphRAG `break` скрывает подходящие последующие chunks.
- **EXECUTED_SHADOW_ONLY:** level0 m2 Python proxy дал конкретный контрпример; **НЕ** native C#; m8 production quality неизвестно.
- **HISTORY_POSITIVE_VERIFIED_FROM_JSON:** E14/E26/E43 были положительными на указанных собственных метриках с узкими знаменателями.
- **NO CLAIM:** 100% exhaustive repo scan, complete branch archaeology, independently executed CODEX/Astra review, actual external Chinese DPO call, native ML run, learned quality gain of RP01, success in all domains.
- **Open root cause RP01:** текущая сильнейшая гипотеза — mismatch нейронного Student и прежних обучаемых функций плюс недостаточная базовая языковая компетенция; требует matched causal experiments, не исправляется автоматически новым HNSW/RAG кодом.

Навигация исходного каталога: [README](README.md), [00 MASTER](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [historical lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [27 full crosswalk](27-RP01-FULL-CAUSAL-CROSSWALK-POSITIVE-HISTORY-2026-10-16.md). Рабочая RAG иерархия: [WorkLeft RAG MASTER](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [usage guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md) — RAG entries являются **указателями**, не proof обучения.

**Skill-stack применён по доступным релевантным источникам:** `architectural-decision-testing`, `phase-planning`, `plan-review`, `code-examination`, `prompt-patch-authoring`, `output-readability-gate`, `experiment-design`, `translation-domain-reality-check`, `learning-path-audit`, `dpo-triple-format` из `core/docs/skills`; ограниченный evidence-bound audit, **не независимая полная 30-pass certification**.
