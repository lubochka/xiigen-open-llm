# Layer 0-A — Compiler Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-017-001 — ARCHITECTURE: auto-scaling topology

A: "Design the auto-scaling architecture — HPA (Horizontal Pod Autoscaler), VPA (Vertical Pod Autoscaler), and cluster autoscaler interaction model."
  "design" → ARCHITECTURE | "auto-scaling architecture" → ARCHITECTURE | "interaction model" → ARCHITECTURE ("pattern", "model")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "HPA", "VPA", "cluster autoscaler" → platform vocabulary, gap

---

## B14-compiler-017-002 — ARCHITECTURE+IMPLEMENT: scaling policy

A: "Architect the capacity scaling topology and implement the scaling policies for the compute tier."
  "architect" → ARCHITECTURE | "capacity scaling topology" → ARCHITECTURE ("topology") | "implement" → IMPLEMENT | "scaling policies" → IMPLEMENT (concrete policy definition)
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "compute tier" → operational vocabulary, gap

---

## B14-compiler-017-003 — ARCHITECTURE: capacity planning model

A: "Define the capacity planning model — peak load projections, headroom calculations, and scaling thresholds for the infrastructure layer."
  "define" → ARCHITECTURE | "capacity planning model" → ARCHITECTURE ("model") | "scaling thresholds" → ARCHITECTURE | "infrastructure layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "peak load projections", "headroom calculations" → operational planning vocabulary, gap

---

## B14-compiler-017-004 — ARCHITECTURE+DOCUMENT: scaling ADR

A: "Design the scaling strategy for the event-processing cluster and document the scale-out triggers and cooldown periods as an ADR."
  "design" → ARCHITECTURE | "scaling strategy" → ARCHITECTURE | "event-processing cluster" → ARCHITECTURE ("cluster") | "document" → DOCUMENT | "ADR" → ARCHITECTURE ("ADR")
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "scale-out triggers", "cooldown periods" → configuration vocabulary, gap

---

## B14-compiler-017-005 — gap: "scale the pods"

A: "Scale the order-service pods to 10 replicas."
  "scale" → gap (operational action) | "pods" → gap | "10 replicas" → gap
  CompiledPredicateIds: []
  Teaching: "scale" as operational action is gap. ARCHITECTURE requires design/architect/define framing: "design the scaling architecture for the order-service."
