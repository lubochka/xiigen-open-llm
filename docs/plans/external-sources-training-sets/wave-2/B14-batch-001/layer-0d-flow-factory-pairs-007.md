# Layer 0-D — Flow Factory Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-007-001 — "blueprint" trigger → InfrastructureArchitectureFlow35

A: ["ARCHITECTURE"] | trigger: "infrastructure blueprint"
B: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  "blueprint" fires ARCHITECTURE independently. No separate design verb needed.

---

## B14-factory-007-002 — "topology" trigger → InfrastructureArchitectureFlow35

A: ["ARCHITECTURE"] | trigger: "replication topology"
B: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  "topology" in infra context fires ARCHITECTURE and routes to 3.5.

---

## B14-factory-007-003 — "diagram" trigger → InfrastructureArchitectureFlow35 (infra context)

A: ["ARCHITECTURE"] | trigger: "infrastructure diagram"
B: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  "diagram" fires ARCHITECTURE → infra vocabulary → 3.5. In app context: → 3.1.

---

## B14-factory-007-004 — "ADR" trigger → InfrastructureArchitectureFlow35 + DocumentationFlow62

A: ["ARCHITECTURE", "DOCUMENT"] | trigger: "write the ADR"
B: FlowFactory.CreateMulti(["ARCHITECTURE","DOCUMENT"]) → [InfrastructureArchitectureFlow35, DocumentationFlow62]
  ADR writing fires ARCHITECTURE ("ADR" abbreviation) and DOCUMENT ("write"). Two flows.

---

## B14-factory-007-005 — "structure" trigger → InfrastructureArchitectureFlow35

A: ["ARCHITECTURE"] | trigger: "structure the pipeline architecture"
B: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  "structure" fires ARCHITECTURE as a design verb → 3.5.

