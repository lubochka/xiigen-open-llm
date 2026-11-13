# Layer 0-A — Compiler Pairs (025)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-025-001 — ContainerRegistry: ARCHITECTURE + IMPLEMENT triggers

A: "Design the container registry topology and implement the image scanning pipeline."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  ARCHITECTURE: "design" + "topology" | IMPLEMENT: "implement" + "pipeline" (code artifact context)
  Gap: "container registry" (tool name is gap; "topology" fires ARCHITECTURE)

---

## B14-compiler-025-002 — ContainerRegistry: SECURITY triggers

A: "Enforce image signing and configure TLS for the registry endpoint."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "TLS" | "image signing" = credential/certificate context → SECURITY
  Gap: "configure" (operational UI action — no code artifact noun present)

---

## B14-compiler-025-003 — ContainerRegistry: API_CONTRACT trigger

A: "Define the registry API contract for image push and pull operations."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]
  ARCHITECTURE: "define" (structural context) | API_CONTRACT: "API contract" + "define" in contract context
  Teaching: "define the API contract" fires both; API_CONTRACT noun is the determining factor.

---

## B14-compiler-025-004 — ContainerRegistry: gap vocabulary

A: "Push the container image to ECR (Elastic Container Registry) and pull it into EKS."
  CompiledPredicateIds: []
  Gap: "push" (operational), "pull" (operational), "ECR" (platform name), "EKS" (platform name)
  TrustState: knowledge_edge_synthetic

---

## B14-compiler-025-005 — ContainerRegistry: IMPLEMENT boundary

A: "Write the Dockerfile and implement the multi-stage build pipeline for the registry."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "write" + "Dockerfile" (code artifact) | "implement" + "pipeline" (IaC/build artifact context)
  Gap: "multi-stage build" (pattern name alone is gap; "implement" fires IMPLEMENT)
