# Layer 0-A — Compiler Pairs (029)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-029-001 — "burndown" + "review" fires QA

A:
  run_id: "B36-0A-029-001"
  user_request: "Review the burndown chart at end of sprint to assess progress."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA regardless of planning context. "burndown" and "sprint" produce
    gap warnings. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## B36-0A-029-002 — "definition of done" pure gap (no define)

A:
  run_id: "B36-0A-029-002"
  user_request: "Align the team on our definition of done before the sprint kickoff."
  Task: "Identify predicates. Note: 'definition' is the noun — does it fire DOCUMENT?"

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
  Teaching: "definition" as a noun (without the verb "define") does NOT fire DOCUMENT.
    Only the verb "define" fires the predicate. "align" is neutral. Zero predicates → knowledge_edge.

---

## B36-0A-029-003 — "retrospective action items" + IMPLEMENT boundary

A:
  run_id: "B36-0A-029-003"
  Task: "Distinguish: 'Capture retrospective action items' vs 'Implement the retrospective action item to add CI/CD'."

B:
  Version1:
    request: "Capture retrospective action items and assign to team members."
    predicates: []
    gap_warnings: ["gap-project-planning-retrospective-action-items-001", "gap-project-planning-retrospective-001"]
    DomainId: "project-management"
  Version2:
    request: "Implement the retrospective action item: add CI/CD pipeline to the codebase."
    predicates: ["IMPLEMENT"]
    gap_warnings: ["gap-project-planning-retrospective-action-items-001"]
    DomainId: "software-development"
    note: "'implement' + 'pipeline' + 'codebase' fire IMPLEMENT"
  Teaching: "capture" is neutral. "implement" + code entity fires IMPLEMENT. Same planning
    term (retrospective action items) — the action verb determines the predicate.

---

## B36-0A-029-004 — "cycle time" + "feature" fires IMPLEMENT

A:
  run_id: "B36-0A-029-004"
  user_request: "Track cycle time and build a feature to surface it in the dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-cycle-time-001"
  Teaching: "build" fires IMPLEMENT. "feature" in delivery context also fires IMPLEMENT.
    "track" is neutral. "cycle time" is gap vocabulary producing a gap warning.
    DomainId: "software-development".

---

## B36-0A-029-005 — "user story mapping" pure gap (no story verb)

A:
  run_id: "B36-0A-029-005"
  user_request: "Run a user story mapping workshop to identify the MVP scope."
  Task: "Identify predicates. Note: 'story' in 'user story mapping'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-user-story-mapping-001"
  Teaching: "story" fires DOCUMENT (false positive). The planning intent ("workshop to identify
    MVP scope") does not suppress the predicate. "run" is neutral. DomainId: "software-development".
    This is the same false-positive rule as "story points."
