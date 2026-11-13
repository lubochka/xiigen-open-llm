# Layer 0-A — Compiler Pairs (035)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-035-001 — "kanban board" + "use-case" fires DOCUMENT

A:
  run_id: "B36-0A-035-001"
  user_request: "Outline the use-case for our kanban board workflow optimization."
  Task: "Identify predicates. 'use-case' is in DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-flow-efficiency-001"
  Teaching: "use-case" fires DOCUMENT unconditionally. "outline" is neutral. "kanban board"
    is gap vocabulary. DomainId: "software-development".

---

## B36-0A-035-002 — "acceptance criteria" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-035-002"
  user_request: "Implement the acceptance criteria validation feature for the backlog items."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-acceptance-criteria-001"
    - "gap-project-planning-backlog-001"
  Teaching: "implement" fires IMPLEMENT. "criteria" fires DOCUMENT. "feature" also fires
    IMPLEMENT. Two predicates fired. "acceptance criteria" and "backlog" are gap vocabulary.
    DomainId: "software-development".

---

## B36-0A-035-003 — "sprint goal" + "definition of done" zero-predicate

A:
  run_id: "B36-0A-035-003"
  user_request: "Align sprint goal with the definition of done for the current iteration."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-iteration-001"
  Teaching: "align" is neutral. All four planning terms are gap vocabulary. Zero predicates
    → knowledge_edge. DomainId: "project-management".

---

## B36-0A-035-004 — "user story mapping" + "epics breakdown" without "story" verb

A:
  run_id: "B36-0A-035-004"
  Task: "Explain why 'user story mapping session' always fires DOCUMENT."

B:
  Explanation:
    token_scan:
      - "user": no predicate
      - "story": DOCUMENT fires (lexicon match — positional)
      - "mapping": no predicate
      - "session": no predicate
    rule: "Lexicon matching is positional. 'story' appears in the token stream regardless of
      whether it is part of a compound noun. DOCUMENT fires when 'story' or 'stories' is found
      as a word token."
    avoidance: "To avoid DOCUMENT firing, use 'journey mapping' or 'experience mapping' instead
      of 'story mapping'. Those formulations do not contain the 'story' token."

---

## B36-0A-035-005 — "retrospective action items" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-035-005"
  user_request: "Build the retrospective action item tracking dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-action-items-001"
    - "gap-project-planning-retrospective-001"
  Teaching: "build" fires IMPLEMENT. "tracking dashboard" is a code entity. "retrospective
    action items" is gap vocabulary. DomainId: "software-development".
