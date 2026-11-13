# Layer 0-A — Compiler Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-009-001 — ARCHITECTURE: canary deployment

A: "Design the canary deployment architecture — define the traffic splitting rules, rollback triggers, and observability hooks."
  "design" → ARCHITECTURE | "canary deployment architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "traffic splitting rules" → ARCHITECTURE ("rules") | "rollback triggers" → ARCHITECTURE ("triggers" as structural pattern) | "observability hooks" → ARCHITECTURE ("hooks")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-009-002 — ARCHITECTURE+SECURITY: zero-trust network

A: "Design the zero-trust network architecture — define mutual authentication boundaries and encrypt all east-west traffic."
  "design" → ARCHITECTURE | "zero-trust network architecture" → ARCHITECTURE + SECURITY ("zero-trust") | "define" → ARCHITECTURE | "mutual authentication boundaries" → SECURITY + ARCHITECTURE ("boundaries") | "encrypt" → SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]

---

## B14-compiler-009-003 — ARCHITECTURE: sidecar pattern

A: "Architect the sidecar proxy pattern for the service mesh — define the configuration injection boundary and traffic intercept layer."
  "architect" → ARCHITECTURE | "sidecar proxy pattern" → ARCHITECTURE ("pattern") | "service mesh" → ARCHITECTURE | "define" → ARCHITECTURE | "configuration injection boundary" → ARCHITECTURE ("boundary") | "traffic intercept layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-009-004 — ARCHITECTURE: horizontal pod autoscaling

A: "Structure the horizontal pod autoscaling architecture — define the HPA (Horizontal Pod Autoscaler) thresholds, cooldown periods, and minimum/maximum replica boundaries."
  "structure" → ARCHITECTURE | "horizontal pod autoscaling architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "HPA thresholds" → ARCHITECTURE ("thresholds" as design parameters) | "replica boundaries" → ARCHITECTURE ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-009-005 — gap: "configure the firewall rules"

A: "Configure the firewall rules to allow traffic on port 443."
  "configure" → gap (ARCHITECTURE requires design/architect verbs) | "firewall rules" → gap (without architecture framing) | "port 443" → gap (operational detail)
  CompiledPredicateIds: [] → gap
  Teaching: "configure" is a gap verb. Architectural firewall design = "design the network perimeter architecture and define the firewall policy boundaries".

