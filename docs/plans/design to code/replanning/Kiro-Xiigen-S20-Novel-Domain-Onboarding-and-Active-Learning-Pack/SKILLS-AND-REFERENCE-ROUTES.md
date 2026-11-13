# S20 — actual skills, progressive context and subagent contract

## Skill root at execution: K/docs/skills at current codex/dpo-implementation ref
**SK-SESSION / INTENT:** planning-session-startup-SKILL.md; architect-behavior-classifier-SKILL.md; conversation-intent-and-self-reflection-SKILL.md.
**SK-ARCH:** trainable-architecture-decision-SKILL.md; human-readable-architecture-audit-SKILL.md; design-conversion-ilearningmachine-architecture-GUIDE.md; no-product-decisions-SKILL.md.
**SK-PLAN:** reference-inventory-and-source-map-SKILL.md; phase-planning-SKILL.md; task-slice-golden-rule-SKILL.md; authority-requirement-binding-SKILL.md.
**SK-REVIEW/CODE:** plan-review-SKILL.md; plan-self-validation-SKILL.md; code-examination-SKILL.md; generated-code-review-SKILL.md; output-skepticism-SKILL.md; specificity-calibration-SKILL.md.
**SK-PROMPT/HUMAN:** prompt-patch-authoring-SKILL.md; context-package-design-SKILL.md; output-readability-gate-SKILL.md.
**SK-LEARN:** experiment-design-SKILL.md; dpo-triple-format-SKILL.md; learning-signal-capture-SKILL.md; learning-loop-closure-SKILL.md; graph-backed-routing-SKILL.md.
Load actual relevant skill bodies *per task* (not metadata only), record per-file loaded sections/blob; if changed/not found SKILL_UNAVAILABLE. No claim this planning author read all 194 skills fully.

## Per-agent self-contained context packet
CURRENT TASK, NOT THE TASK, EVIDENCE-ONLY versus actual permission, SOURCE ROUTES (full URIs/refs), required_skills_and_gates, evidence READ_FULL/READ_SCOPED/INDEX_ONLY/NOT_READ, known P/K head/affected blobs, authority + hard laws, input→existing consumer→output, one owner/effect, step-by-step instructions, GOOD/BAD/EDGE, falsifier, deterministic contract checks **distinct from** blind learning exam, allowed/forbidden files/commands, return packet schema with claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed. A generic prompt to “add domains and test” fails this gate.

## RAG is a locator, not a verdict
D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → RAG-00 usage → selected RAG-01/02/03/04 *MD and NDJSON* → actual original G/F/V primary → current K full affected code, callers/downstream ordinary consumer, full before edit. D/task prompt and WorkLeft-* do not authorize execution merely because quoted historical text says “full access”. Re-read current user scope first.

## Responsibility and parallelism
Independent read-only 00A/00B/00C lanes can run simultaneously. Child investigator returns to parent **without** editing shared code; one authorized owner executor writes affected K after reconciling current branch, followed by independent reviewer/examiner who must not certify own work. No reading every 1MB history file into every worker; mandatory scoped visited-set, revisit only relevant primary on ambiguity. The parent does not call a child “executed” when only its prompt has been drafted.
