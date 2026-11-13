# Layer 0-A — Compiler Pairs (027)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-027-001 — "WIP limits" pure gap

A:
  run_id: "B36-0A-027-001"
  user_request: "Reduce the WIP limits on the in-progress column to improve flow."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-flow-efficiency-001"
  Teaching: "reduce" and "improve" are not predicate keywords. WIP (Work In Progress) limits
    and flow are gap vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-027-002 — "cycle time" pure gap

A:
  run_id: "B36-0A-027-002"
  user_request: "Track cycle time per ticket type and present findings in the retrospective."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-retrospective-001"
  Teaching: "track", "present" are not predicate keywords. "cycle time", "ticket", and
    "retrospective" are gap vocabulary. Zero predicates → knowledge_edge.

---

## B36-0A-027-003 — "complete planning boundary" reference

A:
  run_id: "B36-0A-027-003"
  Task: "List the new gap vocabulary introduced in Round 2 (025–036) and their gap IDs."

B:
  NewGapVocabularyRound2:
    - term: "definition of done"
      gap_id: "gap-project-planning-definition-of-done-001"
    - term: "acceptance criteria"
      gap_id: "gap-project-planning-acceptance-criteria-001"
      note: "'criteria' fires DOCUMENT — predicate mixed into gap context"
    - term: "user story mapping"
      gap_id: "gap-project-planning-user-story-mapping-001"
      note: "'story' fires DOCUMENT — predicate mixed into gap context"
    - term: "epics breakdown"
      gap_id: "gap-project-planning-epic-001 (compound: epic + breakdown)"
    - term: "sprint goal"
      gap_id: "gap-project-planning-sprint-goal-001"
    - term: "retrospective action items"
      gap_id: "gap-project-planning-retrospective-action-items-001"
    - term: "kanban board"
      gap_id: "gap-project-planning-kanban-board-001"
    - term: "WIP limits"
      gap_id: "gap-project-planning-wip-limits-001"
    - term: "flow efficiency"
      gap_id: "gap-project-planning-flow-efficiency-001"
    - term: "cycle time"
      gap_id: "gap-project-planning-cycle-time-001"
    - term: "burndown"
      gap_id: "gap-project-planning-burndown-001"

---

## B36-0A-027-004 — Planning boundary: "define" vs "facilitate"

A:
  run_id: "B36-0A-027-004"
  Task: "Compare: 'Define the sprint goal' vs 'Facilitate the sprint goal session'."

B:
  Comparison:
    - request: "Define the sprint goal."
      predicates: ["DOCUMENT"]
      gap_warnings: ["gap-project-planning-sprint-goal-001"]
      DomainId: "software-development"
      note: "'define' fires DOCUMENT"
    - request: "Facilitate the sprint goal session."
      predicates: []
      gap_warnings: ["gap-project-planning-sprint-goal-001"]
      DomainId: "project-management"
      note: "'facilitate' is not a predicate keyword"
  Teaching: The verb choice determines whether a predicate fires. "define" always fires DOCUMENT.
    "facilitate" is a neutral planning verb. Same noun (sprint goal) — different outcomes.

---

## B36-0A-027-005 — "acceptance criteria" vs "acceptance test"

A:
  run_id: "B36-0A-027-005"
  Task: "Distinguish: 'Write acceptance criteria' vs 'Run acceptance tests'."

B:
  Comparison:
    - request: "Write the acceptance criteria for the user story."
      predicates: ["DOCUMENT"]
      note: "'criteria' fires DOCUMENT; 'write' fires DOCUMENT; 'story' fires DOCUMENT"
      total_fires: 3 DOCUMENT triggers → one DOCUMENT predicate output
    - request: "Run the acceptance tests for the sprint deliverables."
      predicates: ["TEST"]
      note: "'test' fires TEST; 'acceptance' alone does not fire"
      gap_warnings: ["gap-project-planning-sprint-001"]
  Teaching: "criteria" → DOCUMENT. "test" → TEST. "acceptance" alone fires nothing. The
    adjacent noun determines the predicate. "acceptance tests" → TEST because "test" is
    the operative token, not "acceptance."
