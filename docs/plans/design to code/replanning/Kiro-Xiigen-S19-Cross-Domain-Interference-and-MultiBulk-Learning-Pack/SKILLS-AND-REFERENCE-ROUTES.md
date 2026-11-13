# S19 skills, context packaging and RAG retrieval
## Required skill library (K/docs/skills, current ref at execution)
SK-INTENT / architecture: architect-behavior-classifier-SKILL.md; trainable-architecture-decision-SKILL.md; human-readable-architecture-audit-SKILL.md.
SK-PLAN: planning-session-startup-SKILL.md; phase-planning-SKILL.md; reference-inventory-and-source-map-SKILL.md; task-slice-golden-rule-SKILL.md; plan-restructure-SKILL.md.
SK-REVIEW: plan-review-SKILL.md; plan-self-validation-SKILL.md; code-examination-SKILL.md; generated-code-review-SKILL.md; output-skepticism-SKILL.md.
SK-PROMPT: prompt-patch-authoring-SKILL.md; context-package-design-SKILL.md; specificity-calibration-SKILL.md.
SK-HUMAN: conversation-intent-and-self-reflection-SKILL.md; output-readability-gate-SKILL.md; no-product-decisions-SKILL.md.
SK-LEARN: experiment-design-SKILL.md; dpo-triple-format-SKILL.md; design-conversion-negative-dpo-quality-GUIDE.md; dcm-dpo-construction-GUIDE.md; learning-loop-closure-SKILL.md; learning-signal-capture-SKILL.md.
If changed skill names are absent in current K, resolve honestly and record OPEN, never fake a loaded skill. This author directly read scoped portions of relevant skills; did not load all 194 skill bodies.

## Every future worker's minimal route
- Load D/AGENTS.md and R/AGENTS.md as applicable; K/AGENTS.md and K/CLAUDE.md for product code work; latest live instruction overrides stale historical task text.
- Read only the **task-specific** source row and relevant RAG primary, not a 1MB all-history prompt. Declare READ_FULL / READ_SCOPED / INDEX_ONLY / NOT_READ by path.
- RAG master → RAG-00 guidance → RAG-01/02/03/04 selected row → real G/F/V primary → current K code caller→owner→consumer; document unresolved locators.
- Load SK-INTENT, SK-PLAN, SK-REVIEW, SK-ARCH, SK-PROMPT, SK-HUMAN. For code, SK-CODE; for DPO, SK-LEARN; for async Graph, relevant Graph skills. Load full necessary sections yourself; parent's read is not sub-agent evidence.
- Declare CURRENT TASK, NOT THE TASK, allowed/forbidden actions, reference SHAs, good/bad/edge, exact stop/falsifier, return schema. A prompt missing these is NEEDS_FIX.

## Authority and novelty
Previous R01–R25 findings and WorkLeft are baseline, not automatically true runtime. D-04 intent labels are not subject laws. F/DECISIONS-LOCKED has **locked** D-05/D-06/D-07/D-15; plan experiments cannot silently amend them. S17/RP14 own law-version drift, RP01–RP13 own existing respective mechanisms. Store questions beyond the owned S19 selection seam as recipient-specific integration cards, not scope creep.
