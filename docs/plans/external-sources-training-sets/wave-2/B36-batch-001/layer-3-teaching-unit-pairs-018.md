# Layer 3 — Teaching Unit Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-018-001 — Prioritize and triage: both are gap vocabulary

A:
  Question: "Do 'prioritize' and 'triage' fire any predicate?"

B:
  Answer: "Neither fires any predicate. Both are gap vocabulary:
    - 'prioritize': ordering action for backlog/tickets, not in any predicate lexicon
    - 'triage': issue assignment/sorting action, not in any predicate lexicon
    Both produce knowledge-edge when no predicate co-occurs. DomainId: project-management."

---

## B36-L3-018-002 — Triage + define: DOCUMENT fires despite triage framing

A:
  Question: "'Triage the backlog and define priority criteria.' What fires?"

B:
  Answer: "DOCUMENT fires on 'define' and 'criteria'. Triage and backlog are gap.
    CompiledPredicateIds: ['DOCUMENT']. Routes to 2.4 (criteria context → stakeholder requirements).
    DomainId: software-development. Gap warning for backlog-001, ticket-001."

---

## B36-L3-018-003 — "triage" vs "review": only one fires QA

A:
  Question: "Compare: 'Triage the tickets' vs 'Review the tickets'. Which fires QA?"

B:
  Answer: "'Triage the tickets': 'triage' is gap. 'tickets' is gap. Zero predicates → knowledge-edge.
    'Review the tickets': 'review' fires QA. Routes to Phase 6.
    'Triage' ≠ 'review'. The model must learn this boundary: triage is always gap;
    review always fires QA."

---

## B36-L3-018-004 — Multi-gap triage: all six IDs bundled

A:
  Question: "'Triage epic backlog, prioritize sprint tickets, set velocity targets for next iteration.'
             How many SCHEDULE_CAPABILITY_LEARNING signals are emitted?"

B:
  Answer: "One signal. Six gap IDs (epic-001, backlog-001, sprint-001, ticket-001,
    velocity-001, iteration-001) are all bundled in one signal per Rule 22:
    { signal: 'SCHEDULE_CAPABILITY_LEARNING', capabilityGapIds: [all six] }.
    Multiple gap IDs never produce multiple signals."
