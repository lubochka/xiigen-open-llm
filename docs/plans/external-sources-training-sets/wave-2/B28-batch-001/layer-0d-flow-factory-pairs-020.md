# Layer 0-D — Flow Factory Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-020-001 — QA alone: A/B testing gate

A: ["QA"] | [ReleaseGateFlow71] | One flow | no TransitionType contribution

## B28-factory-020-002 — QA+IMPLEMENT: experiment service + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-020-003 — QA+TEST: variant verification + gate

A: ["QA", "TEST"] | [ReleaseGateFlow71, UnitTestingFlow51] | Two flows | documentation
  Teaching: No IMPLEMENT → TransitionType: documentation (TEST does not drive feature_addition alone)

## B28-factory-020-004 — QA+IMPLEMENT+DOCUMENT: experiment framework + criteria + gate

A: ["QA", "IMPLEMENT", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, DocumentationFlow62] | Three flows | feature_addition

## B28-factory-020-005 — gap: A/B configuration

A: [] | "configure A/B split 50/50" → GapFlow | knowledge_edge
