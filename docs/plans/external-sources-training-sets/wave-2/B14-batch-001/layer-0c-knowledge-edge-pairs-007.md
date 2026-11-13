# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E007-001 — "sidecar" fires ARCHITECTURE

A: "Define the sidecar proxy pattern for the AuthService."
  "define" → ARCHITECTURE | "sidecar proxy pattern" → ARCHITECTURE ("sidecar", "pattern")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "sidecar" fires ARCHITECTURE as a recognized structural design pattern. Service mesh sidecars are architectural components.

---

## E007-002 — "egress" / "ingress" fire ARCHITECTURE in network design context

A: "Design the ingress and egress control layers for the cluster perimeter."
  "design" → ARCHITECTURE | "ingress/egress control layers" → ARCHITECTURE ("layers")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "ingress" and "egress" in network design context fire ARCHITECTURE. They represent boundary and flow control — architectural structural concepts.

---

## E007-003 — "monolith" fires ARCHITECTURE in design context

A: "Design the migration path from monolith to microservices architecture."
  "design" → ARCHITECTURE | "migration path" → ARCHITECTURE (structural transition) | "monolith to microservices architecture" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "monolith" in architectural design context fires ARCHITECTURE. Monolith-to-microservices decomposition is an architectural activity.

---

## E007-004 — "microservices" alone is gap; "microservices architecture" fires ARCHITECTURE

A: "We are using microservices."
  "using" → gap | "microservices" → gap (architectural style name without design verb)
  CompiledPredicateIds: [] → gap
  vs "Design the microservices architecture for the platform."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "microservices" as a standalone noun is gap. "microservices architecture" + design verb = ARCHITECTURE.

---

## E007-005 — "CQRS" (Command Query Responsibility Segregation) fires ARCHITECTURE

A: "Design the CQRS (Command Query Responsibility Segregation) pattern for the OrderService."
  "design" → ARCHITECTURE | "CQRS pattern" → ARCHITECTURE (architectural pattern abbreviation)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "CQRS" fires ARCHITECTURE as a recognized architectural pattern abbreviation.

