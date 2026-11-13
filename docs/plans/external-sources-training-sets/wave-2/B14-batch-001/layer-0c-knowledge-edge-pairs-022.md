# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-022-001 — "zero-trust" fires SECURITY AND ARCHITECTURE

A: "Design the zero-trust network architecture for the microservices platform."
  "design" → ARCHITECTURE | "zero-trust network architecture" → ARCHITECTURE ("architecture", "network") + SECURITY ("zero-trust")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Teaching: "zero-trust" fires SECURITY independently, even when embedded in an ARCHITECTURE phrase. Both predicates fire.

---

## B14-edge-022-002 — "PrivateLink" alone does not fire SECURITY

A: "Set up AWS PrivateLink for the payment service endpoint."
  "set up" → gap | "AWS PrivateLink" → gap (platform service name) | "payment service endpoint" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "PrivateLink" is a platform service name, not a security predicate trigger.
  Teaching: "PrivateLink" = gap. To fire ARCHITECTURE+SECURITY: "design the PrivateLink service boundary topology and implement the TLS endpoint configuration."

---

## B14-edge-022-003 — "egress" topology fires ARCHITECTURE

A: "Architect the egress control topology — outbound traffic gateway, allowlist policies, and DNS filtering layer."
  "architect" → ARCHITECTURE | "egress control topology" → ARCHITECTURE ("topology") | "DNS filtering layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "egress" in network topology design fires ARCHITECTURE via "topology" and "layer".

---

## B14-edge-022-004 — "NetworkPolicy" deny-all fires ARCHITECTURE (structural boundary)

A: "Define the NetworkPolicy deny-all baseline and the explicit ingress rules for the production namespace."
  "define" → ARCHITECTURE | "NetworkPolicy deny-all baseline" → ARCHITECTURE ("boundary") | "ingress rules" → ARCHITECTURE (structural rule definition) | "production namespace" → gap
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "define" + structural boundary rules → ARCHITECTURE. "NetworkPolicy" is a Kubernetes resource but "deny-all baseline" is an architectural boundary decision.
