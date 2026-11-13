# Layer 0-A — Compiler Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-003-001 — ARCHITECTURE: auto-scaling

A: "Design the auto-scaling architecture for the compute layer — define scale-out triggers based on CPU and request throughput."
  "design" → ARCHITECTURE | "auto-scaling architecture" → ARCHITECTURE | "compute layer" → ARCHITECTURE ("layer") | "define" → ARCHITECTURE (in architectural context, defines design decisions)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "CPU", "request throughput", "scale-out triggers" → operational metrics vocabulary, gap

---

## B14-compiler-003-002 — ARCHITECTURE: disaster recovery

A: "Architect the disaster recovery topology — multi-region failover with RTO (Recovery Time Objective) under 5 minutes."
  "architect" → ARCHITECTURE | "disaster recovery topology" → ARCHITECTURE | "multi-region failover" → ARCHITECTURE ("failover" structural pattern)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "RTO", "5 minutes", "multi-region" → operational/platform vocabulary, gap

---

## B14-compiler-003-003 — ARCHITECTURE+SECURITY: network segmentation

A: "Design the network segmentation architecture for the microservices platform — enforce zero-trust boundary policies."
  "design" → ARCHITECTURE | "network segmentation architecture" → ARCHITECTURE | "enforce" → SECURITY | "zero-trust" → SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Gap: "microservices platform" → platform vocabulary, gap

---

## B14-compiler-003-004 — ARCHITECTURE: container orchestration

A: "Structure the container orchestration layer — define pod resource limits, namespace boundaries, and rolling update strategy."
  "structure" → ARCHITECTURE | "container orchestration layer" → ARCHITECTURE | "define" → ARCHITECTURE (design decision framing) | "rolling update strategy" → ARCHITECTURE ("strategy")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "pod", "namespace", "container" → Kubernetes vocabulary, gap

---

## B14-compiler-003-005 — gap: "scale up the database"

A: "Scale up the database to handle more traffic."
  "scale up" → gap (operational action, not architectural design) | "database" → gap | "more traffic" → gap
  CompiledPredicateIds: [] → gap
  Teaching: "scale" as an operational verb is gap. "design the scaling architecture" fires ARCHITECTURE; "scale up" alone does not.

