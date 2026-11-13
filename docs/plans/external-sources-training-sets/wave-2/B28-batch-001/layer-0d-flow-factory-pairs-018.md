# Layer 0-D — Flow Factory Pairs (018)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-018-001 — QA+IMPLEMENT: migration runner + gate

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition

## B28-factory-018-002 — QA+IMPLEMENT+TEST: migration + data integrity

A: ["QA", "IMPLEMENT", "TEST"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51] | Three flows | feature_addition

## B28-factory-018-003 — QA+DOCUMENT: migration criteria

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-018-004 — QA+IMPLEMENT+TEST+DOCUMENT: full migration lifecycle

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | [ReleaseGateFlow71, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | Four flows | feature_addition

## B28-factory-018-005 — TEST alone: "validate" in migration context

A: ["TEST"] | "validate row counts after migration" | [UnitTestingFlow51] | documentation
  Teaching: "validate" fires TEST. "run migration" is gap. If only "validate" fires, result is one TEST flow.
