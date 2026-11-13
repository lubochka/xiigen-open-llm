# 41 · PrepareToGPL parity: метрика успеха, контроль неизменившихся алгоритмов и доменная граница

**18.10.2026 | SOURCE_COMPARISON / NEW CROSS-BRANCH HYPOTHESES / NOT_RUN.**

**Актуальная глава приоритетов:** [40 · restore before attribution](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md).  
Pinned core: [historical PrepareToGPL@31346d89](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2), [current codex/dpo-implementation@a392b6be](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6). Writable target only planning/possibleDifs; protected core research is read-only.

## 1. Почему нельзя сравнивать историческое «PASS» с RP01 по одному числу

Из архивного [models/index.json](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/models/index.json) следует: OrchestrationReasoning held_out_accuracy=1, **4 held-out**. Там же явно `semantic_quality_claimed=false`. У модели `orchestration-v1` записана `accuracy=0.9`, у `translation-v1` — `accuracy=0.3333`. ЭТО РАЗНЫЕ получатели/задачи/знаменатели; нельзя выводить общий «качество PrepareToGPL=100%». Более того, у `orchestration-reasoning-v1` archived index указывает `learned_pattern_count=12`, а [manifest](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json) — `learnedPatternCount=15`. Число в реестре нельзя использовать как безусловную метрику фактически загруженных паттернов.

У RP01 [recovery evidence](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): веса Student q/v LoRA обновлялись, но C# free answers 0/3→0/3, EN→RU 0/4→0/4. Суммировать это как «доказанное ухудшение одинаковой нейросети относительно PrepareToGPL» нельзя: **разные задачи и потребители**.

| Что сравниваем | Обязательные поля до вердикта |
|---|---|
| Archive guide-pattern / Unit memory | source family, training rows, independent heldout, state manifest, loaded pattern IDs, actual consumer response |
| RP01 Student q/v LoRA | B0 origin/pretraining, tokenizer exact identity, DPO data accepted, update count, reference, logP margin, actual free decode |
| Translation memory / hybrid kNN | direction, topic, exact-source vs signature/prefix/structure hits, selection order, domain law guard, checkpoint version, answer text |
| C# / SQL structure retrieval | AST grammar version, file-disjoint split, node-vs-file leakage, candidate pool, k, answer/plan correctness |
| Orchestration Graph / learned flow | required dependencies, accepted source gate, learned pattern state, path/routing choice, normal API call |

**Новый критерий:** `historical parity` требует равенства (domain, task, input, consumer, effective learned state, evaluator, source partition). При любом несовпадении фиксируем `NON_COMPARABLE`, а не интерпретируем изменение числа как learning regression.

## 2. G41-01 — изменённое значение reward margin в переводе: метрика могла стать честнее, не хуже

В [old TranslationMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L69-L83) legacy `TrainBatchAsync` записывает `RewardMargin: 0.03` несмотря на прямой комментарий `this path does not claim unit-level translation learning`. Старый `EvaluateAsync` возвращал `RewardMargin: _trainingHistory.Count > 0 ? 0.03 : 0.0` ([old L188–198](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L188-L198)). В текущем [TranslationMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L69-L86) legacy reward заменён на `0.0`; текущий `EvaluateAsync` также `RewardMargin: 0.0` ([L192–201](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L192-L201)).

Старый `TrainStructuredBatchAsync` выводил `rewardMargin` из наличия подстроки `[WRONG:` во всех rejected JSON ([L128–145](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L125-L146)); текущий записывает 0.0 ([L125–145](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L125-L145)). Это **не нейронный DPO margin**, а historical metadata/heuristic. Уменьшение этого поля не свидетельствует, что качество реальных переводов снизилось.

**Micro `P41-METRIC` · topic: translation / profiles EN→RU, RU→EN:** A=архивный сервис/хранилище, B=текущий, один и тот же accepted structured batch 4 строк, одна и та же 6-case unseen panel, reuse exactly the same checkpoint; зафиксировать **независимо**: 1) `RewardMargin`, 2) `TranslationResult.TranslatedText` + корректность смысла, 3) `learned unit state hash`, 4) latency. Если `RewardMargin` изменился, а выход и state одинаковы — **METRIC_DRIFT_ONLY**. Если выход разный — отдельно исследовать caller/adapters/checkpoint. Не обновлять параметры в measurement-only arm. A/B на разных ветках возможен только при совместимых сериализациях; иначе проверять одно состояние на двух инструментальных readers и пометить компарабельность.

## 3. G41-02 — старый переводческий bootstrap засчитывал «успех» шире

[Good TranslationRoutingFlow.cs L82–93](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationRoutingFlow.cs#L82-L95): `passed = translationResult.Confidence > 0.0`. В [current L82–98](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationRoutingFlow.cs#L82-L98) добавлено `!AdapterId.StartsWith("bootstrap-", Ordinal)`; теперь bootstrap не является `TRANSLATED`, даже если confidence положительна. Это изменение **границы PASS/FAIL**, а не показанный дефект learned translator.

**Micro `P41-BOOTSTRAP` · translation:** 2 negative bootstrap cases + 4 learned adapter cases, одинаковые adapter outputs injected in обе версии Flow (test double допустим только для branch-boundary C, не для утверждения о качестве обучения). Сравнить `OverallVerdict`, `FinalHumanResult`, `AdapterId`, confidence; затем отдельно сравнить 6 действительно полученных native translation outputs с независимым meaning judge. Успех теста граничного условия **не** равен положительной метрике обучения. Если ошибка только в PASS, classify `VERDICT_DEFINITION_DRIFT` и обновить comparison ledger.

## 4. G41-03 — неизменившиеся реализационные ядра: не приписывать им branch regression

Сверены exact Git blob SHA обеих pinned веток:
- `27.TranslationDomain/TranslationTrainedUnits.cs`: **identical blob** — обучаемые `_patternCorrections`, `_signatureCorrections`, `_knnStore`, `_wordMap`, `_phraseMap` и прочие механизмы не менялись *в этом файле*;
- `27.TranslationDomain/TranslationSemanticTokenizer.cs`: **identical blob** — POS, роли, concepts/signatures не менялись *в этом файле*;
- `27.TranslationDomain/TranslationCheckpointCache.cs` и `06.VectorSearch/HnswIndex.cs`: **identical blobs**.
При этом `TranslationMachine.cs`, `TranslationRoutingFlow.cs`, `09.Inference/TextGenerator.cs`, `10.LoRA/LoraLinear.cs`, `11.DPO/DpoTrainer.cs` **различаются** (проверены file SHAs). Следовательно, называть «новая версия переиначила POS токенизацию или обучаемую память непосредственно здесь» — **unsupported**; всё ещё открыты вызовы, возможность обучения, source admission, data split, checkpoint restore и consumer use. В частности, **не делаем вывод**, что current RP01 вообще использует эти Unit: [RP01 algorithm/recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) описывает другой Student path.

**Micro `P41-IDENTITY` · EN→RU and C# code docs:** on 4 valid cases each, compute content hashes plus instrumentation of selected `unitId`, `checkpointRef`, `selectedKnnHit`, `generatorType`, `finalConsumer`. Treat missing call as `NOT_REACHED`, not a negative kNN learning result. With the same serialized state, compare normal consumer outputs. If equal, hypothesize measured decline originates upstream/downstream rather than unchanged unit source.

## 5. Обязательная доменная применимость признаков

| Feature / intervention | Где plausibly полезно | Где запрещено переносить без отдельного опыта | Small evaluation |
|---|---|---|---|
| Roslyn AST kinds, block boundaries, dependency order | C#, source-to-doc, refactor, structural code search | RU↔EN general translation, free dialogue | Code: unseen files, compile/semantics of code; source-disjoint split |
| POS, case/gender/agreement, clause order, negation | RU↔EN, technical translation | C# AST kind scores без языка natural text | Translate: preserve agent/patient/negation/manner, 6–12 unseen |
| kNN exact/source+signature + semantic vector | repeated domain examples, cold-start correction | open-world tasks with no counterpart or stale/index mismatch | hit@k and actual answer; compare first-hit vs top-k verified |
| Graph path law/consequence/edge context | planning, orchestration, design-to-code dependency flows | token-level translation where there is no graph claim | plan valid dependencies + novel input, count false positives |
| Constant/bootstrap confidence, fixed rule template | smoke-test contract only | learned semantic quality claims in all domains | missing-memory negative control must NOT earn semantic PASS |

**Decision rule:** domain-specific feature opt-in only after the same-domain blind comparison to baseline; report every negative domain as deliberately unpromoted, not as global algorithm failure. [Existing E09/E14/E26 nuance](38-MASTER-PRIORITIES-POSITIVE-HISTORY-NEW-HYPOTHESES-AND-MICRO-DECISIONS-2026-10-16.md) already establishes historical structural/POS positives with comparability caveats.

## 6. Новизна и контроль claims

- G41-01/02 are **fresh direct branch-to-branch evidence differences in scoring/admission**, not an assertion no prior agent has mentioned these anywhere across 367 planning files; reconcile before assigning a unique globally new ID.
- G41-03 is an **important negative control**, not a new learning method: it saves time by reducing incorrectly targeted fixes.
- Need source-level comparability before any causal inference, independently authored quality judge for C#/ENRU, exact checkpoint identity, and proper ordinary consumer.
- No .NET native run/training/human evaluation in this audit. No edits to protected `llm_mvp_core` research. No automatic reinterpretation of old failed matrix experiments as failures of KNN (see [existing matrix→kNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md)).
