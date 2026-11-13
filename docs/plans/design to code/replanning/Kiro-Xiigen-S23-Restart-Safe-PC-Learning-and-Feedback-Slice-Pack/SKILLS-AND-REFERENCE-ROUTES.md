# S23 skill loading and truthful role separation

K/docs/skills under codex/dpo-implementation is the active skill library; filenames below were verified in the current branch directory. Every worker reads its actual selected skill CONTENT, not just its name. Agent/CLAUDE constraints and SESSION-START-PROMPT are resolved anew. A task packet is independently dispatchable and must not assume another worker loaded a skill.

Common SK-SESSION/INTENT/PLAN/REVIEW/HUMAN/PROMPT:
planning-session-startup-SKILL.md; architect-behavior-classifier-SKILL.md; authority-requirement-binding-SKILL.md; reference-inventory-and-source-map-SKILL.md; phase-planning-SKILL.md; plan-review-SKILL.md; claim-verification-SKILL.md; goal-delivery-completeness-SKILL.md; reconnaissance-gate-SKILL.md; specificity-calibration-SKILL.md; output-skepticism-SKILL.md; trainable-architecture-decision-SKILL.md; human-readable-architecture-audit-SKILL.md; conversation-intent-and-self-reflection-SKILL.md; prompt-patch-authoring-SKILL.md; output-readability-gate-SKILL.md.

Code / recovery tasks additionally: code-examination-SKILL.md; data-flow-integrity-SKILL.md; implementation-doc-code-refs-SKILL.md; verification-before-completion-SKILL.md.
Learning experiments additionally: experiment-design-SKILL.md; capability-measurement-SKILL.md; improvement-measurement-SKILL.md; learning-loop-closure-SKILL.md; learning-path-audit-SKILL.md.
Feedback and DPO additionally: dpo-triple-format-SKILL.md; design-conversion-dpo-authoring-SKILL.md; design-conversion-negative-dpo-quality-GUIDE.md; learning-signal-capture-SKILL.md; client-adaptive-learning-loop-SKILL.md; training-data-gap-audit-SKILL.md.

Recommended sequence for each bounded worker: load minimum common stack and 2–5 task-specific skills; find primary through relevant WorkLeft/RAG route; inspect entire affected class/callers/consumer before code mutation; report skill actual-read receipt with SHA, missing skills or changed routes. Where long skills require more reading, state READ_SCOPED vs READ_FULL. Do not claim 194 skills loaded; do not copy all skills into every worker context. No child may claim an actual CODEX Astra or external Chinese invocation unless a separate real run receipt exists.
