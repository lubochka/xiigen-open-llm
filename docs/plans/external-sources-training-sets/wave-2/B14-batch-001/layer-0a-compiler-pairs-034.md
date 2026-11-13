# Layer 0-A — Compiler Pairs (034)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-034-001 — TerraformModules: IMPLEMENT trigger (primary)

A: "Write the Terraform modules for the VPC (Virtual Private Cloud) networking layer and the EKS node groups."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "write" + "Terraform modules" (IaC code artifact) | "modules" = code artifact noun
  Gap: "VPC" (platform name), "EKS" (platform name), "Terraform" (tool name alone without write/implement)

---

## B14-compiler-034-002 — TerraformModules: ARCHITECTURE + IMPLEMENT

A: "Design the Terraform module hierarchy and implement the reusable networking and compute modules."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  ARCHITECTURE: "design" + "hierarchy" + "module" (structural context) | IMPLEMENT: "implement" + "modules" (IaC artifact)
  Teaching: "module hierarchy" fires ARCHITECTURE (structural noun); "implement modules" fires IMPLEMENT (code artifact)

---

## B14-compiler-034-003 — TerraformModules: DOCUMENT trigger

A: "Document the Terraform module API reference and diagram the module dependency graph."
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  DOCUMENT: "document" + "module API reference" | ARCHITECTURE: "diagram" + "dependency graph" (structural)
  TransitionType: documentation (ARCHITECTURE + DOCUMENT, no IMPLEMENT)

---

## B14-compiler-034-004 — TerraformModules: SECURITY + IMPLEMENT

A: "Write the Terraform IAM (Identity and Access Management) policy modules and enforce encryption at rest."
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  IMPLEMENT: "write" + "IAM policy modules" (code artifact) | SECURITY: "IAM" (security binding context) + "encryption at rest"
  TransitionType: feature_addition

---

## B14-compiler-034-005 — TerraformModules: gap vocabulary

A: "Plan the Terraform run and apply the infrastructure changes."
  CompiledPredicateIds: []
  Gap: "plan" (gap — ARCHITECTURE requires design/architect/structure/blueprint) | "apply" (operational) | "Terraform" (tool name)
  Teaching: "plan" does NOT fire ARCHITECTURE. Only design/architect/structure/blueprint/diagram fire ARCHITECTURE.
