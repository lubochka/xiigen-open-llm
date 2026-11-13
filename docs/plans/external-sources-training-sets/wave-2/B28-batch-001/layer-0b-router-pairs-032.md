# Layer 0-B — Router Pairs (032) — LicenseComplianceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-032-001 — "LicenseComplianceGate" → SECURITY → SecurityFlow6x

A: ["SECURITY"] | "run LicenseComplianceGate" | [SecurityFlow6x]
  Teaching: License compliance fires SECURITY; routes to SecurityFlow6x.

## B28-router-032-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71

A: ["SECURITY", "QA"] | "license compliance + QA review" | [SecurityFlow6x, ReleaseGateFlow71]
  Teaching: SECURITY → SecurityFlow6x; QA → ReleaseGateFlow71. QA last.

## B28-router-032-003 — DOCUMENT+IMPLEMENT → DocumentationFlow62 + ImplementationFlow41

A: ["DOCUMENT", "IMPLEMENT"] | "document compliance criteria + implement checker service" | [DocumentationFlow62, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition.

## B28-router-032-004 — SECURITY+TEST → SecurityFlow6x + TestingFlow

A: ["SECURITY", "TEST"] | "secure artifact repo + validate license" | [SecurityFlow6x, TestingFlow] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-router-032-005 — pure gap → knowledge_edge

A: [] | "check all open-source licenses in the bundle" → GapFlow | knowledge_edge
  Teaching: "check" alone (non-security context) → gap → knowledge_edge.
