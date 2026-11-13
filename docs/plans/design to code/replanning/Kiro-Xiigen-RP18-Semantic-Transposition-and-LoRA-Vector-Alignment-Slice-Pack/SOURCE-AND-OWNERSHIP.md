# RP18 — source, RAG, actual code and ownership ledger

## Source roots, not only names

- **P** = `lubochka/xiigen-planning@llm_plan3`. **D** = `llm mvp/design to code session/`. **R** = `D/replanning/`.
- **G** = `llm mvp/dpo legal build/` — primary legal DPO/model/data/authoring contracts, not automatically the execution permit.
- **F** = `llm mvp/llm dynamic flow/` — domain, stack, lawful arbitration, graph, BM25+kNN. Primary `DECISIONS-LOCKED.md` and `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`.
- **V** = `llm mvp/visual studio & visual studio code continue/` — IDE host/DTC source routes and `RAG-06-llm-dynamic-flow.md`.
- **K** = `lubochka/llm_mvp_core@codex/dpo-implementation`. Initial read 2026-10-13: K HEAD `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Recheck before a future edit; HEAD may move.
- **RAG route actually traversed in this planning session:** D/`09.09.26WorkLeft-RAG-MASTER-INDEX.md` (active front matter, blob `1ed3c912f88dfa4fcf0bc34d23038ba4032918a1`) → D/`09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md` current active section (blob `08c1cc7f92ba61ab05da521be516d105b366885f`) → P/D/`original task.md` (blob `10b9c752ec740d439b46e0730f4cde2562ebe033`) and F primary above → current K methods below. **This is scoped reading, not a claim that every nested RAG/NDJSON or original G/V page was fully traversed.** Each child must traverse its question-specific RAG→primary and log which spans were read.

## Direct observed code anchors (static source, not a runtime receipt)

| Core file@blob | Observed entry | What it establishes / does NOT establish |
|---|---|---|
| `10.LoRA/LoraLinear.cs@1b321e99408c961b896896983d002e3b26425b85` | `Forward` 112–125 applies base projection + scaled B·A after dropout | Real low-rank delta; **not** tokenization or index re-encoding |
| `10.LoRA/LoraApplicator.cs@5eeaf38c5fc12fa2f88dc6c247a3fb2fc250716d` | `Apply` and seeded installation | Adapter attach path; does not establish what an actual ordinary consumer loads |
| `05.Embeddings/EmbeddingModel.cs@a3aa205a036e3808a3f7601a95639a9bab004154` | `EmbedBatch` 31–55, encoder hidden states | Embedding path exists; whether it shares LoRA Student state must be traced |
| `01.Tokenizer/BpeTokenizer.cs@d7c5e69308a32bf832ec4b087097a35b26f51d64` | `Encode` 73+, `Decode` 87+, `EosToken` 38 | Codec/state exists; last-vocab-ID EOS assumption is not an approved semantic guarantee |
| `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs@d1fc22d7823f377da497881c0e540218dd22c6a2` | `VectorWidth=60` 14, `BuildVector` 77+ | A development 60-slot scheme, NOT automatically an embedding or kNN token format |
| `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs@2957816a88aa9cdcd3020bb81be56b5dfff165e0` | `Search(query,_k)` 223 then `hits[0]` 228 | This consumer selects top1 output; multi-neighbor improvements belong to RP02, not RP18 |
| `18.CapabilityTraining/LlmIngredientLogicalUnits.cs@cb8744fcbbcff99821852e789dc92163976c0ad7` | `TransformerEmbeddingKnnLogicalUnit.ExecuteAsync` 169+ | Another existing runtime embedding/retrieval route; decide *which* actual ordinary path is used |
| `11.DPO/DpoTrainer.cs@9c358978075db8c22fe0125eaea9e89e831d5635` | DPO policy/reference ratio documentation and Step | Trainer exists; RP18 must not invent its own loss or claim actual Student utility |

**Evidence depth:** these are explicitly limited static code inspections. No executed teacher, Student, index rebuild, PC measurement or cold ordinary consumer was observed in this RP18 authoring pass. File-size/tokenization/trainer observations from R18/FINDINGS are inherited decisions and require a fresh method check before implementation.

## Mandatory planning references by meaning

- `R/previous rounds thinking results.md` (avoid slide-4-only collapse), `R/PARALLEL-R01-R03-JOIN.md`, **live** `R/PARALLEL-R01-R25-CONTRACT.md`, `R/R01-R25-TO-CODE-SLICES-STRATEGY.md`.
- `R/results/R18/{FINDINGS,DECISIONS,SOURCES,CHANGE-MAP,RUN-LOG}`; `R/R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md` v2 — first Student lesson authority, not RP18 work.
- `R/results/R04,R05,R06,R07,R08,R13,R17,R21,R22,R23,R24,R25` **per relevant domain/decision**; do not reread all 25 wholesale or overwrite results.
- D/`09.09.26WorkLeft-RAG-00-usage-guide.md`, `RAG-01-dpo-legal-build.md`, `RAG-02-design-to-code.md` plus their **corresponding NDJSON**; `RESEARCH-01-training-quality-speed-accuracy-program.md`, `MECH-01-mechanism-validation.md`, `TASK-PROMPTS-TRAINING.md`, `02-teaching-chinese-model.md`, `REPORT-01-progress.md`, and `INTEGRATION-01-training-into-work-program.md`, where relevant.
- K/`docs/skills`: `architect-behavior-classifier-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `plan-self-validation-SKILL.md`, `code-examination-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `improvement-measurement-SKILL.md`, `learning-loop-closure-SKILL.md`, plus task-specific DPO/domain/alignment and QA review skills. Record **actually read** lines/blobs, not merely named skills. Use current K/`AGENTS.md` and `CLAUDE.md`, not a pasted historical fragment.

## Sole owners, with minimal handoffs

RP18 writes only a proven **representation compatibility/transposition seam** and its own receipts, when authorized. RP02 owns domain mapper/native codebook and adaptive kNN. RP01/R18 own DPO Student training/masks/LoRA optimizer; RP03 typed Unit/Flow; RP04 Orch/Graph; RP05/RP10 selected-state journal/promotion; RP09 scientific runner. RP14 law revisions; RP11/12/13 attribution and policy; RP06 host. **One writer for any shared K file**, others return owner-targeted proposals. Historical S00–S10 and Rxx are never “completed” by a new folder.
