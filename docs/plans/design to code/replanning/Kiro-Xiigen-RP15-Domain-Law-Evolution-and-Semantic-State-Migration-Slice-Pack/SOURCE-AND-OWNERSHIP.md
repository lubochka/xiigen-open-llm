# RP15 — primary source routes, evidence depth and ownership

**P:** lubochka/xiigen-planning @ llm_plan3; D = P/llm mvp/design to code session; R = D/replanning; **K:** lubochka/llm_mvp_core @ codex/dpo-implementation; **G:** P/llm mvp/dpo legal build; **F:** P/llm mvp/llm dynamic flow; **V:** P/llm mvp/visual studio & visual studio code continue.

## Required entire route, not one S04 or one RAG listing
1. Planning live **llm_plan3** D/original task.md, D/task prompt.md, D/Алгоритм создания ДПО.md, D/Изменения в плане работы - цель.md, D/goal of the round.md, D/XIIGEN-human-slice-map-2026-09-24.md, D/09.09.26WorkLeft-02-teaching-chinese-model.md, -MECH-01-mechanism-validation.md, -RESEARCH-03-dpo-algorithm-conformance.md, all additional relevant WorkLeft* and original S00–S10 scope.
2. D/09.09.26WorkLeft-RAG-00-usage-guide.md and D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → named Markdown/NDJSON row → actual linked PRIMARY file in G **llm mvp/dpo legal build**, F **llm mvp/llm dynamic flow**, V **llm mvp/visual studio & visual studio code continue**. Record INDEX_ONLY when primary not visited.
3. Replanning previous rounds thinking results.md, PARALLEL-R01-R03-JOIN.md, R01-R25-TO-CODE-SLICES-STRATEGY.md, results/R01–R25 relevant FINDINGS/SOURCES/DECISIONS/CHANGE-MAP/checkpoints, R21 domain curriculum, R22 causal contrasts, R23 PC lifecycle, R24 ordinary acceptance, R25 review; published RP01–RP12 owner packs.
4. Core **codex/dpo-implementation** K/AGENTS.md, CLAUDE.md, docs/skills selected for the task, actual K/00.Core/LearningMachineContracts.cs, DynamicGraphContracts.cs, LogicalContracts.cs, K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs, K/11.DPO/DpoTrainer.cs, K/14.ContinuousLearning/Training/DpoTrainingExecution.cs, K/10.LoRA/LoraLinear.cs and discovered tokenizer/index/state/host callers. Before edits refresh HEAD/blob and inspect full method + caller/consumer.

Selected primary material sampled during RP15 design, NOT full audit: D/original task.md (contains four roles), D/09.09.26WorkLeft-02-teaching-chinese-model.md (current CODEX/Chinese split), G/active-codex-execution-04-dpo-legal-model-policy.md (historical legal overlays require current authorization), F/DECISIONS-LOCKED.md and F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md, V/LeftToWork-CONTINUE-M2-portmap.md; R/results/R21/FINDINGS.md (unchanged shape can hide changed projection), R/results/R22/FINDINGS.md (N/C0/U + same corrected code); R/R01-R25-TO-CODE-SLICES-STRATEGY.md and previous rounds thinking results.md. The G/F/V roots, D's 09.09.26WorkLeft* family, original S00–S10 and results/R01–R25 were enumerated **as directory/source routes**; enumeration is NOT reading all primary files.

Sampled K actual paths/observed blob SHAs at design time (must refresh before edit):
- 00.Core/LearningMachineContracts.cs: 7f605d49762e3fc0c1b4d4ca490596bc9668cdd5 — ILearningMachine owns Train; no actual training inferred.
- 00.Core/DynamicGraphContracts.cs: 987c29396fab838db0eecd8060837d5e5e33bce4 — typed graph candidates/relations; infer no real trained Graph state without caller.
- 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs: 2957816a88aa9cdcd3020bb81be56b5dfff165e0 — training example + model manifest; actual kNN consumer requires deeper trace.
- 14.ContinuousLearning/Training/DpoTrainingExecution.cs: ab5bb45ca15a86a206952f2516a32119c67efde9 — Student update candidate path.
- 10.LoRA/LoraLinear.cs: 1b321e99408c961b896896983d002e3b26425b85 — low-rank adapter, NOT a semantic tokenizer.
Current planning snapshot core branch HEAD observed: 3c7f5f8c7809936e25579603c96ea6df8598fc40. Never use these as future replacement for fresh reads.

## Owner boundaries
| Previous owner | Preserved ownership | RP15 minimal delta / integration |
|---|---|---|
| RP01 | exact Chinese-authored DPO pair, Student LoRA objective, actual first lesson | only compatibility handoff; no replacement trainer |
| RP02 | domain mapping, law-aware semantic tokens/vectors, adaptive real multi-neighbor kNN | version-change impact + compatibility decision; codec/retrieval implementation stays RP02 |
| RP03 | typed Unit many→many, learned Flow correction | identify invalid typed associations; training modification stays RP03 |
| RP04 | async Orchestrator, higher-order Graph, final lawful policy | invalidate/reuse affected motifs, never rewrite controller |
| RP05 | feedback, checkpoint/rollback, continual training and PC state | feed effect plan to its existing atomic owner |
| RP06 | DTC/IDE host/release | consume or instrument real ordinary host; no new transport |
| RP07 | Builder meta-learning | law-change candidates may provide lawful Builder feedback, no teacher substitution |
| RP08 | cross-domain transfer and adoption | targeted drift contrast, not reimplement transfer |
| RP09 | independent examination runner and sealed panels | add law-evolution scenarios without cloned evaluator |
| RP10 | PC pilot and accepted state pointer | reuse atomic promotion and cold read |
| RP11 | hybrid causal credit and cost-aware route allocation | consume credit, do not implement alternate scorer |
| RP12 | causal fault selection and smallest intervention | consume its attribution; RP15 owns state/schema compatibility seam |

**Proof row required for each task:** claim ID | root | branch HEAD+blob | actual file/section/span | provenance and authority | direct read depth FULL_FILE/SCOPED/INDEX_ONLY | upstream producer | actual normal downstream consumer | one falsifier | owner | next task. Historical output/status or source indexing alone is insufficient.

**Important source authority:** unquoted current user instruction governs requested pack. Historical G/F decisions may be locked for product until owner approval, but their earlier 'DPO last' implementation ordering must not be silently treated as the current learning-first priority. Legal/provider terms, consent and explicit runtime grants are still binding and must never be inferred from planning publication.

**No evidence inflation:** task descriptions, test ideas, prior results and this plan do not prove that model updates, migrations, Chinese pair generation or independent Astra reviews occurred.
