# Layer 0-D — Flow Factory Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-013-001 — QA alone: ReleaseGateFlow71

A: ["QA"] | [ReleaseGateFlow71] | One flow | no TransitionType contribution

## B28-factory-013-002 — QA+IMPLEMENT: blue-green service + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-013-003 — QA+IMPLEMENT+DOCUMENT: blue-green runbook

A: ["QA", "IMPLEMENT", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, DocumentationFlow62] | Three flows | feature_addition

## B28-factory-013-004 — gap: blue-green cutover

A: [] | "switch traffic to green" → GapFlow | knowledge_edge

## B28-factory-013-005 — QA+TEST+IMPLEMENT: smoke validation

A: ["QA", "TEST", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition
