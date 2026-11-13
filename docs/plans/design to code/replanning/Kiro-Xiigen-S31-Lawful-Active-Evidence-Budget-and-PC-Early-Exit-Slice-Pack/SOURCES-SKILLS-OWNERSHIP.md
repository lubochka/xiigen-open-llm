# S31 source authority, WorkLeft/RAG, actual core leads and skill routing
P = lubochka/xiigen-planning branch llm_plan3; D = P/llm mvp/design to code session; R = D/replanning; G = P/llm mvp/dpo legal build; F = P/llm mvp/llm dynamic flow; V = P/llm mvp/visual studio & visual studio code continue; K = lubochka/llm_mvp_core branch codex/dpo-implementation. **At every actual task start repin HEAD and relevant source blob SHAs.**

### Mandatory source exploration, not S04-only
First inventory D recursively including original task.md, task prompt.md and .txt, goal of the round.md, Алгоритм создания ДПО.md, all WorkLeft*/LeftToWork*, research reports, old S00–S10 and new RP/S packs; all R/results/R01…R25 five carrier files, and three D/G/F/V product directions (DPO Legal, Design-to-Code, IDE). Then READ related D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → RAG-00 usage → RAG-01/02/03/04 question-matched **both .md and .ndjson** → **actual primary source** in G/F/V, not just index. Read R/previous rounds thinking results.md with authority/version, R/PARALLEL-R01-R03-JOIN.md (9 R01–R09 inputs to R10, independent R01–R20), R/R01-R25-TO-CODE-SLICES-STRATEGY.md, task-matched R/results/Rxx, owner decisions and current code. R07 may be marked REPAIR_REQUIRED; status history ≠ completed runtime evidence. Record path/ref/sha/read depth/actual relevant spans/decision rank/producer→consumer and precise gap. Missing source = NOT_READ, not DONE.

### K source leads actually inspected during S31 package authoring
| K path | Source indicates | Doesn't prove |
| --- | --- | --- |
| 00.Core/LogicalContracts.cs | ILogicalUnit and LogicalUnitResult/flags | ordinary many-to-many final usage |
| 00.Core/LearningMachineContracts.cs | ILearningMachine has training batch/Train authority | distinct learned early-stop policy exists |
| 00.Core/DynamicGraphContracts.cs | typed IQueryable GraphNodeCandidate, OutputRefs | complete OutputRefs materialization |
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs | BPE/HNSW with configured k | beyond-first-hit actual consumer |
| 18.CapabilityTraining/LlmIngredientLogicalUnits.cs | transformer embeddings/LoRA/kNN | LoRA itself equals domain token projection |
| 17.DynamicDomains/DynamicDomainGraph.cs | topology closure and expander | learned higher-order graph exists |
| 11.DPO/DpoTrainer.cs | DPO objective, optimizer/publication guard | actual Student lesson/gain |
| 25.ModelServer/ContinuousLearning/CodeDocumentationOneLessonEndpoint.cs | granted one-lesson preflight | actual optimizer applied / ordinary state |

### Owners and anti-duplication
RP01 Student DPO; RP02 semantic domain vectors/adaptive kNN; RP03 typed Unit/Flow correction; RP04 async Orchestrator/higher-order Graph; RP05/S23 safe feedback/full-PC state; RP06 DTC/IDE; RP07 Builder; RP09 independent exam; RP10 accepted state/cold user; RP11/RP12/RP13 credit/repair/where-when policy; RP16 compaction; RP18 LoRA projection. S17/RP14/RP15 law drift; S19 cross-domain interference; S20/S21 compound; S22 novel; S25 cold user identity; S26 invalid evidence repair; S27 feedback admission; S28 executed hard-negative path; S29 side-effect-safe executed shadow alternative. **S31 owns ONLY absent safe cheap partial observation→existing continue/stop ordinary consumer integration, if proved.** Otherwise REUSE_NO_CHANGE. Do not modify results/Rxx/* or reproduce other owners.

### Skills in actual K/docs/skills (not hypothetical filenames)
SK-INTENT: architect-behavior-classifier-SKILL.md; conversation-intent-and-self-reflection-SKILL.md.
SK-PLAN: phase-planning-SKILL.md; authority-requirement-binding-SKILL.md; reference-inventory-and-source-map-SKILL.md.
SK-ARCH: trainable-architecture-decision-SKILL.md; human-readable-architecture-audit-SKILL.md.
SK-REVIEW: plan-review-SKILL.md; claim-verification-SKILL.md; output-skepticism-SKILL.md.
SK-CODE: code-examination-SKILL.md; generated-code-review-SKILL.md.
SK-PROMPT/HUMAN: prompt-patch-authoring-SKILL.md; output-readability-gate-SKILL.md.
SK-SCIENCE: experiment-design-SKILL.md; improvement-measurement-SKILL.md.
SK-DPO: dpo-triple-format-SKILL.md; design-conversion-negative-dpo-quality-GUIDE.md.

Authoring inspected scoped actual content and headings, not all multi-hundred-KB skills in full. Actual executor MUST read fully task-specific relevant sections and record ACTUALLY_READ/READ_SCOPED/UNAVAILABLE; all 22 workers needn't inject full skill library. Architecture reviewer, planner, independent code reviewer, prompt engineer and human communicators are distinct role constraints; writing a child prompt is not running it. When broad work exceeds a task, child packets must independently provide goal/context/skills/sources/good/bad/edge/stop/effect permission/output; the 22 task packets do this.

### Defer with traceability, not deletion
Preserve D/G/F/V DPO Legal, Figma/Canva DTC, .NET/Angular enterprise and VS/VS Code/Continue/Codex/Claude/Cursor product requirements, under source ID + owner + when they become causal dependency. S31 is not the owner of those integrations; first local C# P/C0 does not wait for their arbitrary test gates.
