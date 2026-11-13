# Layer 0-A — Compiler Pairs (019)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-019-001 — ARCHITECTURE: multi-region topology

A: "Design the multi-region deployment topology — active-active with latency-based routing and data residency compliance."
  "design" → ARCHITECTURE | "multi-region deployment topology" → ARCHITECTURE ("topology", "deployment") | "active-active" → ARCHITECTURE ("pattern") | "data residency" → ARCHITECTURE ("boundary")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "latency-based routing", "data residency compliance" → operational/legal vocabulary, gap

---

## B14-compiler-019-002 — ARCHITECTURE+IMPLEMENT: region failover config

A: "Architect the multi-region failover topology and implement the DNS (Domain Name System) health check routing configuration."
  "architect" → ARCHITECTURE | "multi-region failover topology" → ARCHITECTURE ("topology") | "implement" → IMPLEMENT | "DNS health check routing configuration" → IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "DNS", "health check routing" → operational/tool vocabulary, gap

---

## B14-compiler-019-003 — ARCHITECTURE+DOCUMENT: region boundary ADR

A: "Define the regional boundary architecture and document the data sovereignty decisions as ADRs."
  "define" → ARCHITECTURE | "regional boundary architecture" → ARCHITECTURE ("boundary", "architecture") | "document" → DOCUMENT | "ADRs" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "data sovereignty" → legal/compliance vocabulary, gap

---

## B14-compiler-019-004 — ARCHITECTURE: global load balancing pattern

A: "Structure the global load balancing topology — Anycast routing, health-based region failover, and latency-weighted distribution."
  "structure" → ARCHITECTURE | "global load balancing topology" → ARCHITECTURE ("topology") | "Anycast routing" → ARCHITECTURE ("pattern") | "health-based region failover" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "Anycast", "latency-weighted" → network/operational vocabulary, gap

---

## B14-compiler-019-005 — gap: "deploy to eu-west-1"

A: "Deploy the application to the eu-west-1 region."
  "deploy" → gap (operational deployment action; deploy as a standalone verb without IaC/scripting context is gap) | "eu-west-1" → gap | "application" → gap
  CompiledPredicateIds: []
  Teaching: "deploy" as a standalone operational action is gap. IMPLEMENT fires when deploy is in a coding/scripting/IaC context: "implement the deployment pipeline for eu-west-1."
