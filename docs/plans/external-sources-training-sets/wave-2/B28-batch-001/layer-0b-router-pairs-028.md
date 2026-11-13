# Layer 0-B — Router Pairs (028) — PerformanceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-028-001 — "PerformanceGate" → TEST → TestingFlow

A: ["TEST"] | "pass PerformanceGate" | [TestingFlow]
  Teaching: PerformanceGate fires TEST; routes to TestingFlow.

## B28-router-028-002 — TEST+QA → TestingFlow + ReleaseGateFlow71

A: ["TEST", "QA"] | "load tests + validate + QA approve" | [TestingFlow, ReleaseGateFlow71]
  Teaching: TEST → TestingFlow; QA → ReleaseGateFlow71. TransitionType: none (QA alone does not contribute).

## B28-router-028-003 — IMPLEMENT+DOCUMENT → ImplementationFlow41 + DocumentationFlow62

A: ["IMPLEMENT", "DOCUMENT"] | "implement threshold service + document criteria" | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition; DOCUMENT routes to DocumentationFlow62.

## B28-router-028-004 — ARCHITECTURE+TEST → ArchitectureFlow31 + TestingFlow

A: ["ARCHITECTURE", "TEST"] | "design gate architecture + write tests" | [ArchitectureFlow31, TestingFlow] | documentation
  Teaching: No IMPLEMENT → TransitionType: documentation (ARCHITECTURE present).

## B28-router-028-005 — pure gap → knowledge_edge

A: [] | "monitor performance after release deployment" → GapFlow | knowledge_edge
  Teaching: "monitor" + "deployment" → pure gap → knowledge_edge.
