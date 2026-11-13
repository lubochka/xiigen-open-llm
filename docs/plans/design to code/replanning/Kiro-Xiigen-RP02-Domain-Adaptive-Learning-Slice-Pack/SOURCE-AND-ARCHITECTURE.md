<!-- RP02-CONTEXTUAL-LAW-KNN-ACTIVE-ROUTE -->
**ACTIVE CORRECTION — 14 Oct 2026:** [domain-law vector, syntactic/morphological POS and AST roles, relation-aware kNN and factorial science](DOMAIN-LAW-CONDITIONED-KNN-AND-LEARNING-2026-10-14.md). 14 original task cards + 3 standalone [RP02.01D](tasks/RP02.01D.md)/[RP02.02D](tasks/RP02.02D.md)/[RP02.03C](tasks/RP02.03C.md). Start 01D feature/caller source trace and 03C prereg in parallel with first RP01 Student DPO; 02D identifies actual next-step/typed consumer under own bounded authority. **C06 hits[1] typed fusion and its one `KnnLogicalUnit` consumer are NOT the universal next-word law-aware algorithm or prerequisite to every experiment.** Hard/soft domain laws, POS probabilities, same-referent vs other-referent agreement, vector schema, contextual adaptive kNN and normal consumer outcome must be evidenced separately. C06 still runs when the real typed memory consumer needs it. Historical first-k>1 ordering below is superseded ONLY on priority/scientific attribution, never by deleting unique original tasks, security, licenses, change grants or native release requirements. READ_ONLY/NO_CHANGE adds 0 tests; changed deterministic risks use smallest affected probes; learning is evaluated on heldout real-domain tasks and full PC cost, not coverage or Windows CI. Documentary edit only; Student, Astra and experiments remain NOT_MEASURED/NOT_PERFORMED.
<!-- /RP02-CONTEXTUAL-LAW-KNN-ACTIVE-ROUTE -->

> **2026-10-14 CODE-SEAM EXECUTION LINK:** [FASTEST LAWFUL LEARNING LOOP](FASTEST-LAWFUL-LEARNING-LOOP-2026-10-14.md) makes K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs current `hits[0].Output` and trimmed duplicate-ID vector/payload risk the first C06 investigated seam; verify live K SHA/callers before actual patch. SQL joins the two existing early law pilots *after* earliest lawful learning experiment, no seven-domain blocker. 

> **2026-10-14 CURRENT K IDENTITY RISK / direct code observation:** The `KnnLogicalUnit.Train` loop writes `exampleMap[trimmedId]` (overwrites) and calls `HnswIndex.Add(...,trimmedId)` (appends ID) unconditionally. A duplicate trimmed `ExampleId` can yield `index.Count > exampleMap.Count` and mismatched retrieved vector/payload. `ExecuteAsync` sources `Output` from `_examples[hits[0].id]` and exposes other IDs as evidence. Treat this as a **candidate data-integrity confound** in RP02.02A: inspect actual duplicates/consumer and screen identical ID source-family, tenant, law and permission before k>1/adaptive-k scientific ablation. If the changed training owner requires a guard, use one focused collision-denial boundary probe; no artificial coverage quota. No measured effect/actual Student learning follows from this source reading. Source: K `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs::Train/ExecuteAsync` and `06.VectorSearch/HnswIndex.cs::Add/Search`, current ref `codex/dpo-implementation`.

# RP02 — primary source map, existing implementation and intended causal roles

## 1. Source namespace and priority

P = `lubochka/xiigen-planning@llm_plan3`; D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue`. K=`lubochka/llm_mvp_core@codex/dpo-implementation`. Current branches must be pinned to actual HEAD/blob at each executor start. References below are **routes and previously observed evidence**, not an assertion every listed file was reread in this pack.

**Original source chain (DO NOT read S04 alone):** D/`original task.md`, `task prompt.md`, `goal of the round.md`, `Алгоритм создания ДПО.md`; D/`09.09.26WorkLeft-00...`, `WorkLeft-02-teaching-chinese-model.md`, `WorkLeft-03-decisions-register.md`, `WorkLeft-RAG-MASTER-INDEX.md`, `WorkLeft-RAG-00-usage-guide.md`, `RAG-01..04.md + ndjson`, `WorkLeft-MECH-01...`, `WorkLeft-RESEARCH-01...`, `WorkLeft-RESEARCH-03...`, `WorkLeft-PROMPTS.md`; D/S00–S10 packs and extra subjects; P/G original DPO and DynamicGraph, P/F `DECISIONS-LOCKED.md`, `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`, `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md`; P/V IDE RAG and LeftToWork. Every RAG route needs visited id, source root, **primary** path/ref/section, authority and consumer.

**Replanning results directly used as attributed baseline:** [R04](../results/R04/FINDINGS.md), [R06](../results/R06/FINDINGS.md), [R08](../results/R08/FINDINGS.md), [R12](../results/R12/FINDINGS.md), [R13](../results/R13/FINDINGS.md), [R21](../results/R21/FINDINGS.md), [R22](../R22-CAUSAL-EXPERIMENT-CATALOG.md) C01–C06, [R23](../R23-PC-STATE-FEEDBACK-LIFECYCLE.md), [R25](../R25-ASTRA-REVIEW-PACKET.md), [JOIN](../PARALLEL-R01-R03-JOIN.md), [historical thinking](<../previous rounds thinking results.md>). Source depth of a prior Rxx cannot be promoted to current direct code read. R01/R03 research completeness is open, but a local E2 consumer trace can proceed.

## 2. Directly inspected current code anchors (13 October)

| K path and actual observed mechanism | Existing vs candidate, exact falsifier |
|---|---|
| `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` (`Train`, `ExecuteAsync`, `Vectorize`) | BPE over supervised text, HNSW index, vector BPE-count/predicate-count halves, _k fixed from training. `hits[0]` output; others evidence only. No observed learned dynamic k/typed fusion effect in this function. |
| `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs` | 60 slots = 22 predicate + 5 domain + 32 roots + 1 other; `NormalizeRoots` regex `[^a-z]+` and lowercase means potential non-Latin/operator loss. DIFFERENT space from KnnLogicalUnit. Do not replace all tokenization from this diagnosis. |
| `23.SemanticCompilerAdapter/DomainClassifierUnit.cs` | Real `Classify`, optional `Train`, predicate and context heuristics, `Confidence`; does not establish calibrated 7-subject law mapper. Its `Domain` is development-domain axis; do not confuse with subject or intent. |
| `06.VectorSearch/IVectorIndex.cs`, `BruteForceIndex.cs` | Indexed Search(float[],topK); exact brute-force cosine, in-memory only (`Save/Load` throw). Use as experiment reference, not production persistence guarantee. |
| `17.DynamicDomains/DynamicDomainGraph.cs` | `DomainGraphClosure` and `OrchestratorExpander` handle reachability/path bridges. Topology is not E2 domain classification or trained E4 graph relation state. |

Additional real paths to **trace afresh before modification**: `01.Tokenizer/BpeTokenizer.cs`, `06.VectorSearch/HnswIndex.cs`/ `Distances.cs`, `23.SemanticCompilerAdapter/SemanticTeachingPromptCompiler.cs`, `26.TrainableRouter/Training/TrainableRoutingFlow.cs`, `26.TrainableRouter/Training/RoutingProfileTrainer.cs`, `08.GraphRag/HybridRetriever.cs`, `18.CapabilityTraining/ReplayFusionRanker.cs`, `00.Core/LogicalContracts.cs`, `21.AgentInfrastructure/Logical/LogicalUnitDynamicGraphNode.cs`, actual caller selecting a `KnnLogicalUnit<TInput,TOutput>`. Examine whole file, all callers, consumers and impacted focused tests BEFORE editing. Do not invent a generic domain host/LoRA projection API.

## 3. Strict four-role boundaries

- **ILearningUnit** receives domain, typed input and representation version/schema and expected typed output; handles associated many→many output, not just a count of IDs. RP02 must not break future Unit association, output references or source provenance.
- **ILearningFlow** selects/corrects lawful typed Unit→Unit relations and has separately trained relation state, **not** kNN similarity alone. RP03.
- **ILearningOrchestrator** asynchronously combines admissible paths, per-branch budget/cancellation/partial output and learned path policy. RP04.
- **ILearningDynamicGraph** learns multi-order pattern relations and transfers compositions; Graph DPO Builder qualification is separate. RP04/05.
- Shared DPO/LoRA infrastructure is *not* evidence of four live optimizer recipients. A prototype nearest-neighbor policy must show real state/signal/ordinary consumer before claiming it learned. Do not automatically train all four roles.

## 4. Candidate mathematical E2 policy, NOT already implemented

Let `d=(subjects[],intent,sourceLanguage,targetLanguage,stack,dialect,version,role,project,learner)`, with `UNKNOWN/MIXED` and confidence calibrated from open development families; project/tenant/permissions and execution constraints remain deterministic. `P_d(q)` = eligible examples after rights, version, schema, negative/warning-role and provenance check.

Versioned feature groups `φ_j`: identifiers/lexical, syntax/operators, numeric+units, semantic sense, graph/dependencies and authority; channel schema includes names, nullability, units, shape, coordinate-id, codebook/tokenizer/encoder/index version and allowable loss. Counterexample families: same-law paraphrase, near text but opposite law, novel lawful composition. A string/vector size is not coordinate identity. LoRA `A,B` changes weights of its actual recipient, **does not automatically emit semantic tokens**; any vector-codebook projection requires an explicit encoder/decoder/index owner and downstream test.

One **candidate** re-rank formula (subject to domain validation, normalization and ablation):
`s_d(q,z)=Σ_j w_j(d,q)·sim_j(φ_j(q),φ_j(z)) - λ_d(q)·lawRisk(z)`, with `w_j>=0, Σw_j=1`, known units and feature versions. `k_d(q)=clip(g_d(ambiguity,density,margin,lawRisk,latencyBudget),1,min(|P_d|,Kmax))`; abstain when no legal candidate. This may be asymmetric/nonmetric: use exact eligible retrieval or metric ANN **candidate generation** and rerank, never feed unknown nonmetric distance directly to HNSW as guaranteed. Learnable only with named param state, lawful label/feedback, update, snapshot, loader and downstream evidence. Separate comparison factors: candidate eligibility; representation; fixed-k score; adaptive k/radius; exact-vs-ANN; typed multi-neighbor use.

For BM25 and vector retrieval, use calibrated rank fusion candidate (e.g. weighted reciprocal rank) rather than raw cosine+BM25 addition; only if actual caller and downstream quality warrant it. Exact index serves local diagnostic; report ANN recall and cost separately.

## 5. Domain passports and substantive laws

Seven source subjects: **EN↔RU translation**, **orchestration**, **Figma/design→HTML/CSS** across viewports, **C# documentation**, **Angular documentation**, **SQL documentation**, and **DPO Builder**. Extras and languages identified by R01/R02 remain in owner/trigger map. First two E2 panel families may be C# and EN→RU: independent provenance, one structurally different from code; other five are mapped *without demanding execution before first result*.

| Subject | Hard law example / near miss | What to preserve in tokens/neighbors |
|---|---|---|
| EN→RU | “do not send before approval” versus “send before approval” | Negation, condition, roles, quantity/time; multi-valid translations permitted |
| Orchestration | B consumes result A; reorder B before A | DAG order, permitted side effects, cancel and outputs |
| DTC | Layout valid at desktop but broken at narrow viewport | Tree, spatial constraints, assets, units, RTL if applicable |
| C# documentation | `s?.Length` nullable versus `s?.Length ?? 0` distinct expressions | AST/nullability, language version, actual method contracts |
| Angular documentation | Event from child mistaken for parent data binding | Directive/template relation, scopes, lifecycle version |
| SQL documentation | LEFT JOIN with right-side WHERE condition eliminates unmatched rows | Dialect, NULL, key/cardinality/joins/filter placement |
| Builder | chosen/rejected differ only cosmetics or are both wrong | Exact common x, lawful preference, positive/negative quality, provenance |

Illustrations are DOCUMENT_EXAMPLES, **never certified teaching pairs**. Actual DPO sides by authorized Chinese author, exact independent CODEX judge where S04 applies.

## 6. Safety and status

Exact scientific setup in [LEARNING-EXAMINATION](LEARNING-EXAMINATION.md); grants and independent review in [GOVERNANCE](GOVERNANCE-AND-REVIEW.md). Directly observed code ≠ observed outcome. This pack performs no code edit, no actual new training, no external model calls or scientific measurement. RAG whole-program closure remains with existing Rxx owners.
