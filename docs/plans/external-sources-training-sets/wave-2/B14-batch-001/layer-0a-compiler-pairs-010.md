# Layer 0-A — Compiler Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-010-001 — ARCHITECTURE: hybrid cloud

A: "Architect the hybrid cloud topology — define the connectivity layer between on-premises data centre and cloud VPCs (Virtual Private Clouds)."
  "architect" → ARCHITECTURE | "hybrid cloud topology" → ARCHITECTURE | "define" → ARCHITECTURE | "connectivity layer" → ARCHITECTURE ("layer") | "VPCs" → ARCHITECTURE (network boundary units)
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-010-002 — ARCHITECTURE+DOCUMENT: platform diagram

A: "Design the platform architecture and produce a C4 (Context, Containers, Components, Code) diagram showing all infrastructure boundaries and service interactions."
  "design" → ARCHITECTURE | "platform architecture" → ARCHITECTURE | "C4 diagram" → ARCHITECTURE ("diagram") + DOCUMENT | "infrastructure boundaries" → ARCHITECTURE ("boundaries") | "service interactions" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]

---

## B14-compiler-010-003 — ARCHITECTURE: rate limiting layer

A: "Structure the rate limiting and throttling layer — define per-consumer quotas, burst capacity, and backpressure propagation rules."
  "structure" → ARCHITECTURE | "rate limiting and throttling layer" → ARCHITECTURE ("layer") | "define" → ARCHITECTURE | "burst capacity" → ARCHITECTURE (capacity design constraint) | "backpressure propagation rules" → ARCHITECTURE ("rules", "propagation" as structural flow)
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-010-004 — ARCHITECTURE+IMPLEMENT: logging pipeline

A: "Design the centralised log aggregation architecture and implement the log shipping agents for all microservice containers."
  "design" → ARCHITECTURE | "centralised log aggregation architecture" → ARCHITECTURE | "implement" → IMPLEMENT | "log shipping agents" → IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

---

## B14-compiler-010-005 — gap: "increase memory for the pods"

A: "Increase the memory limit for all pods in the production namespace."
  "increase" → gap | "memory limit" → gap (operational tuning, not architectural design) | "pods" → gap | "production namespace" → gap
  CompiledPredicateIds: [] → gap
  Teaching: Operational tuning requests (increase/decrease resource limits, restart pods) are gap. Architecture design asks: design, architect, structure, blueprint.

