# S22 source → RAG → primary → current code → consumer map
**Directly scoped planning evidence; NOT a full read of every primary file or a full K audit.** Every future worker repeats needed reads on current heads.

| Class | Location / current route | What was actually established or must be checked |
|---|---|---|
| P origin | D/`original task.md`; `task prompt.md`; `goal of the round.md`; `Алгоритм создания ДПО.md` | Original four roles/parallel combinations, teaching and DTC/IDE scope; initial spans inspected, NOT entire historical corpus |
| P WorkLeft | D/`09.09.26WorkLeft-02-teaching-chinese-model.md`, `09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, `09.09.26WorkLeft-REPORT-01-progress.md` | Exact teacher roles, one-pair/no-repeat, honest not-yet-learned status; first two opening portions inspected |
| P RAG | D/`09.09.26WorkLeft-RAG-MASTER-INDEX.md` → `RAG-00-usage-guide.md` → `RAG-01/02/03/04.md` + NDJSON → actual G/F/V primary | Master and RAG-02 opening inspected; indexes DO NOT prove primary was read. Executor must traverse selected exact row to original source |
| F locked | P/`llm mvp/llm dynamic flow/DECISIONS-LOCKED.md` | **Directly read** D-04, D-05, D-06, D-07, D-10–D-15. D-04 labels are intent; no ad-hoc override of D-14 idle or D-15 arbiter |
| R history | R/`previous rounds thinking results.md` opening and R07 correction; R01–R25 own findings; `PARALLEL-R01-R03-JOIN.md` | Read opening; R07=S04-only failure; R10 joins 9 independent early findings. Research documentation is baseline, not execution |
| R applied | R/`results/R21/FINDINGS.md`, `DOMAIN-EXPERIMENT-RECIPES.md`, `R22-CAUSAL-EXPERIMENT-CATALOG.md`, `results/R22/FINDINGS.md` | Source-family split, C00–C11, positive/negative/unknown; measured claims are still NOT_MEASURED |
| K intent | K/`21.AgentInfrastructure/Logical/PromptDomainClassifier.cs` | **Directly inspected initial classifier code:** D-04 `SemanticCompilerAdapter.DomainClassifierUnit`, metadata confidence/label; not proof of subject OOD |
| K retrieval | K/`21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` | **Scoped initial code inspected** including `Train` and parameters; full search/use currently needs S22.00A/.01C fresh method trace |
| K logical | K/`00.Core/LogicalContracts.cs`, `LearningMachineContracts.cs` | **Scoped code directly inspected**: `ILogicalUnit<>`, `LogicalUnitRequest`, `LogicalUnitResult`, training Batch and Machine authority |
| K domain graph | K/`17.DynamicDomains/DynamicDomainGraph.cs` | **Directly inspected** topology closure/expander, not open-domain taxonomy/admission |
| K unresolved | K/`21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`; 10.LoRA; 11.DPO; 18.CapabilityTraining; 06.VectorSearch; actual DTC/IDE entry | Paths as **tracing targets** only; not claimed read here. Workers must inspect complete actual changed file/consumers |
| K skills | K/`docs/skills` (194 entries at source listing) | **Scoped front sections** read from architecture, planning, plan review, code examination, prompt, human, DPO, experiment and context skills; neither all 194 nor full listed skills were loaded. Workers read needed exact bodies |

### Existing code and owner boundaries
P=`lubochka/xiigen-planning:llm_plan3`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. During preparation K HEAD was observed `d81a2f29a6f3a13e591567ae0653d1878a996472`, P `8ff9db877faec4e1ec99fe9f8e7ebc7a839e182c` **as observation, not pinned forever**. Current task must refresh heads and file blobs.

Open-world admission is not entitled to rewrite F decisions. Map known, missing, unsupported and ambiguous subject law to safe response. A DomainGraph transition is not automatically a domain registry. A D-04 learned/seeded label is not a Go/C#/SQL subject-domain proof. Reuse RP01/02/03/04/05/06/07/09/10/11–14/18 owners; record S17/S19 boundaries; no duplicate model/learner.

### Source-read accounting required of each worker
`[{repo,ref,path,blob_sha,lines_or_span,READ_FULL|READ_SCOPED|INDEX_ONLY|NOT_READ,claim_supported,limitation,consumer}]`. If source is missing, say MISSING and hand off one exact resolver. Do not mark whole primary traversed from MD or NDJSON index; no stale result revision/status inflation; do not write `results/Rxx/*` in S22.
