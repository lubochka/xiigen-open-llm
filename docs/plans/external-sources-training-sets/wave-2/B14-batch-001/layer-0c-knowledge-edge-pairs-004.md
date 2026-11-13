# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E004-001 — "boundary" fires ARCHITECTURE

A: "Define the service boundary and data ownership rules for the OrderManagement domain."
  "define" → ARCHITECTURE | "service boundary" → ARCHITECTURE ("boundary") | "data ownership rules" → ARCHITECTURE ("rules" as structural constraints)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "boundary" in architectural context (service boundary, system boundary, trust boundary, data boundary) fires ARCHITECTURE.

---

## E004-002 — "strategy" fires ARCHITECTURE in design context

A: "Define the data partitioning strategy for the UserService sharded database."
  "define" → ARCHITECTURE | "data partitioning strategy" → ARCHITECTURE ("strategy" as design decision)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "strategy" as a design artifact (caching strategy, scaling strategy, retry strategy, partitioning strategy) fires ARCHITECTURE in design-verb context.

---

## E004-003 — "diagram" fires ARCHITECTURE

A: "Diagram the service-to-service dependency graph for the microservices platform."
  "diagram" → ARCHITECTURE (design verb) | "dependency graph" → ARCHITECTURE | "microservices platform" → ARCHITECTURE ("platform" in arch framing)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "diagram" fires ARCHITECTURE as a design verb. It also implies DOCUMENT. When used together with "document", both predicates fire.

---

## E004-004 — "migration" alone is gap; "migration architecture" fires ARCHITECTURE

A: "Migrate the OrderService database from PostgreSQL to MongoDB."
  "migrate" → gap (operational action without architecture framing) | "PostgreSQL", "MongoDB" → gap (database names)
  CompiledPredicateIds: [] → gap
  vs "Design the migration architecture for the OrderService database transition."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "migrate" = gap. "migration architecture" or "design the migration strategy" = ARCHITECTURE.

---

## E004-005 — "ADR" fires ARCHITECTURE

A: "Write the ADR (Architecture Decision Record) for the chosen message broker technology."
  "write" → DOCUMENT (in context of writing records) | "ADR" → ARCHITECTURE (abbreviation for Architecture Decision Record) | "Architecture Decision Record" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Teaching: "ADR" as an abbreviation fires ARCHITECTURE. The full form "Architecture Decision Record" also fires ARCHITECTURE.

