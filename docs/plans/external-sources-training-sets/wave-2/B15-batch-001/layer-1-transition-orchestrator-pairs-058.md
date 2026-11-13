# Layer 1 — Transition Orchestrator Pairs (058)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-058-001 — Complete B15 transition output format (final reference)

SelectedLogicalTransition fields:
- TransitionType: "feature_addition" | "refactoring"
- SubPhaseIds: string[] (ordered, deduplicated)
- ContextCarryForward: true (always in B15)
- SourceRequest: original user request text

---

## B15-t1-058-002 — SubPhaseIds deduplication from cross-service multi-domain requests

A: "Build SagaOrchestrator with a saga domain model and CompensationService with a compensation domain model — both have domain models."
B: SubPhaseIds: ["3.1", "4.1"] | Two ARCHITECTURE triggers → one "3.1".
Two IMPLEMENT triggers (both services) → one "4.1".

---

## B15-t1-058-003 — Full seven-predicate transition (all predicates)

A: "Design the event architecture and domain model, define event API contracts, build EventPublisherService, secure event endpoints, write integration tests, document the event schema, and QA before release."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "3.3", "security", "4.1", "5.1", "2.1", "7.1"] | ContextCarryForward: true
Seven predicates: ARCHITECTURE + API_CONTRACT + SECURITY + IMPLEMENT + TEST + DOCUMENT + QA.

---

## B15-t1-058-004 — QA position confirmed at end in seven-predicate SubPhaseIds

SubPhaseIds: ["3.1", "3.3", "security", "4.1", "5.1", "2.1", "7.1"]
QA ("7.1") is always the last element. All other sub-phases maintain their relative ordering.

---

## B15-t1-058-005 — Maximum SubPhaseIds length in B15 = 7

Maximum: ["3.1", "3.3", "security", "4.1", "5.1", "2.1", "7.1"] — seven entries.
Replacing "5.1" with "5.3" or "7.1" with "7.3" keeps the count at seven.
No B15 request generates more than seven unique predicates.
