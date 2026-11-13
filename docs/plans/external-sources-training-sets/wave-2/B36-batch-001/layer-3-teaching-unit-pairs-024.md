# Layer 3 — Teaching Unit Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-024-001 — Complete B36 teaching: 5 rules to master

A:
  Question: "State the 5 most important rules to master for B36 gap batch handling."

B:
  Rules:
    1. Zero predicates → knowledge_edge, DomainId: project-management, SCHEDULE_CAPABILITY_LEARNING emitted.
    2. One predicate → routed, DomainId: software-development, SCHEDULE_CAPABILITY_LEARNING NOT emitted.
    3. "story" → DOCUMENT fires (false positive in "story points"; true positive in "user story"). Never suppress.
    4. "review" → QA fires. "retrospective" → gap (never QA). The boundary is sharp and literal.
    5. QA-last: when QA co-occurs with other predicates, QA flow executes last.

---

## B36-L3-024-002 — Three v6 corruption types: definitions

A:
  Question: "What are the three v6 corruption types for B36?"

B:
  Corruptions:
    v6-01: "PROJECT_PLANNING predicate invented. Zero predicates should fire but corrupt model emits
      CompiledPredicateIds: ['PROJECT_PLANNING']. This predicate does not exist."

    v6-02: "Predicate suppressed because sentence 'feels like planning'. Model emits
      CompiledPredicateIds: [] for a sentence containing 'review' (QA) or 'story' (DOCUMENT).
      The predicate must fire unconditionally."

    v6-03: "DomainId: 'project-management' emitted when predicate fired. Model confuses gap
      vocabulary proportion with DomainId determination. DomainId is binary: predicate fired
      → 'software-development'. Zero predicates → 'project-management'."

---

## B36-L3-024-003 — SCHEDULE_CAPABILITY_LEARNING complete teaching

A:
  Question: "What is SCHEDULE_CAPABILITY_LEARNING, when is it emitted, and what does it NOT do?"

B:
  Definition: "A signal emitted by the orchestrator when CompiledPredicateIds: [] and known
    gap vocabulary is detected. It notifies the AutonomousLearningPipeline (subproject 16)
    that new vocabulary needs a registered predicate or sub-phase."

  When emitted: "CompiledPredicateIds: [] AND gap vocabulary present."
  When NOT emitted: "Any predicate fires (even false positive DOCUMENT via 'story')."

  What it does NOT do:
    - Does NOT cause the current request to be retried
    - Does NOT cause a fallback flow to execute
    - Does NOT affect DomainId
    - Does NOT produce FinalHumanResult

  Format: One signal per request. All gap IDs bundled.

---

## B36-L3-024-004 — Complete DomainId rule with all edge cases

A:
  Question: "State the complete DomainId determination rule with all B36 edge cases."

B:
  PrimaryRule:
    "CompiledPredicateIds: [] → DomainId: 'project-management'"
    "Any predicate fired → DomainId: 'software-development'"

  EdgeCases:
    - "story fires DOCUMENT (false positive) → DomainId: 'software-development' (predicate fired)"
    - "review fires QA in retrospective context → DomainId: 'software-development'"
    - "design fires ARCHITECTURE in roadmap context → DomainId: 'software-development'"
    - "10 gap terms + 0 predicates → DomainId: 'project-management'"
    - "1 gap term + 1 predicate → DomainId: 'software-development'"

  Hard rule: "Gap vocabulary proportion is irrelevant to DomainId. One predicate fires
    → 'software-development'. Always. No exceptions."
