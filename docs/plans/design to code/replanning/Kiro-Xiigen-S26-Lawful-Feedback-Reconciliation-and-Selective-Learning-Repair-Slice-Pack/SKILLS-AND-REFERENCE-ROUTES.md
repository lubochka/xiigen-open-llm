# S26 exact skills: load content, not filename alone
For every task re-read P/D/replanning AGENTS, K/AGENTS and CLAUDE when applicable, full [START](START-PROMPT.md) and one complete task. At K `docs/skills/` (branch `codex/dpo-implementation`) load:

- **SK-SESSION / SK-INTENT**: `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`.
- **SK-PLAN**: `authority-requirement-binding-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-self-validation-SKILL.md`.
- **SK-REVIEW/CODE**: `plan-review-SKILL.md`, `claim-verification-SKILL.md`, `goal-delivery-completeness-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `verification-before-completion-SKILL.md`.
- **SK-ARCH**: `trainable-architecture-decision-SKILL.md`, `no-product-decisions-SKILL.md`, `human-readable-architecture-audit-SKILL.md`.
- **SK-PROMPT/COMMUNICATION**: `prompt-patch-authoring-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`, `output-readability-gate-SKILL.md`.
- **SK-DPO/EXAM**: `dpo-triple-format-SKILL.md`, `design-conversion-dpo-authoring-SKILL.md`, `experiment-design-SKILL.md`, `improvement-measurement-SKILL.md`, `learning-loop-closure-SKILL.md`.
- Task-specific exact two skill names appear in EACH S26.xx card. Do not assume a GUIDE filename ending exists; discover actual K tree and report `SKILL_UNAVAILABLE` honestly.

**Source navigation and RAG:** original→WorkLeft relevant prompts/MECH/RESEARCH-03/model roles→RAG MASTER+00→relevant RAG-01/02/04 MD/NDJSON→ACTUAL P/G/F/V primary document identified by `source.file`→actual K complete file/interface/callers/consumers. Read [SOURCE-AND-OWNERSHIP](SOURCE-AND-OWNERSHIP.md) for links. R01–R25 findings are attributed secondary unless independently opened and provenance depth recorded.

**Task packet compatibility**: CURRENT_TASK, NOT_THE_TASK, EVIDENCE_ONLY, selected skills, current inputs/ref SHAs, direct RAG and primary, protected requirement IDs, typed target algorithm, GOOD/BAD/EDGE/falsifier, step-by-step, subtask A/B return-only, permitted/forbidden effects, independent examination and output schema are mandatory in each card. Historical required 30/50 passes/old model names are not silently imported as 30 independent current S26 invocations.
