# Layer 0-B — Router Pairs (035) — ContainerScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-035-001 — "ContainerScanGate" → SECURITY → SecurityFlow6x

A: ["SECURITY"] | "pass ContainerScanGate" | [SecurityFlow6x]
  Teaching: Container scan fires SECURITY; routes to SecurityFlow6x.

## B28-router-035-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71

A: ["SECURITY", "QA"] | "container scan + QA review" | [SecurityFlow6x, ReleaseGateFlow71]
  Teaching: SECURITY → SecurityFlow6x; QA → ReleaseGateFlow71.

## B28-router-035-003 — IMPLEMENT+SECURITY → ImplementationFlow41 + SecurityFlow6x

A: ["IMPLEMENT", "SECURITY"] | "build container + service + secure scan" | [ImplementationFlow41, SecurityFlow6x] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. "service" fires IMPLEMENT.

## B28-router-035-004 — DOCUMENT+QA → DocumentationFlow62 + ReleaseGateFlow71

A: ["DOCUMENT", "QA"] | "document scan criteria + QA approve policy" | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-router-035-005 — pure gap → knowledge_edge

A: [] | "push scanned container to artifact registry" → GapFlow | knowledge_edge
  Teaching: "push" + "registry" → gap → knowledge_edge.
