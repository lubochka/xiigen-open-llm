# Layer 0-A — Compiler Pairs (054) — LoadBalancerSwitch
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-054-001 — "load balancer switch" is gap

A: "Switch the load balancer to route to the new backend pool."
  "switch load balancer" → gap | "route to new backend pool" → gap
  Teaching: Load balancer routing vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-054-002 — IMPLEMENT+QA for load balancer controller service

A: "Implement the load balancer switch controller service and QA the traffic distribution."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Implement+service → IMPLEMENT. QA fires on abbreviation.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-054-003 — "weighted routing" and "round-robin" are gap

A: "Configure weighted routing and round-robin load distribution."
  "configure" → gap | "weighted routing" → gap | "round-robin" → gap | "load distribution" → gap
  Teaching: Load balancing strategy configuration vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-054-004 — TEST+QA for load balancer verification

A: "Verify load distribution is correct after the switch and QA the routing rules."
  "verify" → TEST | "load distribution" → gap | "QA" → QA
  Teaching: "verify" fires TEST. QA last. Load balancer context is gap filler.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-054-005 — SECURITY+QA for load balancer TLS (Transport Layer Security)

A: "Secure the load balancer TLS (Transport Layer Security) termination and QA the certificate handshake."
  "secure" → SECURITY | "QA" → QA
  Teaching: "secure" fires SECURITY in TLS context. QA last.
  CompiledPredicateIds: ["SECURITY", "QA"]
