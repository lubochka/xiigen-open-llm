# RP02 E2 source-to-code map — witnessed anchors and explicit gaps

**Observed 13.10.2026.** Planning branch observed HEAD e1541142c420ab923aae62ec181210cb867ec915; core branch observed HEAD 3c7f5f8c7809936e25579603c96ea6df8598fc40. These are discovery pins and may change. Freshly pin before each code task; a file reference alone is not full caller comprehension.

## Four planning roots (do not resolve every reference relative to D)

- P:D = llm mvp/design to code session: original task.md, task prompt.md, goal of the round.md, Алгоритм создания ДПО.md, 09.09.26WorkLeft* including 02-teaching-chinese-model, RESEARCH-01, MECH-01, RAG-MASTER-INDEX/00/01…04 MD+NDJSON, original S00–S10 packs, human slice map.
- P:G = llm mvp/dpo legal build: active-codex-execution-original-task.md; active-codex-execution-plan-07-dpo-builder.md; active-codex-execution-ILEARNINGDYNAMICGRAPH-PLAN-v1.md; active-codex-execution-guide-01-how-to-build-dpo.md and RAG-01 primary provenance.
- P:F = llm mvp/llm dynamic flow: DECISIONS-LOCKED.md, ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md, DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md. Read current semantic authority: StackDetector precedes destructive normalization; positive/negative/warning examples are distinct; retrieved score cannot override final arbiter rejection. Historical example numeric thresholds are not newly approved RP02 thresholds.
- P:V = llm mvp/visual studio & visual studio code continue: RAG-06-llm-dynamic-flow.md, LeftToWork-CONTINUE-* and PLAN-VS-VSCODE-CONTINUE-v1.md. These name the ordinary IDE consumer obligations, not proven E2 adoption.

**Replanning baseline to reuse, not recreate:** results/R04,R06,R07,R08,R12,R13,R18,R21,R22,R23,R24,R25; previous rounds thinking results.md (large chronology: targeted spans only); PARALLEL-R01-R03-JOIN.md; PARALLEL-R01-R25-CONTRACT.md; R22-CAUSAL-EXPERIMENT-CATALOG.md C00…C11; DOMAIN-EXPERIMENT-RECIPES.md §9; RP01 tasks/RP01.04A.md and .04B.md = READ_ONLY_FORWARD_HANDOFF. R21 is documentary curriculum; R22 science proposal; no actual E2 accuracy or independent reviewer receipt.

## Core source evidence actually read (K at observation SHA)

| File / code | Observed behavior | RP02 decision, limitation |
|---|---|---|
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs, approximate lines 102–199, 201–323; blob 2957816a88aa9cdcd3020bb81be56b5dfff165e0 | Train builds per-unit BPE 320 vocab, HNSW m=8 and predicate/BPE two-tier hashed vector; k normalized at train constructor, query Search(query,_k); best=hits[0], output=bestExample.Output, others as evidence IDs | **Confirmed first-hit consumer**. Multi-neighbor typed use and per-query k not shown in this class. Need full current file and actual production callers/consumer before patch; this does not prove every path uses it. |
| 23.SemanticCompilerAdapter/DomainClassifierUnit.cs, blob fef3b92985c93f9661e3a5eb936054c9f9edec15 | DomainClassifierRequest, labels for development domain, confidence heuristics, optional trained predicate kNN; mapping CODE_Q/BUG/etc to intents | Not automatically seven subject domains. SQL+BUG must keep independent subject, intent and stack. Route to source compiler/ordinary caller. |
| 06.VectorSearch/IVectorIndex.cs, blob e952a8679d098b9e511f2a656b98f7c55b044504 | Add(vector,id,metadata) and Search(query,topK) returning List(id,score) | Thin vector index, no typed neighbor role or law decision. |
| 06.VectorSearch/BruteForceIndex.cs, blob e49ee8fe6ce7c37dc4a6513aea1c39d0d7388a53 | Exact sorted cosine top-k over in-memory entries | Use as diagnostic baseline for matching eligible pool, not universal large-scale deploy index. |
| 06.VectorSearch/HnswIndex.cs, blob 03287c05bcd6c4422d679a3b2e37a03f418918d6 | HNSW based on cosine, Search topK, SetEfSearch, Save/Load | ANN approximation, not a generic hook for arbitrary asymmetric query-dependent distance; test recall/cost against exact after candidate geometry fixed. |
| 06.VectorSearch/Distances.cs, blob 3bef456735d33aa23228c6aee72dde308a09bcac | CosineSimilarity | Dot similarity isn't domain law. Need coordinate and length compatibility checks at real caller. |
| 18.CapabilityTraining/ReplayFusionRanker.cs, blob fd2c1d657a38ea485136fb8e875aceb8f4b16a23 | Fixed fusion weights 0.45/0.35/0.20 over lexical/predicate overlap; topK results | One other real ranking candidate, NOT by itself a trainable adaptive reranker; its ordinary consumers must be traced if chosen. |
| 17.DynamicDomains/DynamicDomainGraph.cs, blob 026def695cc0404f85d182f6aff4da4a66f46bbc | GraphClosure and OrchestratorExpander deterministic reachable/bridge logic | No proof higher-order Graph is trained; leave to RP04. |
| 01.Tokenizer/BpeTokenizer.cs, blob d7c5e69308a32bf832ec4b087097a35b26f51d64 | BPE Encode/Decode and artifact state identity | BPE IDs ≠ semantically typed IR/vector/projection; preserve exact code operators and vocabulary identity. |
| 05.Embeddings/EmbeddingModel.cs, blob a3aa205a036e3808a3f7601a95639a9bab004154 | Transformer-derived embeddings; guarded fallback to token-id bins when vector near zero | Mark fallback and collisions as separate measurement risks; don't equate output shape to meaning. |
| 00.Core/LearningMachineContracts.cs, blob 7f605d49762e3fc0c1b4d4ca490596bc9668cdd5 | ILearningMachine training boundary above Logical units, LearningTrainingItem preference triple | Do not introduce trainer inside Unit/Flow just for RP02; require typed route of a recognized signal to machine-level update. |

Source map is partly directly read, partly navigation-only as stated. At task dispatch: read fresh exact full changed files; search callers and consumers in K (21/22/23/26/09/25, 08.GraphRag, 28.SemanticNlp, 12.SemanticCompiler, etc) and other code as discovery warrants. **Do not assume this table has established a complete product call graph.**

## Direct evidence from Rxx (ATTRIBUTED to owners, not author remeasurement)

- R08/FINDINGS F-C02: DomainClassifier intent labels are not the seven subject domains; confidence .90/.95 is not empirical domain accuracy.
- R08/FINDINGS F-C03: KnnLogicalUnit _k fixed, top-hit output with all-hit evidence; initial R08 read incomplete; current RP02 reading of ~110–323 confirms the relevant branch, not consumer reachability.
- R22/FINDINGS RCOR22-07: C03 rerank same candidates vs geometry re-retrieve exact candidates; C05 exact/ANN separate candidate sets. Current status DESIGN_BINDING_REQUIRED/LEARNING_NOT_MEASURED.
- R21/FINDINGS F21R2-05: retrieval_eligible (rights/split/vector version) differs from action_admissible (law/side effects/type). Negative memory can be retrieved as warning, not executed as preferred positive.
- R24/FINDINGS: ordinary DTC/IDE user-facing learning route is independent of whether retrieval unit passes a focused test.

## Required per-task SOURCE RECEIPT

For every claimed behavior: repo/ref/HEAD, path/blob, line span, what was actually read (FULL/SELECTED/INDEX/ATTRIBUTED), exact source law and authority, current producer/caller/consumer, changed learned/deterministic factor, expected effect, falsifier, downstream requirement and next owner. Follow RAG Master→RAG MD/NDJSON→named G/F/V/D primary passage→K code/caller (with visited-set). If primary missing, mark OPEN and continue bounded reachable work; never replace it with an RAG title.
