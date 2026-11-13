# Layer 0-D — Flow Factory Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-011-001 — ARCHITECTURE+IMPLEMENT+TEST+SECURITY+DOCUMENT+QA → 6 flows

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT", "QA"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51, SecurityAuditFlow63, DocumentationFlow62, QAGatingFlow73]
  Six concurrent flows. TransitionType: feature_addition. Full platform delivery lifecycle.

---

## B14-factory-011-002 — ARCHITECTURE: "define" verb in structural context → ARCHITECTURE

A: ["ARCHITECTURE"] | trigger: "define the network boundary rules"
B: [InfrastructureArchitectureFlow35]
  "define" fires ARCHITECTURE only in structural design context (boundary rules, topology, layer configuration). "define the requirements" → gap.

---

## B14-factory-011-003 — ARCHITECTURE+IMPLEMENT: TransitionType feature_addition always

A: ["ARCHITECTURE", "IMPLEMENT"]
B: TransitionType: feature_addition regardless of other predicates
  Teaching: IMPLEMENT drives feature_addition. Even "refactor + implement" → feature_addition (IMPLEMENT overrides refactoring).

---

## B14-factory-011-004 — ARCHITECTURE: "refactor" verb

A: ["ARCHITECTURE"] | trigger: "refactor the service mesh architecture"
B: [InfrastructureArchitectureFlow35] | TransitionType: refactoring
  Teaching: "refactor" overrides documentation TransitionType for ARCHITECTURE.

---

## B14-factory-011-005 — Gap: no flow, SCHEDULE_CAPABILITY_LEARNING

A: [] | "our infrastructure needs improvement"
B: FlowFactory.CreateGap() → GapFlow | SCHEDULE_CAPABILITY_LEARNING emitted
  Teaching: Vague infrastructure intent without design verb → gap. GapHandler emits SCHEDULE_CAPABILITY_LEARNING.

