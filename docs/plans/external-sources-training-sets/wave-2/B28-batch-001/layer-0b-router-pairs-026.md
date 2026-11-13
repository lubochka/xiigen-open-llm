# Layer 0-B — Router Pairs (026) — SanityCheckSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-026-001 — "SanityCheckSuite" → TEST → TestingFlow

A: ["TEST"] | "run SanityCheckSuite" | [TestingFlow]
  Teaching: SanityCheckSuite fires TEST; routes to TestingFlow.

## B28-router-026-002 — TEST+QA → TestingFlow + ReleaseGateFlow71

A: ["TEST", "QA"] | "sanity check + QA review" | [TestingFlow, ReleaseGateFlow71]
  Teaching: TEST → TestingFlow; QA → ReleaseGateFlow71. No TransitionType from QA alone.

## B28-router-026-003 — ARCHITECTURE+DOCUMENT+IMPLEMENT → three flows

A: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"] | "design + criteria + implement service" | [ArchitectureFlow31, DocumentationFlow62, ImplementationFlow41] | feature_addition
  Teaching: Three flows; IMPLEMENT drives feature_addition.

## B28-router-026-004 — "validate" → TEST → TestingFlow

A: ["TEST"] | "validate sanity check results" | [TestingFlow]
  Teaching: "validate" fires TEST; routes to TestingFlow regardless of context.

## B28-router-026-005 — pure gap → GapFlow → knowledge_edge

A: [] | "schedule sanity check after rollout" → GapFlow | knowledge_edge
  Teaching: "schedule" + "rollout" → gap → knowledge_edge only.
