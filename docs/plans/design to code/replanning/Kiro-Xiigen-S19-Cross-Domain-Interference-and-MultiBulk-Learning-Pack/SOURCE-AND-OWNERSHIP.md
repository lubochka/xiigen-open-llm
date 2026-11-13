# S19 source routes, evidence depth and ownership
## Roots and branch identity
P = lubochka/xiigen-planning, branch llm_plan3; D = P/llm mvp/design to code session; R = D/replanning; G = P/llm mvp/dpo legal build; F = P/llm mvp/llm dynamic flow; V = P/llm mvp/visual studio & visual studio code continue; K = lubochka/llm_mvp_core, branch codex/dpo-implementation.
This plan was drafted against a moving P branch (head changed during preparation) and directly verified K ref **3c7f5f8c7809936e25579603c96ea6df8598fc40**. All future executor source spans/SHAs MUST be refreshed. A directory scan is INDEX_ONLY, not full RAG traversal.

## Mandatory scoped descent for every executed task
1. Latest user's live request; D/original task.md, task prompt.md, goal of the round.md, Алгоритм создания ДПО.md; preserve DTC/IDE and four roles and old decisions, not S04-only shortcuts.
2. D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → D/09.09.26WorkLeft-RAG-00-usage-guide.md → applicable D/RAG-01-dpo-legal-build, RAG-02-design-to-code, RAG-03-merge-and-branches, RAG-04-vs-vscode-continue MD + NDJSON **then actual referenced G/F/V primary**. Record visited paths and aliases; an index row is not SOURCE_TRACED.
3. D/09.09.26WorkLeft-02-teaching-chinese-model.md, -MECH-01-mechanism-validation.md, -RESEARCH-03-dpo-algorithm-conformance.md, -RESEARCH-01-training-quality-speed-accuracy-program.md, -PLAN-HUMAN-01.md, -PROMPTS.md, -03-decisions-register.md plus applicable training work orders.
4. R/previous rounds thinking results.md, PARALLEL-R01-R03-JOIN.md + current PARALLEL-R01-R25-CONTRACT.md, R01-R25-TO-CODE-SLICES-STRATEGY.md and selected Rxx FINDINGS/DECISIONS/SOURCES/RUN-LOG rather than treating R01–R25 as globally completed.
5. F/DECISIONS-LOCKED.md (especially D-04, D-05, D-06, D-07, D-08, D-09, D-10, D-15), ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md, DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md; retrieve actual law context and authorization.
6. K/AGENTS.md, K/CLAUDE.md, current required docs/skills; full affected K code file, callers, actual ordinary consumer, state restore, failure paths, intended output. Do NOT rely on line ranges copied from old work orders.

## Source observations from direct scoped reads (not broad audit)
- K/21.AgentInfrastructure/Logical/PromptDomainClassifier.cs D-04 seeded CODE_Q etc classifiers — not generic subject-domain law mapping.
- K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs ExecuteAsync reads multiple search hits yet best=hits[0] to choose output; no claim that RP02 patch is implemented in current K.
- K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs BuildBatch admits raw pair then calls Compact on prompt/chosen/rejected before Encode; existing DPO source may lose exact pair semantics.
- K/11.DPO/DpoTrainer.cs has policy/reference and mutation gate; K/10.LoRA/LoraApplicator.cs has LoRA application contracts. No evidence from these reads alone of domain-switch correctness, beneficial gradient, state adoption, or code changes.
- F/DECISIONS-LOCKED includes per-project adapter and 70/30 replay, currently treated as **source decisions**, not verified new experiments.
- S17 README claims a 14-task manifest, but S17 remote directory inspected at this authoring time listed nine high-level files **without tasks/ or PACKAGE-MANIFEST.json**. Do not inherit an unverified pass or copy it as S19's package format.
- S18 was not seen in the enumerated R directory at authoring time; S19 does NOT assert S18 completion or reserve its unknown work.

## Owner/evidence routing
RP01 first exact Student lesson; RP02 semantic domain/subject tokens + multi-hit kNN; RP03 Unit/Flow; RP04 Orchestrator/Graph; RP05 feedback/replay/transaction; RP06 DTC/IDE host; RP07 Builder; RP08 transfer; RP09 independent runner; RP10 selected-state PC promotion; RP11 credit; RP12 repair attribution; RP13 policy; RP14 and S17 version drift. This S19 only owns cross-domain collision selection seam **if** actual consumer evidence shows a gap. Shared files demand one writer and fresh HEAD; ownership arbitration before write.

## Source/evidence grade for handoff
READ_SCOPED: selected D prompts, current F locked decisions, R previous rounds early section, R19/R25 findings, R18 implementation order, selected RP07–RP14 and S17 high-level docs, selected K methods, skill starts. INDEX_ONLY: all enumerated R and K/docs/skills entries. NOT_READ_FULL: many WorkLeft 100KB–1MB files and original G/F/V primary documents and large skill bodies. Executor must close only the task-relevant gaps; no false full-source audit claim.
