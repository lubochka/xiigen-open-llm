# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-029-001 — "acceptance criteria" always triggers predicate — never pure gap

A:
  Task: "Confirm: can 'acceptance criteria' ever produce KNOWLEDGE_EDGE?"

B:
  Answer: "No. 'acceptance criteria' always contains 'criteria' which fires DOCUMENT.
    There is no formulation of 'acceptance criteria' that avoids the DOCUMENT predicate
    because 'criteria' is positionally in the DOCUMENT lexicon."
  Demonstration:
    - "Meet the acceptance criteria." → "criteria" fires DOCUMENT → ROUTED
    - "Acceptance criteria passed." → "criteria" fires DOCUMENT → ROUTED
    - "Check acceptance criteria." → "criteria" fires DOCUMENT → ROUTED
  Rule: "acceptance criteria" is ALWAYS a DOCUMENT trigger. It cannot produce KNOWLEDGE_EDGE.

---

## B36-0C-029-002 — "user story mapping" always triggers predicate — never pure gap

A:
  Task: "Confirm: can 'user story mapping' ever produce KNOWLEDGE_EDGE?"

B:
  Answer: "No. 'user story mapping' always contains 'story' which fires DOCUMENT.
    The compound planning noun 'user story mapping' is not a special case. Lexicon matching
    is positional — 'story' fires DOCUMENT regardless of adjacent tokens."
  Avoidance: "Use 'journey mapping' or 'experience mapping' to avoid the DOCUMENT false positive."
  Rule: "user story mapping" is ALWAYS a DOCUMENT trigger. KNOWLEDGE_EDGE is not possible.

---

## B36-0C-029-003 — "kanban board" with multiple predicates — no SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Design the kanban board structure for the engineering team's workflow."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001"]
  Rule: ARCHITECTURE fired → no SCHEDULE_CAPABILITY_LEARNING. Gap warning is metadata only.

---

## B36-0C-029-004 — "WIP limits" + "approve" — no SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Approve the new WIP limits before rolling them out to all teams."
  CompiledPredicateIds: ["QA"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001"]
  Rule: QA fired → no SCHEDULE_CAPABILITY_LEARNING.
