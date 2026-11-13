# Layer 3 — Teaching Unit Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T014-001 — "service mesh" fires ARCHITECTURE as architectural pattern noun

A: "Design the service mesh for the platform." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "service mesh" → ARCHITECTURE (architectural pattern noun). Both contribute. Route to 3.5.

---

## T014-002 — mTLS fires SECURITY independently of surrounding predicates

A: "Architect the service mesh topology and enforce mTLS." Predicates?
B: ["ARCHITECTURE", "SECURITY"]. "architect" + "topology" → ARCHITECTURE. "mTLS" → SECURITY ("tls"). Both fire independently. Neither suppresses the other. Routes: 3.5 + 6.3.

---

## T014-003 — "enable Istio" is gap

A: "Enable Istio on the cluster." Any predicates?
B: Zero predicates. "enable" is operational gap. "Istio" is tool name gap. Result: [] → knowledge_edge. To fire ARCHITECTURE: "design the Istio service mesh topology for the cluster."

---

## T014-004 — "circuit breaker pattern" fires ARCHITECTURE

A: "Design the circuit breaker pattern for the service mesh." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "circuit breaker pattern" → ARCHITECTURE via "pattern" (architectural pattern noun). Route to 3.5.

---

## T014-005 — ARCHITECTURE+IMPLEMENT+SECURITY for three-predicate service mesh IaC

A: "Architect the Istio sidecar topology, implement the mTLS policy, and configure the TLS certificate rotation." Predicates?
B: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]. "architect" + "sidecar topology" → ARCHITECTURE. "implement" + "mTLS policy" → IMPLEMENT + SECURITY ("mTLS"). "TLS certificate rotation" → SECURITY ("tls", "certificate"). Three flows: 3.5 + 4.1 + 6.3. TransitionType: feature_addition.
