# Layer 0-A — Compiler Pairs (033)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-033-001 — "kanban board" pure gap request

A:
  run_id: "B36-0A-033-001"
  user_request: "Move overdue tickets to the blocked column on the kanban board."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-kanban-board-001"
  Teaching: "move" is not a predicate keyword in planning context. "tickets" and "kanban board"
    are gap vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-033-002 — "flow efficiency" + "design" fires ARCHITECTURE

A:
  run_id: "B36-0A-033-002"
  user_request: "Design a system to automatically calculate flow efficiency from kanban data."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-kanban-board-001"
  Teaching: "design" fires ARCHITECTURE. "system" is an architectural entity. "flow efficiency"
    and "kanban" are gap vocabulary. DomainId: "software-development".

---

## B36-0A-033-003 — "user story mapping" vs "user story writing"

A:
  run_id: "B36-0A-033-003"
  Task: "Both contain 'story'. Are outcomes identical?"

B:
  Comparison:
    - request: "Run a user story mapping session."
      predicates: ["DOCUMENT"]
      note: "'story' fires DOCUMENT — same rule as 'story points'"
    - request: "Write user stories for the feature backlog."
      predicates: ["DOCUMENT"]
      note: "'story' fires DOCUMENT; 'write' also fires DOCUMENT; 'stories' fires DOCUMENT"
  Teaching: Both fire DOCUMENT via "story"/"stories" token match. The outcomes are identical.
    "mapping" vs "writing" as the adjacent noun does not change the predicate — only the
    DOCUMENT lexicon token "story"/"stories" matters.

---

## B36-0A-033-004 — "definition of done" + zero predicates (complete reference)

A:
  run_id: "B36-0A-033-004"
  Task: "Enumerate all formulations of definition of done that produce zero predicates."

B:
  ZeroPredicateFormulations:
    - "Align on the definition of done."
    - "Revisit the definition of done for the sprint."
    - "Present the definition of done to the team."
    - "Update the definition of done." (no code entity → no IMPLEMENT)
    - "Communicate the definition of done." (no "stakeholder" → no DOCUMENT)
    - "Track adherence to the definition of done."
  PredicateFormulations:
    - "Define the definition of done." → DOCUMENT (verb "define" fires)
    - "Specify the definition of done." → DOCUMENT ("specify" fires)
    - "Review the definition of done." → QA ("review" fires)
    - "Document the definition of done." → DOCUMENT ("document" fires)
    - "Implement the definition of done checklist." → IMPLEMENT ("implement" + "checklist" fires)

---

## B36-0A-033-005 — "WIP limits" + "approve" fires QA

A:
  run_id: "B36-0A-033-005"
  user_request: "Approve the new WIP limits before rolling them out to all teams."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-wip-limits-001"
  Teaching: "approve" fires QA unconditionally. "WIP limits" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING: NOT emitted (QA fired).
