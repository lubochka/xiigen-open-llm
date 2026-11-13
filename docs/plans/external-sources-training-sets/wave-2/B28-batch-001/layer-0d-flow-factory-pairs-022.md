# Layer 0-D — Flow Factory Pairs (022)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-022-001 — QA+DOCUMENT: compliance criteria + gate

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation

## B28-factory-022-002 — QA+IMPLEMENT+SECURITY: compliance gate service + secure audit

A: ["QA", "IMPLEMENT", "SECURITY"] | [ReleaseGateFlow71, ImplementationFlow41, SecurityFlow6x] | Three flows | feature_addition

## B28-factory-022-003 — QA+DOCUMENT+SECURITY: security controls doc + compliance report

A: ["QA", "DOCUMENT", "SECURITY"] | [ReleaseGateFlow71, DocumentationFlow62, SecurityFlow6x] | Three flows | documentation

## B28-factory-022-004 — QA+IMPLEMENT+DOCUMENT+SECURITY: full compliance lifecycle

A: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"] | [ReleaseGateFlow71, ImplementationFlow41, DocumentationFlow62, SecurityFlow6x] | Four flows | feature_addition

## B28-factory-022-005 — gap: audit scheduling

A: [] | "schedule SOC 2 audit" → GapFlow | knowledge_edge
