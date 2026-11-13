# Layer 0-D — Flow Factory Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-004-001 — ARCHITECTURE+IMPLEMENT+DOCUMENT → 3 flows

A: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT","DOCUMENT"]) → [InfrastructureArchitectureFlow35, ImplementationFlow41, DocumentationFlow62]
  TransitionType: feature_addition

---

## B14-factory-004-002 — ARCHITECTURE+SECURITY+IMPLEMENT → 3 flows

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","SECURITY","IMPLEMENT"]) → [InfrastructureArchitectureFlow35, SecurityAuditFlow63, ImplementationFlow41]
  Three flows. SECURITY sub-phase resolved per context (6.3 or 5.2).

---

## B14-factory-004-003 — ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT → 4 flows

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT","TEST","DOCUMENT"]) → [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62]
  TransitionType: feature_addition | Four concurrent flows.

---

## B14-factory-004-004 — ARCHITECTURE+DOCUMENT+QA → 3 flows

A: ["ARCHITECTURE", "DOCUMENT", "QA"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","DOCUMENT","QA"]) → [InfrastructureArchitectureFlow35, DocumentationFlow62, QAGatingFlow73]
  TransitionType: documentation (no IMPLEMENT)

---

## B14-factory-004-005 — Five predicates → 5 flows

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51, SecurityAuditFlow63, DocumentationFlow62]
  Five concurrent flows. No cap. Each predicate routes independently.

