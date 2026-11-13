# RP09 — independent instrumented view of XIIGEN hybrid learning

## Proposed observation model (not invented existing C# interfaces)
A single immutable **EvaluationCase** binds caseId/sourceFamily/domainLawVersion/language/stack/intent/rights/role/input/allowedOutputs/independent rubric/heldoutSplit. An **InvocationTrace** binds accepted-state digest, original user request hash, actual consumer, domain signal, tokenizer/IR/embedding/index coordinate schema, eligible and warning neighbors, k policy/ranker, typed Unit input-output associations, Flow corrections, async branches, Graph motifs, arbiter choice, final user result, elapsed, process-tree resource and effect receipts. **ExperimentRecord** binds preregistered control/treatment, stable panel version, objective, rule of independent scoring, missingness, raw artifacts and uncertainty. These are an interoperable *proposed schema*, not claims that types already exist; reuse present contracts where possible.

## Mechanism chain and ownership
input + domain law/rights → mapper [learnable only if real state owner] → parser/tokenizer/semantic IR [coordinate- and law-versioned] → positive/negative/warning eligible memory → exact/HNSW kNN + *query/domain-dependent* k/radius/rerank → ILearningUnit **typed many-to-many** candidate associations → ILearningFlow **input/output corrections and fan-in/fan-out** → ILearningOrchestrator **async bounded lawful path selection** → ILearningDynamicGraph **order-2/3 transferable patterns** → deterministic legality/permissions/final arbiter → ordinary core/DTC/IDE result.

Existing **ILearningMachine / Student** owns actual admissible LoRA/DPO mutation. Do NOT add Train methods to inference-only roles, duplicate a trainer, or call all search/state updates learning. Distinguish memory-index update, learned selection policy, new parameter tensor and semantic projection.

## What an independent trace must prove
| Layer | Legal deterministic invariant | One causal learned question |
|---|---|---|
| Domain mapper | Source, version, typed rights, UNKNOWN/MIXED before unsupported routing | Does *changed mapper state* improve multi-axis mapping with frozen solver? |
| Token/representation | Preserve not/no, operator, units, RTL, NULL, DOM coordinates, binding versions | Does law-aware encoder lead to better unseen task, not just cosine? |
| kNN | Scope/tenant/source, vector-space compatibility, positive vs warning separation | Do >1 neighbors influence ordinary output; is dynamic k/rerank better than fixed with same resources? |
| Unit | Output requested type, association IDs, multiple inputs→multiple outputs | With frozen Flow, does changed Unit state alter useful result? |
| Flow | Port types, OutputRef validity, fan-out/fan-in ordering | With frozen Units, does learned correction improve downstream answer? |
| Orchestrator | DAG dependencies, branch deadlines, cancellation, effect permit | Does learned route outperform deterministic lawful baseline after ALL branch cost? |
| DynamicGraph | Typed motifs, higher-order lawful relation, bounded expansion | Does learned order-2/3 state improve unseen composition vs seen retrieval? |
| Student DPO/LoRA | Same exact x, both Chinese-authored sides, accepted-by-CODEX bytes, response mask, frozen reference, one durable update | On source-family-disjoint panel, does P/C0/T diverge only due to learning and improve? |
| Ordinary consumer | Chosen checkpoint/index/host identity and legal publication | Does cold selected state actually produce changed useful output to user? |

## DPO and LoRA—not equivalent to a vector tokenizer
Maintain distinct fingerprints for (1) original x/chosen/rejected and Chinese teacher/role receipts; (2) admitted encoded prompt and **response-only** masks, preserving discriminating span; (3) LLM policy/reference score and sign/scale of backward; (4) LoRA keys/shapes/adapter+optimizer/RNG/state parent; (5) semantic representation+codebook/decoder/output projector, if they exist. LoRA matrices cannot be described as learned discrete vector tokens without an actual codec mapping and named training objective/consumer.

Reference role ≠ CODEX pair judge; CODEX operator selection ≠ independently blinded Student outcome evaluator; exact-code Astra architecture reviewer ≠ either of these. Training and evaluation *in this pack* require separately existing authorization and one-new-pair allowance; planning alone authorizes no calls.

## Independence / no duplicate implementations
Instrumentation wraps the actual selected caller and records tracing only. If the normal consumer is unreachable, mark CALL_CHAIN_OPEN; do not insert fixture-only path and count it as adoption. If RP02 owner needs to fix first-hit consumption or tokenizer semantic loss, RP09 emits a concrete file/method/falsifier packet to that owner. RP09 may implement only an actual absent examination seam (manifest parser, frozen panel, scorer, resource collector) with bounded proper review; it never repairs production algorithm in parallel.