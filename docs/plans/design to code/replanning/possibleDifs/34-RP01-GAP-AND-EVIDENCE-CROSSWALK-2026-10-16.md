
# N34 — единая причинная карта RP01 + исторических положительных механизмов + новых предобучающих пробелов

**Дата:** 2026-10-16 | **Метод:** сравнение первичных исходников, published historical JSON и existing evidence owners; source-only micro-proxy | **НЕ CLAIM:** полнота всех 259 документов, native обучение, независимая CODEX/Astra проверка, root cause RP01 0/3.

## Почему первые улучшения потерялись

[RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) публикует C#500 (16 adapter tensors modified; free 0/3→0/3), ENRU500 (free 0/4→0/4). Это **показатель именно Student**. Нельзя сравнивать его как будто это тот же consumer, что [E09](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) (C# KNN top1 0.2413→0.3507, 4492 training pairs, 5617 tokens, при этом latency 14.5→763.5ms) или [E26](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) (0.4→1.0 на 15 словах, 6 hard), [E43 replay](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) (retention 0.3226→0.3978). Эти эксперименты положительны **для своей панели/получателя**, не доказательство универсального обучения.

E07 structural matrix (negative) и E09 Structural KNN (positive) — не одна и та же intervention/data split. Их нельзя объявлять matched proof или автоматически переносить AST на перевод. [Историческое reconciled объяснение](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md).

## Матрица различий: не изобретать проверенные другими агентами находки заново

| Класс | Источник / существующий владелец доказательства | Domain / какой learner | Самый дешёвый decisive probe |
|---|---|---|---|
| Teacher-authorship, quality of chosen/rejected, hard-negative family, absence of law contrast | [02-DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [N32 same prompt](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md) | Student DPO: C#, language, PC | hash and law-correct independent judge + tiny train/heldout |
| Random B0, supervised prerequisite, no first-gradient, embedding disconnected | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [N33](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md) | Neural base; not KNN | 8–32 SFT controlled overfit, finite difference, free response |
| Exact DPO response masks/reference/objective/StepVector/LoRA q/v | [N33 overview](33-MASTER-RP01-QUALITY-DIVERGENCE-AND-NOVELTY-2026-10-16.md), [03 DPO](03-DPO-PAIR-AND-GRADIENT.md) | DPO Student | gradient sign and same-example margin, no fake semantics |
| BPE, EOS, truncation, common prefix, cached/full decoder mismatch | [BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [answer boundary](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Student/translation, actual consumer-dependent | exact input IDs + cold restore and free-answer |
| DPO→typed learned sub-units, silent dispatch, identity / polarity | [structured units](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N23 identity](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) | Unit/Flow/Graph | learning receipt → mutated Unit → ordinary output |
| AST tree, syntax order, negation, roles, POS, domain routing | [E07/E09](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [E26](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [N27 role](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md) | C# AST vs ENRU roles ≠ universal feature | source-disjoint minimal pairs with conditional KNN feature gate |
| KNN HNSW topology/add/prune/index-state, retrieval numerator | [N28 HNSW](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md), [N33 RNG](33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md) | vector consumers only | oracle brute-force / ANN topK and correct lesson ID |
| KNN fusion rerank, lesson crowding, candidate normalization | [N20](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md), [N31](31-NEW-REPLAY-COUNT-CLIFF-AND-PSEUDOSTRUCTURE-2026-10-16.md) | KNN/hybrid recall, not neural DPO | add irrelevant memory, same query → ordinary correctness |
| Checkpoint, LoRA/optimizer/RNG, state adoption and stale consumer | [restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [N27 mutable](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | all learner types, but distinct artifacts | pre/post fresh-process exact decision parity |
| Replay + learned selector versus static deterministic fallback | [replay](07-CONTINUAL-REPLAY.md), [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | ProductHost/Units/KNN PC | one new lesson → final answer and retention |
| Historical scoring leakage, shuffled split vs law correctness | [E35/E36](23-NEW-E35-E36-COUPLED-RNG-AND-FALSE-KNN-PASS-2026-10-16.md), [N28](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md) | All | blind family split, good+bad paired, negative controls |
| **NEW N34-A** mismatched EOF/EOS in dataset builder | [N34 detailed primary source](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) | only baseline corpus pretraining | same-file code/ENRU doc-boundary AB |
| **NEW N34-B** stride=1 overlapping batch windows, limited effective reach | [N34](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) | only DataLoader pretraining; conditional quality | stride 1 vs seqLen vs half, equal updates and exposures |
| **NEW N34-C** silent uint16 saturation of token IDs >65535 | [N34](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) | vocab >65536, NOT RP01 629 | range trap/readback; optional large-vocab heldout |
| **NEW N34-D** shard transitions, file order, missing manifest | [N34](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) | multi-file pretraining; not immediate DPO | pinned file SHA/order, input/target window ids |

## Приоритет без новых огромных циклов

**RP01 P0 first:** (1) доказать, что у Student есть базовая компетенция и достижимый gradient, (2) 4–8 разрешённых семантически сильных DPO пар, (3) tiny train-overfit vs independent heldout free answers и clean restore, (4) проверка, вызывает ли продукт **именно** научившегося recipient. Если нет — не расходовать следующие 500 / Windows gate.

**P1 parallel domain question (not proof):** re-run 2×2 Student-only vs Unit+learned KNN, conditional E09-AST for C#, E26-POS for RU/EN, source-family-disjoint, positive+negative cases, fallback trace. **Не пытаться лечить C# LoRA Student одним изменением E09 kNN.**

**N34 prerequisite-only:** если действительно строим warmed B0 собственным pretraining, проверять его exact token pipeline на очень малом корпусе, gating on N33 gradient/warmup. Отрицательный контроль: RP01 StepVector и frozen HNSW memory не должны меняться от этого toggled dataset loader.

## Роли, документы, границы утверждений

Существующие успешные и неуспешные результаты immutable. Версия core pinned. Работа выполняется в planning `possibleDifs`; protected core `docs/plans/replanning-possible-diffs` не изменяется. Никаких prompt/model/judge calls, native quality training, DPO authoring, .NET testing или Windows CI в N34 не было. Existing CODEX/Astra case-selection/independent evaluation and permitted Chinese open teacher-authoring remain future prerequisites, not simulated achievements.

**Артефакты:** [N34 source-audit](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) · [MICRO-N34](34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md) · [proxy mjs](34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs) · [receipt](34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json).
