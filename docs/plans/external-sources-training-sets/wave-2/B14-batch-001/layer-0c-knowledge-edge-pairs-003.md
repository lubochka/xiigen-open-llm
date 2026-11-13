# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E003-001 — "infrastructure" fires ARCHITECTURE with design verb

A: "Design the infrastructure for the new payment processing platform."
  "design" → ARCHITECTURE | "infrastructure" → ARCHITECTURE (structural noun)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "infrastructure" fires ARCHITECTURE when paired with a design verb. "check the infrastructure" → gap; "design the infrastructure" → ARCHITECTURE.

---

## E003-002 — Cloud names (AWS, Azure, GCP) are gap

A: "Move all services to AWS (Amazon Web Services)."
  "move" → gap | "AWS" → gap (platform name, not a predicate trigger)
  CompiledPredicateIds: [] → gap
  Teaching: Platform names (AWS, Azure, GCP, Kubernetes, EKS, ECS, S3) do not fire ARCHITECTURE or any predicate. They are gap vocabulary.

---

## E003-003 — "pattern" fires ARCHITECTURE in structural context

A: "Define the circuit breaker pattern for the PaymentService outbound calls."
  "define" → ARCHITECTURE | "circuit breaker pattern" → ARCHITECTURE ("pattern" as structural design concept)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "pattern" in architecture context (circuit breaker, bulkhead, sidecar, CQRS, event sourcing) fires ARCHITECTURE.

---

## E003-004 — "scale" as operational verb is gap; "scaling architecture" fires ARCHITECTURE

A: "Scale the database to handle peak load."
  "scale" → gap (operational action) | "peak load" → gap
  CompiledPredicateIds: [] → gap
  vs "Design the database scaling architecture for peak load scenarios."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "scale" as verb = gap. "scaling architecture" as a design noun = ARCHITECTURE.

---

## E003-005 — "component" fires ARCHITECTURE in structural design context

A: "Define the component boundaries for the payment processing architecture."
  "define" → ARCHITECTURE | "component boundaries" → ARCHITECTURE ("component" + "boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "component" in architectural context (component boundaries, component diagram, component dependencies) fires ARCHITECTURE.

