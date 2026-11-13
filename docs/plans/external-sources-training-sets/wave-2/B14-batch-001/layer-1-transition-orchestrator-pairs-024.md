# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-024-001 — Complete TransitionType decision tree

TransitionType selection (Transition Orchestrator view):
  Step 1: Zero predicates? → unknown
  Step 2: IMPLEMENT in predicates? → feature_addition (IMPLEMENT always drives feature_addition)
  Step 3: Only ARCHITECTURE with design verbs (design, architect, structure, define, blueprint)? → feature_addition
  Step 4: ARCHITECTURE + DOCUMENT with no IMPLEMENT? → documentation
  Step 5: DOCUMENT only? → documentation
  Step 6: TEST only or TEST + ARCHITECTURE? → feature_addition
  Step 7: QA only? → feature_addition (QA gate creation)
  Default: feature_addition for any predicate except documentation cases above

---

## B14-trans-024-002 — Documentation TransitionType conditions

documentation TransitionType fires ONLY when:
  1. DOCUMENT predicate is present AND
  2. IMPLEMENT predicate is absent AND
  3. The primary action verbs are documentary (document, diagram, write up, record, describe)
  Common patterns: ARCHITECTURE+DOCUMENT (no IMPLEMENT) → documentation
  Counter-examples: ARCHITECTURE+IMPLEMENT+DOCUMENT → feature_addition (IMPLEMENT overrides)

---

## B14-trans-024-003 — feature_addition TransitionType for ARCHITECTURE alone

ARCHITECTURE alone with design verbs (design, architect, structure, define) → feature_addition
Rationale: A new architectural design artifact is a net-new deliverable = feature_addition.
This is NOT documentation because the architectural design is being created, not recorded.
Compare: "design the architecture" (feature_addition) vs "document the existing architecture" (documentation).
The distinction: creating new vs recording existing.

---

## B14-trans-024-004 — Sequential flow ordering in Transition Orchestrator

When multiple flows are created (multiple predicates), the Transition Orchestrator imposes sequential ordering:
  Phase 3.5 (ARCHITECTURE) → Phase 6.3 (SECURITY) → Phase 4.1 (IMPLEMENT) → Phase 5.1 (TEST) → Phase 6.2 (DOCUMENT) → Phase 7.3 (QA)
  This ordering ensures design precedes implementation, security is validated before code ships, and QA gates last.
  Concurrent execution is possible but ordering defines dependency chains.
