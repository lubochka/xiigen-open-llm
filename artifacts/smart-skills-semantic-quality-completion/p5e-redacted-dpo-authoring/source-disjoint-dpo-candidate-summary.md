# Source-disjoint DPO candidate summary

Это summary для parent review будущего source-disjoint split. Оно не разрешает training или eval.

## Counts by target machine

- PromptToTask: 2
- OrchestrationReasoning: 5

## Counts by source family

- `sf-prompt-to-task-basic-prompt-77A175A9`: 1
- `sf-orchestration-phase-transition-8CD45F23`: 1
- `sf-p5a-prompt-to-task-authority-rules-54DF62A3`: 1
- `sf-p5a-both-skills-inventory-905F88B4`: 1
- `sf-p5a-orch-universal-orchestrator-plan-C10AD9A5`: 1
- `sf-p5a-orch-dependency-relations-7DB3DAB0`: 1
- `sf-p5a-orch-flow-isolation-F85D7BAA`: 1

## Possible future grouping

- PromptToTask: one possible train candidate and one possible eval candidate, but parent may require more independent families.
- OrchestrationReasoning: three possible train candidates and two possible eval candidates, all still parent-review gated.

## Same-source and near-duplicate warnings

- Phase-boundary row and multi-branch row both teach review-before-next-step variants; future split must keep them source-disjoint and check near-duplicate wording.
- Core-vs-adapter row has both-target origin; this P5E slice materializes only the OrchestrationReasoning variant from P5D, not a duplicate PromptToTask row.
- Dependency-reasoning and flow-isolation rows come from code/test evidence families; chosen/rejected text must remain stack-free during parent review.

## Boundaries

- training_allowed=false
- eval_allowed=false
- training_ready_rows=0
- semantic_quality_claimed=false
