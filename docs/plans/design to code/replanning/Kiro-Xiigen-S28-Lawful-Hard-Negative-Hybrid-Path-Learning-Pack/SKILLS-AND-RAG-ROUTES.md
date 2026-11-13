# S28 — role-specific skill stack and model context

Each task is standalone: read its own `tasks/S28.XX.md` plus primary sources and **actual skill content**, not just names. Do not inject 193 unrelated skills into every context. The verified K/docs/skills files include:
- **Architecture:** `trainable-architecture-decision-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `service-boundary-design-SKILL.md`, `architectural-decision-testing-SKILL.md`.
- **Intent, planning, review:** `conversation-intent-and-self-reflection-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `intent-to-plan-SKILL.md`, `authority-requirement-binding-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `plan-restructure-SKILL.md`, `claim-verification-SKILL.md`, `output-skepticism-SKILL.md`.
- **Code and architecture verification:** `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `verification-before-completion-SKILL.md`.
- **Prompt & human communication:** `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `goal-delivery-completeness-SKILL.md`.
- **Learning:** `experiment-design-SKILL.md`, `improvement-measurement-SKILL.md`, `learning-loop-closure-SKILL.md`, `dpo-triple-format-SKILL.md`, `design-conversion-negative-dpo-quality-GUIDE.md`, `learning-path-audit-SKILL.md`, `graph-backed-routing-SKILL.md`.
A task must state which of these it ACTUALLY read and what version; unavailable skill is a recorded unknown, not claimed loaded. RAG->primary: D/WorkLeft RAG-MASTER->RAG-00->relevant RAG-01 DPO / RAG-02 DTC / RAG-03 branches / RAG-04 VS/IDE MD+NDJSON->actual G/F/V documents; do not stop at index.

## Roles / independent owners
Architect traces current law/code and proposes one change; model-ready executor applies only specifically authorized bounded effect; different plan/code reviewer validates exact candidate; separate independent domain scorer judges blinded outputs; CODEX on GPT-6 Astra 6 UltraCode selects actual Student DPO case and **separately** judges exact final Chinese pair only when actually invoked; approved external open Chinese Teacher authors BOTH final Student responses, never CODEX/user edit under false authorship. This S28 pack only DESIGNS those tasks; no subagent, CODEX, Chinese teacher or Astra review is implicitly invoked.

## Efficiency
Repin P/K once per bounded dispatch; source excerpt budget by causal question, not all files by default. Cache already verified exact SHA within the SAME task; reread changed blobs, actual callers and new primary. Require a single new source/algorithm intervention per worker, shared-writer serialization, output size budget and one human-readable Russian decision + precise open gaps.
