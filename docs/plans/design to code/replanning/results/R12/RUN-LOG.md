# R12 RUN-LOG — R12-ARCH-2 current publication, 13.10.2026

## User-authorized task and hard bounds

Review and repair R12 of the full 25-run learning-first replanning. Publish to GitHub llm_plan3/replanning directly. Cover six A–F architecture subjects, not only S04/DPO. Read primary original/WorkLeft/RAG/flow decisions, current R15/R16 and targeted core code. Retain all four learning roles, domain-specific token/vector/dynamic-kNN, LoRA/projector, compact PC self-improvement and Chinese-both-sides/CODEX-Astra selection+judgment. R01–R16 parallel research and existing R16 publication must not be overwritten.

**Current result:** R12 A–F INTEGRATED DESIGN CANDIDATE / E1 CONDITIONAL / E2 PREDECLARED FALLBACK / EXACT IMPLEMENTATION NOT READY / LEARNING NOT MEASURED / INDEPENDENT ASTRA REVIEW NOT PERFORMED. Old R12-CP1 selected-fast-path wording and R12-REPAIR-1 REEXECUTION_REQUIRED are historical checkpoints; this execution addresses their documentary A–F gap but does not erase code and measurement gaps.

## Actual work in this invocation

1. Read current AGENTS.md, COMMON-CONTRACT, run-prompts/COMMON and R12, full R12-REPAIR-WORK-ORDER and five existing R12 carriers; checked R15 and R16 review/status, R13 STATUS OVERRIDE, R05/R06/R07 findings and the parallel join/source routes.
2. Inspected GitHub trees for whole llm mvp, design-to-code session (including WorkLeft and S00–S10 packs), dynamic-flow and replanning directories. Directory listing = STRUCTURE_READ, not contents of every tree branch.
3. Direct source reads: original task/task prompt/goal, planning of testing, original DPO algorithm (relevant spans incl. historical chosen fallback), WorkLeft-02/INTEGRATION-01/RESEARCH-03/PLAN-HUMAN-01/DESIGN-ACTIONS/MECH targeted, RAG-01/usage targeted, dynamic-flow DECISIONS-LOCKED and arbiter/KNN amendment; S04 RUN §§2.1–2.3 targeted; previous rounds beginning. Current R16 WorkLeft role updates take precedence in scoped conflicts. See SOURCES.csv for limits.
4. Read target sections of current core skills: trainable architecture, plan, plan-review, code examination/review, prompt patch, source map and human-readable audit. Some oversized skill files were **targeted sections, not full skill-file load**. No separate reviewer/subagent was invoked.
5. Direct code read in core codex/dpo-implementation: LogicalContracts; LlmIngredientLogicalUnits BuildBatch/Compact/ToMask; DpoTrainer StepVector and ComputeLogProbs; KnnLogicalUnit Train/Execute; DomainClassifierUnit; LoraLinear; TrainableRoutingFlow; CodeDocumentationRoutingFlow; checkpoint start boundary; DynamicGraphMachine; edge authority execution and selected-target executor. Read depths are bounded; absence of a traced caller is NOT global nonexistence.
6. Synthesized and published R12-A authority matrix, B four causal contracts, C immutable DPO state machine/objective, D domain-parametric representation/rerank/k, E LoRA/projector/PC, F three walkthroughs, E1–E4 pre-comparison and explicit consumer handoffs in FINDINGS/DECISIONS/SOURCES/CHANGE-MAP/RUN-LOG.

## Code observations versus remaining assumptions

- **CODE_TRACED:** 18 BuildBatch creates raw admission then Compact collapses whitespace and 480-char trims; masks prompt+answer. 11 DpoTrainer has a real policy/reference optimizer but current log-probs/backward are full-sequence mean. 21 KnnLogicalUnit stores k hits but uses first as answer. DynamicGraphMachine declares training not activated.
- **PARTIAL CODE TRACE:** Training/checkpoint start and normal CodeDocumentation routing are separate discovered paths; no fresh proof that ordinary generation uses promoted policy. Edge/L3 execution exists, but actual trained graph checkpoint→preload and outcome not established.
- **DESIGN ONLY:** learned domain-aware reranker/k, law-preserving vector projector, full Unit many-association bridge, correction/path policies, higher-order Graph training; optimum formula/k/PC fit and bulk gains NOT_MEASURED.
- **REVIEW:** R15 parent NEEDS_FIX is documented, separate CODEX GPT-6 Astra UltraCode independent review NOT_PERFORMED. This author has not impersonated CODEX or a Chinese generator.

## Publication and verification boundary

This R12 invocation changes **only its five result carriers**. Publication must be a non-destructive, expected-head GitHub commit; then each remote file and branch SHA are read back. Record publication SHA in the external completion report, because the RUN-LOG content is itself part of the same commit. Do not claim a GitHub push before tool confirmation.

No product tests, CI/Windows, builds, model calls, Chinese teacher DPO, CODEX lesson selection/judgment, weight/data/index mutation, training, paid generation, independence or empirical learning assessment happened. No existing tests/workflows disabled.

## Handoffs

- R13 repair: bind populated first E1 experiment card with selected policy/caller, frozen evaluation, lawful pair and resources; E2 fallback.
- R18 implement minimum exact pair/objective/consumer bridge with direct code owner review, without a second trainer and without premature release gates.
- R21/R22/R23 cover distinct domain representations, actual learned kNN/Flow/Graph, PC restart/retention and user-feedback learning.
- R15 future independent Astra re-review exact new candidate; R16 existing document corrections preserved, R19/R20 propagation based on real status.
