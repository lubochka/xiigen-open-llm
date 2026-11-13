# Layer 0-D — Flow Factory Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-002-001 — ARCHITECTURE alone → InfrastructureArchitectureFlow35

A: ["ARCHITECTURE"] | infra context
B: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  Flow: InfrastructureArchitectureFlow35
  TransitionType: documentation

---

## B14-factory-002-002 — ARCHITECTURE+IMPLEMENT → 2 flows

A: ["ARCHITECTURE", "IMPLEMENT"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT"]) → [InfrastructureArchitectureFlow35, ImplementationFlow41]
  TransitionType: feature_addition

---

## B14-factory-002-003 — ARCHITECTURE+DOCUMENT → 2 flows

A: ["ARCHITECTURE", "DOCUMENT"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","DOCUMENT"]) → [InfrastructureArchitectureFlow35, DocumentationFlow62]
  TransitionType: documentation

---

## B14-factory-002-004 — ARCHITECTURE+SECURITY → 2 flows

A: ["ARCHITECTURE", "SECURITY"] | audit context
B: FlowFactory.CreateMulti(["ARCHITECTURE","SECURITY"]) → [InfrastructureArchitectureFlow35, SecurityAuditFlow63]
  Both flows created. ARCHITECTURE is not suppressed by SECURITY.

---

## B14-factory-002-005 — gap → FlowFactory.CreateGap()

A: []
B: FlowFactory.CreateGap() → GapFlow
  No InfrastructureArchitectureFlow35. SCHEDULE_CAPABILITY_LEARNING emitted.

