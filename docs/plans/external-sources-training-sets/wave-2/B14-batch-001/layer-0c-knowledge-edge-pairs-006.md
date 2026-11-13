# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E006-001 — "gateway" fires ARCHITECTURE in routing context

A: "Design the API gateway layer with routing rules, authentication middleware, and rate limiting."
  "design" → ARCHITECTURE | "API gateway layer" → ARCHITECTURE ("gateway", "layer") | "routing rules" → ARCHITECTURE ("rules")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "gateway" in routing/structural context fires ARCHITECTURE. "API gateway" is an architectural component.

---

## E006-002 — "set up" is gap for ARCHITECTURE

A: "Set up the Kubernetes cluster for the production environment."
  "set up" → gap (operational/configuration action, not architecture design) | "Kubernetes cluster" → gap (platform name)
  CompiledPredicateIds: [] → gap
  Teaching: "set up", "configure", "deploy", "install" are gap verbs for ARCHITECTURE. Requires: design, architect, structure, blueprint, diagram, define (in structural context).

---

## E006-003 — "replication" fires ARCHITECTURE in topology context

A: "Define the cross-region replication topology for the EventStore."
  "define" → ARCHITECTURE | "cross-region replication topology" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "replication" in topology/design context fires ARCHITECTURE. Database replication topology, event store replication design → ARCHITECTURE.

---

## E006-004 — "pods" / "containers" / "namespaces" are gap alone

A: "List all pods running in the production namespace."
  "list" → gap | "pods" → gap (Kubernetes operational vocabulary) | "production namespace" → gap
  CompiledPredicateIds: [] → gap
  Teaching: Kubernetes operational terms (pod, container, namespace, deployment, replica set) are gap by themselves. They only contribute to ARCHITECTURE when inside a design-framing sentence ("structure the pod scheduling architecture").

---

## E006-005 — "failover" fires ARCHITECTURE in topology design

A: "Design the failover topology for the PaymentService — active-passive with automatic DNS (Domain Name System) promotion."
  "design" → ARCHITECTURE | "failover topology" → ARCHITECTURE | "active-passive" → ARCHITECTURE (structural pattern) | "automatic DNS promotion" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "failover" in topology/design context fires ARCHITECTURE. Failover is a structural pattern.

