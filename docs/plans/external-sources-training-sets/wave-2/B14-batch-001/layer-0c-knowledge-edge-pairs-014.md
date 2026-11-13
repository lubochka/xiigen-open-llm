# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-014-001 — "service mesh" as noun fires ARCHITECTURE in design context

A: "Architect the service mesh for the payment processing domain."
  "architect" → ARCHITECTURE | "service mesh" → ARCHITECTURE (architectural pattern noun)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "service mesh" is an architectural pattern noun that fires ARCHITECTURE when paired with a design verb.

---

## B14-edge-014-002 — "sidecar" alone does not fire ARCHITECTURE

A: "Restart the sidecar proxy on the order-service pod."
  "restart" → gap | "sidecar proxy" → gap (container/operational context without design framing)
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "sidecar proxy" as an operational target is gap.
  Teaching: "sidecar pattern" fires ARCHITECTURE (design framing). "sidecar proxy" as a running container is operational gap.

---

## B14-edge-014-003 — mTLS in isolation fires SECURITY

A: "Enforce mTLS (mutual TLS) between all services in the mesh."
  "mTLS" → SECURITY ("tls") | "enforce" → gap alone, but "enforce mTLS" is a security configuration
  Boundary: "enforce" alone is gap. "enforce mTLS" — "mTLS" fires SECURITY independently.
  CompiledPredicateIds: ["SECURITY"]
  Teaching: SECURITY vocabulary (mTLS, tls, ssl, secret) fires SECURITY regardless of the action verb around it.

---

## B14-edge-014-004 — circuit breaker pattern fires ARCHITECTURE

A: "Design the circuit breaker pattern for the service mesh — failure threshold, probe interval, and recovery strategy."
  "design" → ARCHITECTURE | "circuit breaker pattern" → ARCHITECTURE ("pattern") | "recovery strategy" → ARCHITECTURE ("strategy")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "circuit breaker" is an architectural pattern noun. "pattern" fires ARCHITECTURE.
