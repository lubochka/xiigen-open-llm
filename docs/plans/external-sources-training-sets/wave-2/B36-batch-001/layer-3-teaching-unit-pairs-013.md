# Layer 3 — Teaching Unit Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-013-001 — Planning-poker: what the orchestrator must do

A:
  Question: "A developer says: 'Let's run a planning-poker session to estimate the backlog.'
             What does the orchestrator produce and why?"

B:
  Answer: "Zero predicates → knowledge-edge path. No sub-phase selected.
    DomainId: 'project-management'. SCHEDULE_CAPABILITY_LEARNING emitted.
    'planning-poker' and 'estimate' and 'backlog' are all gap vocabulary."

  Key rules:
    - Rule 1: 'planning-poker' has no registered predicate
    - Rule 17: 'estimate' is not a predicate
    - Rule 1: 'backlog' is gap vocabulary
    - Result: CompiledPredicateIds: [], knowledge_edge, DomainId: project-management

---

## B36-L3-013-002 — Planning-poker + "story": DOCUMENT fires despite planning intent

A:
  Question: "What happens when 'story points' appear in a planning-poker sentence?"

B:
  Answer: "DOCUMENT fires on 'story'. The compiler is literal. Intent (effort estimation)
    is irrelevant. CompiledPredicateIds: ['DOCUMENT']. DomainId: 'software-development'.
    Gap warning emitted for story-point estimation not captured."

  Teaching: The false positive is correct output. Do not suppress DOCUMENT.

---

## B36-L3-013-003 — Planning-poker + "review": QA fires

A:
  Question: "What happens when 'review the planning poker results' is the request?"

B:
  Answer: "QA fires on 'review'. Routes to Phase 6. 'planning poker' is gap warning.
    CompiledPredicateIds: ['QA']. DomainId: 'software-development'."

---

## B36-L3-013-004 — Planning-poker + "implement": IMPLEMENT dominates

A:
  Question: "After planning poker, 'implement the top backlog features'. Which signal wins?"

B:
  Answer: "IMPLEMENT fires on 'implement' and 'features'. 'planning poker' and 'backlog'
    are gap warnings. IMPLEMENT routes to 4.1. DomainId: 'software-development'.
    The planning-poker framing does not prevent or alter the IMPLEMENT routing."
