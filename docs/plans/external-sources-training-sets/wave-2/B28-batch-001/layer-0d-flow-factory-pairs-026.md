# Layer 0-D — Flow Factory Pairs (026) — SanityCheckSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-026-001 — TEST alone → TestingFlow → documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: TEST alone → documentation (no IMPLEMENT).

## B28-factory-026-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation. QA does not drive TransitionType.

## B28-factory-026-003 — ARCHITECTURE+DOCUMENT+IMPLEMENT → three flows → feature_addition

A: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"] | [ArchitectureFlow31, DocumentationFlow62, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. Three flows instantiated.

## B28-factory-026-004 — IMPLEMENT+TEST+QA → feature_addition (IMPLEMENT wins)

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition regardless of TEST and QA presence.

## B28-factory-026-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "schedule sanity check after rollout" → gap → knowledge_edge.
