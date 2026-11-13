# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-002-001 — ARCHITECTURE alone → documentation

A: ["ARCHITECTURE"] | "design the load balancer topology"
B: TransitionType: documentation | Sequential: design → document
  Teaching: ARCHITECTURE without IMPLEMENT → documentation TransitionType. Design artifacts = documentation outputs.

---

## B14-transition-002-002 — ARCHITECTURE+IMPLEMENT → feature_addition

A: ["ARCHITECTURE", "IMPLEMENT"] | "architect the service mesh + implement sidecar configs"
B: TransitionType: feature_addition | Sequential: ARCHITECTURE design → IMPLEMENT code
  Teaching: IMPLEMENT drives feature_addition. Architectural design + code implementation = feature.

---

## B14-transition-002-003 — ARCHITECTURE+DOCUMENT → documentation

A: ["ARCHITECTURE", "DOCUMENT"] | "design the topology + document in ADRs"
B: TransitionType: documentation | Sequential: ARCHITECTURE design → DOCUMENT recording
  Teaching: DOCUMENT without IMPLEMENT → documentation TransitionType even with ARCHITECTURE.

---

## B14-transition-002-004 — ARCHITECTURE+IMPLEMENT+TEST → feature_addition

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"] | IaC workflow
B: TransitionType: feature_addition | Sequential: ARCHITECTURE → IMPLEMENT → TEST
  Teaching: IaC delivery: design infrastructure, implement modules, validate with tests. IMPLEMENT drives feature_addition.

---

## B14-transition-002-005 — gap → no TransitionType

A: [] | "plan the infrastructure setup"
B: No TransitionType assigned | GapHandler path
  Teaching: Gap requests have no TransitionType. TransitionType is only assigned when at least one non-gap predicate is compiled.

