# Layer 0-A — Compiler Pairs (023)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-023-001 — ARCHITECTURE+IMPLEMENT: Terraform module scaffold

A: "Design the infrastructure module architecture and scaffold the Terraform modules for the VPC (Virtual Private Cloud) and compute layers."
  "design" → ARCHITECTURE | "infrastructure module architecture" → ARCHITECTURE | "scaffold" → IMPLEMENT (scaffold is an IMPLEMENT action verb) | "Terraform modules" → IMPLEMENT | "VPC and compute layers" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "Terraform", "VPC" → tool/platform vocabulary, gap

---

## B14-compiler-023-002 — IMPLEMENT+ARCHITECTURE: Helm chart creation

A: "Create the Helm chart structure for the microservices deployment and configure the values hierarchy."
  "create" → IMPLEMENT | "Helm chart structure" → IMPLEMENT (code artifact) | "configure" → IMPLEMENT | "values hierarchy" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Gap: "Helm chart", "microservices deployment" → tool/platform vocabulary, gap

---

## B14-compiler-023-003 — IMPLEMENT+ARCHITECTURE: Ansible playbook

A: "Write the Ansible playbook to provision the Kubernetes worker nodes and define the node group topology."
  "write" → IMPLEMENT | "Ansible playbook" → IMPLEMENT (IaC script) | "provision" → IMPLEMENT | "define" → ARCHITECTURE | "node group topology" → ARCHITECTURE ("topology")
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Gap: "Ansible playbook", "Kubernetes worker nodes" → tool/platform vocabulary, gap

---

## B14-compiler-023-004 — IMPLEMENT: Dockerfile scaffold

A: "Build the multi-stage Dockerfile for the API (Application Programming Interface) service and configure the runtime layer dependencies."
  "build" → IMPLEMENT | "multi-stage Dockerfile" → IMPLEMENT (IaC/containerization code) | "configure" → IMPLEMENT | "runtime layer dependencies" → ARCHITECTURE ("layer", "dependency")
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Gap: "Dockerfile", "multi-stage" → tool vocabulary, gap

---

## B14-compiler-023-005 — IMPLEMENT+ARCHITECTURE+SECURITY: secure IaC

A: "Implement the Terraform network module and ensure the TLS (Transport Layer Security) certificate configuration and secrets injection are baked into the infrastructure topology."
  "implement" → IMPLEMENT | "Terraform network module" → IMPLEMENT | "TLS certificate configuration" → SECURITY ("tls", "certificate") | "secrets injection" → SECURITY ("secret") | "infrastructure topology" → ARCHITECTURE ("topology", "infrastructure")
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
  Gap: "Terraform", "baked into" → tool/colloquial vocabulary, gap
