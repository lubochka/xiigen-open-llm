# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-024-001 — Complete TransitionType classification rules

| Request Pattern | TransitionType |
|----------------|----------------|
| "build/create/implement/scaffold/bootstrap new service" | feature_addition |
| "extend/add to existing service" | feature_addition |
| "refactor existing service" | refactoring |
| "refactor + extend" | refactoring (refactor wins) |
| "refactor + write tests" | refactoring (both predicates fire; TransitionType: refactoring) |

---

## B15-t1-024-002 — Complete SubPhase ordering rules

Canonical ordering for full pipeline:
1. ArchitectureFlow31 (sub-phase 3.1) — first
2. APIContractFlow33 (sub-phase 3.3) — before implementation
3. SecurityAuditFlow — before implementation
4. ImplementationFlow41 (sub-phase 4.1)
5. TestingFlow51 / E2ETestingFlow53 (sub-phase 5.1 / 5.3)
6. DocumentationFlow21 (sub-phase 2.1) — parallel with 4.1 and 5.x
7. QAGatingFlow71 / ReleaseGateFlow73 (sub-phase 7.1 / 7.3) — always last

---

## B15-t1-024-003 — TransitionId format

Format: "transition-{batch_id}-{run_id}-{sub_phases}-{transition_type}"
Example: "transition-B15-t1-013-001-4.1-feature_addition"
Multi-phase: "transition-B15-t1-016-003-multi-feature_addition"
Refactoring: "transition-B15-t1-022-001-4.1-refactoring"

---

## B15-t1-024-004 — ContextCarryForward rule

ContextCarryForward: true for all B15 transitions. Context from each completed flow carries into the next flow in the ordered pipeline. Architecture decisions carry into API contract design; API contracts carry into implementation; implementation carries into testing.

---

## B15-t1-024-005 — No GapHandler transitions in B15

B15 has no GapHandler (GAP) transitions. Every meaningful request fires at least IMPLEMENT. GapHandler only fires when no predicate matches at all — which does not occur in B15 because all requests contain at least one IMPLEMENT trigger verb.
