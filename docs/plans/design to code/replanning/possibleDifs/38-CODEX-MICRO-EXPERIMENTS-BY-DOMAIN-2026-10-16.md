# N38 MICRO — быстрые доменные фальсификаторы и исполнение без переписывания RP01

**Дата:** 16.10.2026. **Состояние: READY FOR EXECUTOR / NOT_RUN_NATIVE / NOT_RUN_INDEPENDENT_CODEX_ASTRA_REVIEW.** Проверяемый код `llm_mvp_core/codex/dpo-implementation@aa284ba21790c5d7601328997cd6174c85cee878`. Этот документ — **план исполнения, не заявление о реальных экспериментах**. Единственный исполненный источник — маленький JS source-logic proxy [N38 receipt](38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json). Для повторного запуска получить fresh HEAD и pinned branch/input hashes; чужие результаты, веса и private evaluation files не изменять.

## Архитектурные инварианты для всех опытов

1. **Identify learner and ordinary recipient** before any measurement: (A) neural Student `DpoTrainer.StepVector→LoRA→TextGenerator`; (B) `TranslationMachine→SemanticTokenizer/Executor/KNN/Verifier→TranslationRoutingFlow`; (C) generic `KnnLogicalUnit`; (D) generated-capability `LogicalDpoTrainingUnit`/`ReplayFusionRanker`. A/B без реального общего получателя не вычислять в единый score.
2. Freeze `code SHA`, `source dataset SHA`, semantic law families, individual training/eval ids, model/LoRA/tokenizer/vector-index version and seed, decoder settings, number of optimizer calls and checkpoint refs. Evaluation sample **source-family disjoint** for claims of transfer; exact-known probe separate. Train and evaluation not the same files/templates merely renamed.
3. **Human role = CODEX with model astra 6 ultracode**: selects the laws/prompts and approves semantic gold/rubric *before* looking at treatment predictions. **DPO author = external open Chinese model**: legally authorised exact provider/model/terms required, generates *both chosen and rejected*. If no provider is available, **do not invent a call**; diagnostic fixed synthetic material with truthful `DETERMINISTIC_TEST_FIXTURE` status is acceptable but cannot be called production DPO. **Independent quality judge = separate CODEX astra 6 ultracode**, blind to arm/order; no self-review by the authoring run.
4. Score complete user-visible answer and negative controls. For each case record `correct-law`, `wrong-accepted`, `abstain`, `partial`, domain, confidence, gate reason, used component and trace (match IDs, source law, prompt, chosen/rejected model-visible token IDs, optimizer step, checkpoint identity); for code require compiler/execution when law depends on it.
5. Scientific acceptance statuses only: `SOURCE_CONDITION_CONFIRMED`, `NATIVE_MECHANISM_VERIFIED`, `TRAIN_OVERFIT_VERIFIED`, `HELDOUT_GAIN_OBSERVED`, `NO_GAIN_OBSERVED`, `INVALID_OR_LEAKED`, `NOT_RUN`. Reporting a token hash or unit test PASS is never `HELDOUT_GAIN_OBSERVED`. Prefer single native `dotnet test --filter FullyQualifiedName~N38_...` to long Windows CI.

## M38.00 — сначала установить обычный потребитель (быстрый source trace)

| Source seam | Must trace / artifact |
| --- | --- |
| RP01 neural | `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L100-L210` → `11.DPO/DpoTrainer.cs` → `09.Inference/TextGenerator.cs`; model/root tokenizer/reference and exact chosen/rejected mask |
| Logical capability | `18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1351-L1390` → `11.DPO/DpoConvergenceMonitor.cs`; actual configured threshold/window |
| generic KNN | `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L110-L265` → real caller and `06.VectorSearch/HnswIndex.cs`; do not assume the generated-capability embedding unit is this generic class |
| Translation KNN/Units | `27.TranslationDomain/TranslationTrainedUnits.cs` → `27.TranslationDomain/TranslationDirectionAdapters.cs` → `27.TranslationDomain/TranslationRoutingFlow.cs` |
| Replay | `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1780-L1890` → `18.CapabilityTraining/ReplayFusionRanker.cs` → legal replay manifests and `BuildKnnTrainingCorpus` |

**Gate M38.00:** if actual caller→recipient is absent or unavailable, record `NOT_REACHABLE_IN_THIS_PRODUCT_PATH` and **do not** claim production quality damage. A method in the repository is not automatically an active product pathway.

## M38.01 — generic KNN duplicate-ID → wrong output (code/AST + routing)

**Primary law:** when two different lessons are submitted, the original lesson must remain retrievable under its own identity; conflicting same-id must be rejected or explicitly versioned. Input: two approved source-disjoint code snippets with opposite verified C# action labels; repeat in ordinary domain routing with two unlike commands. Exact content/labels prepared by CODEX astra 6 ultracode.

**A (control):** `KnnLogicalUnit.Train` with two **unique ids**, same texts/vectors/labels/order.
**B (current defect trigger):** same rows but **equal trimmed ExampleId**, distinct target labels; do not alter text vectors.
**C (shadow fix):** duplicate id gets explicit `DUPLICATE_EXAMPLE_ID` error *before index mutation*; no learned output claimed.
**D (negative):** two rows same id **and same semantic law and same output**, verify whether duplicates still contaminate counts/search, not necessarily output correctness.

*L0 source-only:* index appends2, map count1; refer to [N38 receipt](38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json). *L1 native:* implement one targeted test with existing `LogicalUnitRequest` helpers and injectable HNSW trace (no permanent product instrumentation required). Train 2 examples per arm, issue 2 exact-source queries, 2 source-disjoint same-law queries and 2 opposite-law distractors. Log hit-vector origin, returned id and mapped output; use same tokenizer and fixed seeds where possible. Because HNSW has an unseeded static RNG, never pretend paired native ANN results are bitwise deterministic; query/inspect exact matching first and use brute-force cosine shadow to separate ANN recall from id mapping.

**Pass mechanism**: B maps hit originated from A to B's later stored output; A does not. **Pass quality**: blind CODEX judge reports fewer wrong-accepted law outputs on source-disjoint requests after remedy. **Falsifier**: duplicate ids impossible under actual admissible producer, or execution never retrieves overwritten id; downgrade priority. Memory cost and insertion order matters. STOP if no caller.

## M38.02 — vector-density loss ↔ early-stop (EN→RU + SQL)

**Primary law**: stopping should depend on preference progress and domain heldout skill, not arbitrary count of active teaching-vector slots. First **no training at all**: for one frozen exact response-masked batch, read-only derive `rawDpoLoss`, `weightedLoss`, `VectorSignal`, `RewardMargin` and `Accuracy`. Choose two vectors same shape and finite: zero active slots vs exactly60 active slots, weight1. Same B0/reference/LoRA values and pair.

| Arm | Vector active slots | Expected `VectorSignal` | Exact `StepVector` weighted loss for raw log-loss L |
| --- | ---: | ---: | ---: |
| A, sparse | 0 | 1 | L |
| B, dense | 60 | 2 | 2L |
| C, coordinate permutation | 60, different positions | 2 | 2L |
| D, semantic-control | 60, signs/directions inverted | 2 | 2L |

A–D **have the same scalar density objective under a frozen model**, but these vectors may induce different legal upstream behaviors outside `VectorSignal`; keep raw DPO pair identical. SOURCE-LOGIC receipt uses `L=ln2`, resulting ~0.693147 vs 1.386294; that is **not** a Student run.

Then optional tiny actual capability experiments (e.g. 8 same-domain approved, 8 independent heldout; run ≤12 updates each; actual permission and unique publication root for each arm): compare **control early-stop by reported weighted `lastMetrics.Loss`** versus **shadow monitor using normalized unweighted objective computed under the same policy**. Do **not** silently change gradient scale and claim only monitor matters: use a 2×2 (monitor metric × teaching density) and frozen exact data, same DPO config/seeds; log actual optimizer calls and valid saved checkpoint identity. Gate if training pairs do not admit under server authorization or model has no learnability.

**Outcome matrix:** early-stop step differs with density but judged skill unchanged ⇒ miscalibration, not semantic learning gain; normalized metric improves C#/ENRU/SQL independent results with same compute ⇒ conditional causal gain; no difference ⇒ `NO_GAIN_OBSERVED`. RP01 simulation **does not call this convergence monitor**, so N38.02 must not be labelled root cause RP01's fixed50+50 updates.

## M38.03 — replay fusion, three-way feature claim vs reality (C#/SQL/translation)

Freeze ≥4 **legally admitted, verified** existing replay manifests in *same permitted domain*; otherwise source-only simulation cannot be called a valid product run. Build a mini-law panel of 4 existing outputs: gold target law, negated/opposite law with many common words, paraphrased gold with few words in common, unrelated one. Source-quality gold adjudicated blind *before* ranking.

**A:** current `ReplayFusionRanker.Rank(topK=3)`.
**B:** lexical BM25-like alone (negative control).
**C:** Jaccard token-set alone (negative control).
**D:** domain-specific signed relation/AST/source-role eligible rerank **after legal attestation**, not hardcoding an eval answer; no change to source manifests/checkpoints.

**L0 deterministic check:** code records `pattern = vector` and linear combination ⇒ 65% overlap+35% BM25-like; no independent KNN/AST input. **L1 native:** run `Rank` over frozen list, capture top3 and scores, feed selected memory to *the actual generated-capability retry path* (not a fake ranker result), rate end-user correct-law/wrong/abstain per domain. Evaluate ≥4 source-disjoint independently gold-marked questions; preserve both positive and hard-negative roles. Separate genuine source-language semantic-rank changes from selection by createdAtUtc. A corpus with ≤3 records **cannot test top3 admission loss** — ranker returns same list, so report N/A.

**Interpretation:** D improves retained correct-law ratio under equal data and cost, and ordinary consumer confirms ⇒ promote to RP03/domain-conditional trial; D just picks different records with no better result ⇒ no gain; AST improves C# but worsens translation ⇒ **do not deploy globally**.

## M38.04 — matrix→KNN transfer must be identified from real paired history, not from FAIL/PASS headlines

Use already archived [`E09`](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E09StructuralKnn.cs), [`E26`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) and source archives listed in [historical crosswalk](38-CODEX-EVIDENCE-CROSSWALK-RP01-HISTORICAL-BRANCHES-2026-10-16.md), but demand original run JSON + commit + semantic task before claiming same transferred method. Specifically inspect old `FAIL` whose feature is reported to work after its function moved from matrix to kNN. Create one single law with four arms at same input/history: **no rule**, **matrix rule only**, **KNN rule only**, **combined**, with one shared independent holdout (C# AST sibling relation 8 cases; ENRU subject/negation/order 8; SQL `JOIN ON`/`WHERE`/`IS NULL` 8). For translation negative controls modify roles and explicit negation; for C# keep compilation target; for SQL mark dialect and actual query result.

Score correct-law@1, wrong-accepted, abstain, old-law retention, runtime/memory. Matrix FAIL + KNN PASS on DIFFERENT data is **not** causal relocation proof. If historical code/saved outcomes cannot be paired, report `HISTORICAL_CAUSAL_PAIR_NOT_RECOVERED`, never invent numbers.

## M38.05 — neural RP01 checkpoint learnability ladder, separate from N38 KNN

This is the highest priority **for explaining current neural regression**; it was already proposed in [RP01 causal diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md). Do not call it a new N38 discovery.

**P0 exact mask/gradient:** one 8–32-pair legal source-family training micro, selected by CODEX/Astra; external authorised open Chinese teacher creates chosen/rejected, CODEX/Astra independently checks both sides and exact source law. For source-only diagnostics, intentionally artificial truthfully labelled pairs allowed *only* as synthetic/failing regressions. Run finite-difference directional gradient and null/reference control, record model-visible prompt/response mask positions, EOS and exact tokenizer SHA; if no finite-difference agreement, stop before large data.

**3-arm learnability:** (A) exact random B0 DPO-only; (B) same B0 with supervised chosen-only cross-entropy warmup then DPO; (C) warmed B0 chosen-only SFT control. Pin *equal training-token/compute budget* and separate checkpoint roots, optimizer RNG seed and reference. Do not assert all three arms have identical optimizer state if their preparation differs; record it. Measures train overfit AND separately free generation on 8–12 source-family-disjoint blind C#/ENRU examples, score by real compiler/gold translations. If same-student tiny exact tasks cannot be overfit even on train, examine optimizer, mask, EOS, target and frozen layers rather than adding500 random pairs. No predetermined promised +95% improvement.

## Execution sequencing, review, and publication contract

1. **Architecture / research** (`human-readable-architecture-audit-SKILL.md`): N38 evidence map, ordinary recipient, domain applicability, earlier reported findings dedup. **Planning** (`phase-planning-SKILL.md`): one causal factor per mini-task, sequence small prototypes before integration. **Plan review** (`plan-review-SKILL.md`): independently check task scope, input provenance, no conflated metric and no unreviewed proposed fixes. **Code examination** (`code-examination-SKILL.md`): exact source line + zero bypass of admission guard, fail-closed behavior and code-to-runtime proof. **Prompt engineering** (`prompt-patch-authoring-SKILL.md`): detailed executable instructions and blind selectors/evaluators; do not make solver both author and judge. **Human communication** (`conversation-intent-and-self-reflection-SKILL.md`): short readable Russian result vs unknown; **experiment design** (`experiment-design-SKILL.md`): controls, counterexamples, stop rules. Skill root: [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills).
2. Work order for each test must include `task_id`/`allowed_paths`/`forbidden_paths`/`exact inputs & SHA`/`single independent domain gold`/`available resources (no invented external teacher)`/`single-factor A/B and negative control`/`report artifacts`/`review gate`/`stop code`. Avoid uncontrolled fan-out or giant CI and avoid changing `docs/plans/replanning-possible-diffs` in core.
3. If implementing a fix later, only owner of the relevant slice may do it: neural DPO and LoRA under RP01, semantic vectors/index under RP02, Units routing under RP03, graph/replay under RP04 or assigned slice. Report cross-owner dependency but do not silently combine changes.
4. Independent verifier (separate CODEX/Astra 6 UltraCode pass) must inspect actual generated text, source law and before/after, not just compile/Unit pass. Use explicit outcome enum: `NO_GAIN_OBSERVED` is a useful falsified hypothesis; never retitle frozen FAIL to PASS.
5. **Deliverable:** compact JSON per case and one MD with pairs/semantic family, vector masks, actual endpoint, source proof, selected neighbor/early-stop step, outcome, latency and cold checkpoint recovery. If no native runtime available, `NOT_RUN_NATIVE` and reason, preserve source-only receipt.

## Ready-to-reuse executor prompt (do not skip source checks)

> You are CODEX running model astra 6 ultracode as a **selector/investigator**. Read current scoped AGENTS, WorkLeft RAG, relevant core skills and N38 crosswalk. Do not author external-teacher DPO; only an actual approved open Chinese model may generate the DPO chosen and rejected sides. Create a source-bound minimal causal A/B for **one** N38-ID or RP01 learning mechanism and record the ordinary consumer, frozen baseline, independent gold and exact code SHA. No large Windows CI, no threshold tuning, no unverified score. Separate your author role from a **fresh independent CODEX/Astra 6 UltraCode quality evaluator**. Never mutate protected core research docs, frozen training receipts or legacy verdicts. Deliver a table of positive/negative evidence and report NOT_RUN on unavailable resources.

[Novel source findings](38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md) · [Evidence crosswalk](38-CODEX-EVIDENCE-CROSSWALK-RP01-HISTORICAL-BRANCHES-2026-10-16.md) · [logic receipt](38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json).
