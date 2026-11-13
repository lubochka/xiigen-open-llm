# Layer 0-C — Knowledge Edge Pairs (026)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-026-001 — "run through" knowledge edge

"run through [topic] with [team]" → gap (walkthrough/explanation = teaching).
"run through [checklist/steps]" with deploy/configure action → deploy/configure fires; "run through" = gap warning.
"run through" without code action following → gap (gap-skills-guidance-mentor-001).

Note: "run through" is closer to "walk-through" than to "run [tests]". "run [tests]" = TEST fires. "run through [topic]" = teaching → gap.

---

## KE-026-002 — "review with" knowledge edge: three distinct meanings

Meaning 1: "review [topic] with [people]" = collaborative discussion → gap.
Meaning 2: "review [code/system] for [issues]" = audit action → SECURITY or TEST depending on focus.
Meaning 3: "review and [action verb]" = gap warning + action fires.

Distinguishing test: Does "with [person]" follow? → meaning 1 → gap.
Does "for [problem type]" follow? → meaning 2 → SECURITY/TEST.
Does "and [code verb]" follow? → meaning 3 → gap warning + predicate.

---

## KE-026-003 — "present" knowledge edge

"present [anything] to [audience]" → gap always (presentation = communication = teaching act).
Topic of presentation does not change routing: present architecture = gap; present security findings = gap; present test results = gap.
"present and then [code verb]" → gap warning + code predicate fires.

No exceptions: "present" in guidance context always → gap-skills-guidance-train-001.

---

## KE-026-004 — "discuss" knowledge edge

"discuss [topic]" → gap (dialogue = knowledge-sharing = teaching-adjacent).
"discuss [topic] with [team]" → gap.
"discuss and then [code verb]" → gap warning + code predicate.
Topic irrelevant: "discuss security" = gap; "discuss architecture" = gap; "discuss tests" = gap.

Distinction from "explain": both are gap, both use same rule. "discuss" = two-way; "explain" = one-way. Both → gap. DomainId: software-development for both.

---

## KE-026-005 — "show" vs "demonstrate" boundary

"show how to [verb]" = "demonstrate" equivalent → gap-skills-guidance-train-001.
"show the team [existing artifact]" = demonstration → gap-skills-guidance-train-001.
"show [results]" in pure code/output context (not human-directed) → no gap.
"demonstrate [process] to [audience]" → gap-skills-guidance-train-001.
Both verbs share the same gap ID. Neither fires any code predicate.
