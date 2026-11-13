# Layer 0-A — Compiler Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-005-001 — ARCHITECTURE: service mesh

A: "Design the service mesh architecture — define sidecar proxy configuration, circuit breaker patterns, and mTLS (mutual TLS) policy."
  "design" → ARCHITECTURE | "service mesh architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "circuit breaker patterns" → ARCHITECTURE ("patterns") | "mTLS policy" → ARCHITECTURE ("policy" as structural constraint) + SECURITY ("mTLS" = security protocol)
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Teaching: mTLS fires SECURITY even in architecture context. ARCHITECTURE and SECURITY are independent.

---

## B14-compiler-005-002 — ARCHITECTURE: data tier

A: "Architect the data tier — design the primary/read-replica topology, partitioning strategy, and cross-region replication."
  "architect" → ARCHITECTURE | "data tier" → ARCHITECTURE ("tier") | "design" → ARCHITECTURE | "topology" → ARCHITECTURE | "partitioning strategy" → ARCHITECTURE ("strategy") | "cross-region replication" → ARCHITECTURE (structural pattern)
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-005-003 — ARCHITECTURE+QA: sign-off

A: "Design the infrastructure architecture for the staging environment and get QA sign-off on the network topology."
  "design" → ARCHITECTURE | "infrastructure architecture" → ARCHITECTURE | "QA sign-off" → QA | "network topology" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "QA"]

---

## B14-compiler-005-004 — ARCHITECTURE: deployment pipeline

A: "Structure the CI/CD (Continuous Integration/Continuous Deployment) pipeline architecture — define the promotion gates between environments."
  "structure" → ARCHITECTURE | "CI/CD pipeline architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "promotion gates" → ARCHITECTURE ("gates" as structural checkpoints)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "CI/CD", "environments" → DevOps vocabulary, gap

---

## B14-compiler-005-005 — gap: "set up Kubernetes"

A: "Set up Kubernetes for our staging environment."
  "set up" → gap (ARCHITECTURE requires design/architect/structure/blueprint/diagram) | "Kubernetes" → gap (platform name) | "staging environment" → gap
  CompiledPredicateIds: [] → gap
  Teaching: Platform configuration requests ("set up", "configure", "deploy") are gap unless paired with architecture framing verbs.

