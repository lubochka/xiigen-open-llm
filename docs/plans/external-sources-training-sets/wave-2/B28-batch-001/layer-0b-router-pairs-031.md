# Layer 0-B — Router Pairs (031) — DependencyCheckGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-031-001 — "DependencyCheckGate" → SECURITY → SecurityFlow6x

A: ["SECURITY"] | "pass DependencyCheckGate" | [SecurityFlow6x]
  Teaching: DependencyCheckGate fires SECURITY (vulnerability check); routes to SecurityFlow6x.

## B28-router-031-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71

A: ["SECURITY", "QA"] | "dependency check + QA sign-off" | [SecurityFlow6x, ReleaseGateFlow71]
  Teaching: SECURITY → SecurityFlow6x; QA → ReleaseGateFlow71. QA always last.

## B28-router-031-003 — IMPLEMENT+DOCUMENT → ImplementationFlow41 + DocumentationFlow62

A: ["IMPLEMENT", "DOCUMENT"] | "implement checker service + document criteria" | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. Both flows instantiated.

## B28-router-031-004 — SECURITY+TEST → SecurityFlow6x + TestingFlow

A: ["SECURITY", "TEST"] | "secure pipeline + validate dependencies" | [SecurityFlow6x, TestingFlow] | documentation
  Teaching: No IMPLEMENT → documentation. SECURITY → SecurityFlow6x; TEST → TestingFlow.

## B28-router-031-005 — pure gap → knowledge_edge

A: [] | "update npm dependencies before rollout" → GapFlow | knowledge_edge
  Teaching: "update" + "rollout" → gap → knowledge_edge.
