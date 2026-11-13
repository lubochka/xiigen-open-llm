# Layer 0-B — Router Pairs (052) — CapacityVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-052-001 — TEST: "capacity verification" → TestingFlow

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "capacity verification" fires TEST → TestingFlow. No IMPLEMENT = documentation.

## B28-router-052-002 — IMPLEMENT+TEST+QA for capacity checker service

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-052-003 — TEST: "verify scaling behavior" → TestingFlow

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "verify" in scaling context fires TEST. "configure scaling" in same sentence is gap.

## B28-router-052-004 — TEST+DOCUMENT+QA for capacity gate with criteria

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-router-052-005 — Pure gap: "capacity plan, resource quotas" → knowledge_edge

A: [] | "review capacity plan and adjust resource quotas" → GapFlow | knowledge_edge
  Teaching: Capacity planning review vocabulary is gap.
