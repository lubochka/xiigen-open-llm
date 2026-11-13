# Layer 0-B — Router Pairs (030) — SecurityScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-030-001 — "SecurityScanGate" → SECURITY → SecurityFlow6x

A: ["SECURITY"] | "run SecurityScanGate" | [SecurityFlow6x]
  Teaching: SecurityScanGate fires SECURITY; routes to SecurityFlow6x.

## B28-router-030-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71

A: ["SECURITY", "QA"] | "security scan + QA review" | [SecurityFlow6x, ReleaseGateFlow71]
  Teaching: SECURITY → SecurityFlow6x; QA → ReleaseGateFlow71. TransitionType: documentation (no IMPLEMENT).

## B28-router-030-003 — SECURITY+IMPLEMENT → SecurityFlow6x + ImplementationFlow41

A: ["SECURITY", "IMPLEMENT"] | "secure endpoints + implement patch service" | [SecurityFlow6x, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. SECURITY → SecurityFlow6x.

## B28-router-030-004 — DOCUMENT+QA → DocumentationFlow62 + ReleaseGateFlow71

A: ["DOCUMENT", "QA"] | "document security criteria + QA sign-off" | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-router-030-005 — pure gap → knowledge_edge

A: [] | "trigger SecurityScanGate on each commit" → GapFlow | knowledge_edge
  Teaching: "trigger" + "commit" → pure gap → knowledge_edge.
