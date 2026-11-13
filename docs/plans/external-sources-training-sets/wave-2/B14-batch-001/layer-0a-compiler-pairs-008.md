# Layer 0-A — Compiler Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-008-001 — ARCHITECTURE: ingress layer

A: "Design the ingress layer architecture — define the TLS termination, WAF (Web Application Firewall) placement, and DDoS (Distributed Denial of Service) mitigation boundary."
  "design" → ARCHITECTURE | "ingress layer architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "TLS termination" → SECURITY ("TLS") + ARCHITECTURE (termination point = structural placement) | "WAF placement" → SECURITY + ARCHITECTURE | "DDoS mitigation boundary" → ARCHITECTURE ("boundary") + SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]

---

## B14-compiler-008-002 — ARCHITECTURE: stateful vs stateless

A: "Structure the service architecture — define which components should be stateless and which must maintain persistent state, and design the state management layer."
  "structure" → ARCHITECTURE | "service architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "stateless/persistent state components" → ARCHITECTURE ("components", "state management") | "design" → ARCHITECTURE | "state management layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-008-003 — ARCHITECTURE+TEST+IMPLEMENT: infra as code

A: "Architect the infrastructure-as-code (IaC) pipeline — implement the Terraform modules and write integration tests to validate provisioning."
  "architect" → ARCHITECTURE | "infrastructure-as-code pipeline" → ARCHITECTURE | "implement" → IMPLEMENT | "Terraform modules" → IMPLEMENT | "write integration tests" → TEST | "validate" → TEST
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST"]

---

## B14-compiler-008-004 — ARCHITECTURE: DNS (Domain Name System)

A: "Design the DNS (Domain Name System) routing layer — define the GeoDNS (Geographic DNS) failover topology and health-check policies."
  "design" → ARCHITECTURE | "DNS routing layer" → ARCHITECTURE ("layer") | "define" → ARCHITECTURE | "GeoDNS failover topology" → ARCHITECTURE | "health-check policies" → ARCHITECTURE ("policies" as structural constraints)
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-008-005 — gap: "add a load balancer"

A: "Add a load balancer to the setup."
  "add" → gap (not an architecture design verb) | "load balancer" → gap (component name without architecture framing) | "setup" → gap
  CompiledPredicateIds: [] → gap
  Teaching: "add" is gap for ARCHITECTURE. Requires: design/architect/structure/blueprint/diagram. "Add a component" is operational/IMPLEMENT framing, not architecture design.

