# Layer 0-D — Flow Factory Pairs (003)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-003-001 — IMPLEMENT+QA (feature gate)
Predicates: ["IMPLEMENT", "QA"] → ImplementationFlow41 + QAGatingFlow73 | QA sub-phase 7.3

## FF-003-002 — IMPLEMENT+QA (release gate)
Predicates: ["IMPLEMENT", "QA"] → ImplementationFlow41 + ReleaseGateFlow71 | QA sub-phase 7.1

## FF-003-003 — ARCHITECTURE+IMPLEMENT
Predicates: ["ARCHITECTURE", "IMPLEMENT"] → ArchitectureFlow31 + ImplementationFlow41 | TransitionType: feature_addition

## FF-003-004 — IMPLEMENT+AUTHORIZATION
Predicates: ["IMPLEMENT", "AUTHORIZATION"] → ImplementationFlow41 + AuthorizationFlow | TransitionType: feature_addition

## FF-003-005 — IMPLEMENT refactoring
Predicates: ["IMPLEMENT"] | TransitionType: refactoring (refactor verb present)
Factory: ImplementationFlow41 | Same flow; different TransitionType.
