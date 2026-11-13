# N37 — RP01 против успешных исторических механизмов: причинная crosswalk-карта, новый E22 и быстрый порядок проверки

**16 октября 2026 · EVIDENCE ROUTER / SOURCE-BOUND DELTA / DRAFT, НЕ окончательное установление root cause.**  
Core read-only snapshot: `codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1bff7e6672`; sibling `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`, `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639`. Planning branch `llm_plan3`. **Файлы core `docs/plans/replanning-possible-diffs/**` остаются только источником, ни одной правки там.** Этот документ — новая **маршрутизация причин и экспериментальных решений**, не присвоение уже опубликованных другими агентами выводов.

## 1. Центральное различие: разный получатель обучения ≠ плохой единый алгоритм

```text
Реальный запрос и закон предметной области
     │
     ├─ CODEX (Astra 6 UltraCode) выбирает задачу (human-role)
     │           ↓
     │  внешняя открытая китайская модель создаёт chosen И rejected
     │           ↓
     │  CODEX (Astra 6 UltraCode) независимо судит точную пару
     │           ↓
     ├─ DPO → token IDs → signed objective → AdamW → q/v LoRA → Student → TextGenerator
     │                                                    │
     ├─ отдельная проекция контраста в typed Tokenizer/Executor/Verifier Units
     │     → phrase/word/POS/order/AST memory → KNN/rule routing → ordinary Flow
     │
     └─ versioned restore + accepted-parent + source-disjoint blind exam
                                            ↓
                              CODEX оценивает реальный ответ
```

Эта схема — **анализ возможных recipient routes**, а не доказательство, что все стрелки в RP01 реализованы и соединены.

### Что измерено RP01

По [RP01 learning recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) **C#500**: 500 пар (25 параметризованных families×20), 50 updates, 16 LoRA tensors изменились, base39 остались, free generation **0/3→0/3**. **EN→RU500**: ещё 500 пар (20 категорий×25), ещё 50 updates, свободные ответы **0/4→0/4**. Изменились preference logit/margin, но не продемонстрирована domain correctness. B0 — **random** vocab629, hidden512, 4 слоя, не языковая pretrained base; LoRA-only DPO без предварительной языковой компетенции может не дать осмысленного текста. Это сильная **причинная гипотеза**, не доказательство, что вся DPO математика или LoRA сломана.

### Что реально работало раньше — ограниченные первичные свидетельства

| Historical donor / branch | Сохранённый результат | Граница доказательства |
|---|---|---|
| [E09 Tier1 — DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md) | **0.3368→0.3472**, relative +3.1%, *FAILED* из-за 5%-порога | Небольшой положительный сигнал не равен «нулевой функции», но не достиг порога и не доказывает перенос в product |
| [E09 Tier2](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | **0.2413→0.3507**, *PASSED* | В [E09 code](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L78) baseline считает полную validation, KNN только `Take(500)`; не matched causal panel; per-file kindMap может дрейфовать |
| [E14 Context-Action KNN — small](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-001.json) | **0.75→1.00** на 4 validation files | Узкий позитив; у другого large [01.05-002](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) **0.8824→0.8676**; source/domain/balance существенно влияют |
| [E22 latest 07.05](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json) | **macro-F1 0.4709→0.5646**, train360/val90, PASSED | Положительный KNN router для *автоматической разметки*; **старые FAIL summary неполны**. Следующее дополнительное N37 показывает, почему это не гарантия обучения *по одной* паре |
| [E26 Contextual Dictionary](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) | disambiguation **0.40→1.00** на 15 preset cases (6 hard) | POS/контекст может быть важным для EN/RU; это не semantic independent cold translation student |
| [E43 replay](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | retention **0.3226→0.3978** в старом bigram/replay | Это другой learner; не разрешает silent replay одной already applied DPO pair |
| [DesignToCode training / Translation recipient archaeology](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) | В соседней ветке встречаются настоящие optimizer steps и checkpoint; train accuracy может улучшаться | **Train metrics ≠ frozen ordinary free answer**; факт результата нужно проверять по точной commit/data/model версии |

О **матричном эксперименте, перемещённом в KNN**: [12 source transfer](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) перечисляет кандидатов (E07 structural context, E09, E14, E12 numerical matrix), но chain `old failing feature → exact migration commit → new KNN consumer → same-panel causal gain` **по-прежнему OPEN**. Нельзя объявлять E22 этим переносом: E22 применял KNN до и после последнего refactor. Для подтверждения искать конкретный донорский diff, прежние и новые формулы *одного* признака, его получателя и результат на неизменном gold.

## 2. Сводная карта возможных пробелов с prior-evidence и быстрыми фальсификаторами

Обозначения: **C** — условие в исходнике подтверждено; **R** — старый run receipt; **H** — причинный эффект пока гипотеза; **U** — actor/consumer не установлен. Все пункты ниже (кроме N37) ранее исследованы: здесь они сгруппированы, а не объявлены новыми открытиями.

| Код / причина | Статус → recipient | Предшествующий документ и минимальный A/B |
|---|---|---|
| **N01. Raw pair provenance:** simulation/template вместо авторства обеих сторон внешним teacher; нет CODEX exact-pair judge | R/H · все DPO | [02](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [32 same prompt](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md): проверить exact bytes, licence/provider/provenance/valid pair; не переписывать исторические имитации |
| **N02. Три задачи не одно:** одно семейство параметризовано сотнями строк; train↔heldout leakage; weak/easy rejected | R/H · C#/RU/Builder | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [32](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md): зафиксировать семьи ДО генерации, matched-length hard negatives |
| **N03. DPO masks/logratios:** prompt в loss, response offsets, sum/mean, length, signs, beta, reference freeze | C/H · neural | [03](03-DPO-PAIR-AND-GRADIENT.md), [core read-only 41](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md): finite difference, common prefix and length-matched contrast |
| **N04. Cold-start without basic language:** random B0; DPO-only preference оптимизирует не fluency | R/H · neural P0 | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): matched tiny chosen-only CE warmup+same DPO vs raw B0+DPO |
| **N05. LoRA q/v capacity and actual recipient:** rank8/alpha16, frozen base/head | C/H · neural | [LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [core read-only 47](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/47-current-attention-backprop-and-position-contract.md): same initialized state, controlled module-target ablation; gradient→free answer |
| **N06. AdamW/grad finite & scale:** updates exist but too small, nonfinite/clip or dimension mismatch | C/H · neural/vector | [N26](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md), [N33](33-NEW-NONFINITE-GRAD-CLIP-CASCADE-AND-SFT-STATE-2026-10-16.md): finite check, parameter grad norms, one meaningful output change |
| **N07. SFT/pretrain prerequisites:** embedding grad disconnected, zero-LR warmup, data boundaries and curriculum | C/H · warm-start | [N33](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), [N34](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md): first update and next-token overfit before scaling |
| **N08. Tokenization/encoder drift:** BPE vocab629, retrained unit BPE320, Unicode, EOS/stop, length, bridge token erasure | C/H · neural/vector/translation | [N21](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [16](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [N33 UTF8](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md): check actual model-visible IDs, token offsets, eos, decoder, no truncation |
| **N09. Vector encoding blind to ordering/role:** hashed bag/count tiers merge negation/AST/POS order | C/H · KNN | [05](05-SEMANTIC-VECTOR-KNN.md), [N27](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md): same lemma, inverted actor, reversed edge, test domain-specific kernel |
| **N10. Structural KNN historical validation:** E07/E09 fields & cross-file SyntaxKind mapping, unmatched validation | C/R · C# AST | [12](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [N35](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md): identical source families and gold, distance one-factor |
| **N11. Contextual POS function not transferred:** E26 left/right tags vs weak current word map | R/C/H · translation | [N23 E26](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md): compare same ambiguous source with minimal POS alteration |
| **N12. KNN identity, duplicates, sorting and scope:** source signatures, stale index, k window, candidate contamination | C/H · generic and Translation | [N14](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [N34 alias](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md), [N30 HNSW](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md): inject conflicting examples, check original IDs and top-k |
| **N13. NEW E22 single-lesson vote cliff:** n0→n1 switch alltrue→allfalse, fixed configured-k denominator, no positive one-shot learning | **C/H NEW** · only E22 router | [N37 finding](37-NEW-E22-ONE-PAIR-KNN-VOTE-CLIFF-AND-ONLINE-REPLACE-2026-10-16.md): 8 synthetic source-proxy cases + [tiny matched pilot](37-MICRO-E22-ONE-LESSON-KNN-CAUSAL-PILOT-2026-10-16.md) |
| **N14. NEW E22 Train clears state:** online delta lesson could erase earlier remembered labels; rebuild Train is fine | **C/U/H NEW** · conditional E22 | [N37 finding](37-NEW-E22-ONE-PAIR-KNN-VOTE-CLIFF-AND-ONLINE-REPLACE-2026-10-16.md): full snapshot vs `Train([delta])` on same old-law query |
| **N15. NEW E22 empty query:** corpus order creates classification without query predicates | **C/U/H NEW** · conditional E22 | [N37 finding](37-NEW-E22-ONE-PAIR-KNN-VOTE-CLIFF-AND-ONLINE-REPLACE-2026-10-16.md): same memory permuted, no-evidence abstain |
| **N16. KNN/ANN metric and version:** encoder/index versions, namespace/signature alias, nonfinite vectors, HNSW unreachable lesson | C/H · vector | [N16](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md), [N28 HNSW](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md): recall exact lesson before/after index rebuild and reopen |
| **N17. DPO→sub-Unit decomposition:** chosen/rejected wrong branch, missing negation/roles/constraints, direction or language key | C/H · Translation Units/Builder | [10](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md), [N16](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md): raw exact pair→learned sub-unit before/after, verify no wrong polarity |
| **N18. Unit parser/value/state identity:** case-insensitive collisions, punctuation ordinal drift, quoted source, operator erased, non-atomic batch | C/H · Unit | [N23 case](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md), [N33 phrase](33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md), [N30 operator](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md): one opposite-law pair A/B |
| **N19. Deterministic authority masks learning:** fallback/verifier max to .99, confidence not calibrated, hard-coded path wins | C/H · Translation/Fusion | [N35](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md), [N36](36-NEW-E46-SYNTHETIC-FAIL-MISREAD-AND-POST-VERIFIER-NOVELTY-SIGNAL-ERASURE-2026-10-16.md): compare raw retrieval-vs-postverifier score, wrong accepted/abstain |
| **N20. GraphRAG non-evidence, context budget:** seedless PageRank yields snippets even if query has no seed | C/H · GraphRAG | [N36](36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md), [N28](28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md): pre-PPR valid-seed guard; same budget/query |
| **N21. Ordinary consumer bypass:** stored neural/Unit learning never read by ProductHost generated answer, fallback overrides correct Unit | C/U/H · product | [N18](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [N35 map](35-RP01-ALL-RECIPIENTS-CAUSAL-PRIORITY-MAP-2026-10-16.md): exact user endpoint trace before/after saved restore |
| **N22. Checkpoint/replay identity:** optimizer/RNG/tokenizer/index/graph not atomic or parent not accepted, applied-once missed, old law forgotten | C/H · all learner state | [06](06-CHECKPOINT-RESTORE-ADOPTION.md), [N27 replay](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md), [N34 E43](34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md): same input, same parent+child restore, no double-pair |
| **N23. Fake evaluations:** small heldout / rank-only, marker-only law judge, source-order leakage, rare bucket val0, metric denominator mismatch | C/R/H · all evaluations | [N28 scorer](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md), [N35 rare](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md), [core E22 65](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md): blind source-family split and independent semantic laws |
| **N24. Loss-only stop and nonlocal retrieval interference:** loss plateau hides no free gain, fusion corpus growth changes ranking for unrelated input | C/H · student/fusion | [N35 stop](35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md), [N20 fusion](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md): same frozen request before/after one appended lesson |
| **N25. Feedback/route and reward contract:** delayed/wrong label, inverted DPO direction, verifier signs, separated Train/Predict source families | C/H · feedback | [N24 E36](24-NEW-E36-FEEDBACK-KNN-SPLIT-AND-ROUTING-2026-10-16.md), [N26 roles](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md): correct reason→exact accepted pair→consumer quality |

## 3. Быстрая причинная очередь, ориентированная на качество, а не Windows CI

1. **P0 / neural prerequisite:** exact original RP01 (0/3,0/4) сохранять неизменным; новая source-family-disjoint панель. B0→tiny chosen-only 8–32 overfit→new free answer vs same B0→DPO. Если first-token logits движутся, но смысл не улучшается, не делать ещё 500 DPO.
2. **P0 / actual DPO receipt:** один разрешённый **новый** exact-pair, проверка одних и тех же response IDs, gradient finite-difference, actual step count/reference snapshot, CODEX/Astra 6 UltraCode judge. Пара без допустимого авторства INVALID_EVIDENCE.
3. **P1 / known-positive mechanisms on correct recipients:** 12–24 C# AST/source family queries для E09/E14 one-factor feature; 12–24 ENRU ambiguity/POS/negation для E26 Unit; 12–24 Orchestration unit paths. Сравнить corrected lesson source→memory→ordinary consumer (не Student logits).
4. **P1 / new N37 E22:** [восемь source-proxy контрастов](37-MICRO-E22-ONE-LESSON-SOURCE-RECEIPT.json), потом tiny native actual `PredicateDatasetRouter` и независимая многометочная panel **только если такой Unit потребляется продуктом**. Гипотеза не тянет за собой правку `KnnLogicalUnit`.
5. **P1 / state+restore:** в одном живом домене trace `old accepted state + one new pair + one applied update + fresh process cold reopen + old-law retention + independent gold`; `IMPROVE` / `REGRESS` / `INCONCLUSIVE` / `INVALID_EVIDENCE`. Без parent accepted checkpoint → `NO_ACCEPTED_BASE`, не имитировать rollback.
6. **P2 / ranking, cross-domain and graph:** по источнику только после consumer trace проверить seedless retrieval, stale ANN, negative word map, fallback priority, source order; провести frozen-query one-factor A/B и учитывать coverage vs false accept.

Для любого пилота минимальная запись: `domain, source_family, source_sha, core_sha, model/adapter/tokenizer/index fingerprints, prompt_context_id, pair_id + full authorized provenance (if DPO), recipient_unit_id, one-factor change, before/after raw final answer, semantic law, blind judge, old-law retention, latency, evidence status, rollback decision`. Отсутствующий field = **UNKNOWN**, не заполнять догадкой.

## 4. Доменно-условные функции — не одна универсальная KNN-метрика

| Домен | Проверяемый полезный признак / функция | Вредный перенос / контроль |
|---|---|---|
| **EN↔RU** | POS left/right, morphological agreement, role/order, negation, protected entity/number, language direction, source context | Bag-of-words/casefold может склеить обратный смысл; exact `E22 C# label` не годится |
| **Оркестрация** | predicate set + typed preconditions/effects + edge order + valid goal transitions; multi-label routing | nearest lexical match без графовой причинности / неверный последовательный порядок |
| **Figma→HTML+CSS** | hierarchical DOM/layout, alignment, responsive viewport, box geometry, style tokens | токенизация текстом не показывает геометрии; обычный AST KNN не достаточен |
| **Документация C#** | Roslyn AST, method scope, null/dataflow, async/exception, real API version | whole-file `Contains(".")` и unrelated null guard создают ложные BugFix labels; order в коде обязателен |
| **Документация Angular** | component/template tree, change detection, lifecycle and event/data direction, Angular version | статический bag similarity не заменяет typed template semantics |
| **Документация SQL** | dialect, schema/JOIN cardinality, NULL/3VL, aggregate semantics, transaction law | смешение table/column/NULL и порядок JOIN с неверными приоритетами |
| **DPO Builder** | exact violated law, chosen/rejected same prompt, independent judge, nontrivial correct-vs-wrong, no both-bad | синтетические лёгкие negatives, утечка экзамена, declaration of teacher without receipt |

Эти 7 названий и цель предметной оценки установлены в [LEARNING-EVALUATION-CONTRACT.md](../LEARNING-EVALUATION-CONTRACT.md). Переход между доменами допускается **только** при сохранении типа/смысла embedding, law compatibility и проверенном ordinary consumer. Сложность 1 → 100 примеров не означает автоматическое однотипное поведение всех функций.

## 5. Роли и стоп-критерии

**Human-in-the-loop = CODEX, Astra 6 UltraCode:** он *выбирает* задачу и независимо *оценивает exact final DPO chosen/rejected и реальный Student output*. **Teacher = разрешённая внешняя открытая китайская модель** и создаёт **обе** стороны chosen/rejected. Developer/CODEX-промпт не должен тихо заменить сторону rejected результатом слабого Student и переименовать author. Сохранение DPO+checkpoint, корректный API и «изменились веса» не означают полезную обученность. Сейчас выполнен **source audit + 8-case V8 арифметический proxy**, а **native .NET learning, independent CODEX/Astra eval, actual approved teacher и blind production-quality run не выполнены**.

**Ограничение новизны:** novel N37 к *PredicateDatasetRouter* относительно просмотренных соседних документов; позитив E22, E09/E14/E26/E43, RP01 cold-start, RAG roles и прочие пробелы — ранее известные findings, здесь с адресами. До публикации `ACCEPT` необходим независимый review при exact commit и readback. Закрытый `llm_mvp_core/docs/plans/replanning-possible-diffs` не менять.

**Источники/навыки для продолжения:** [RAG MASTER](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [MECH-01](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md), [RESEARCH-03 DPO](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md), [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills): `architectural-decision-testing`, `phase-planning`, `plan-review`, `code-examination`, `prompt-patch-authoring`, `conversation-intent-and-self-reflection`, `experiment-design`, `learning-path-audit`, `human-readable-architecture-audit`.
