# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-023-001 — "scaffold" fires IMPLEMENT (IaC context)

A: "Scaffold the Terraform module directory structure for the networking layer."
  "scaffold" → IMPLEMENT | "Terraform module directory structure" → IMPLEMENT (IaC code scaffold) | "networking layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Teaching: "scaffold" is an IMPLEMENT action verb. "layer" fires ARCHITECTURE as structural noun.

---

## B14-edge-023-002 — "write the Terraform" fires IMPLEMENT

A: "Write the Terraform resource definitions for the EKS node groups and the IAM (Identity and Access Management) role bindings."
  "write" → IMPLEMENT | "Terraform resource definitions" → IMPLEMENT (IaC code) | "IAM role bindings" → SECURITY ("credential" context — IAM = identity/credential binding)
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Teaching: "write" fires IMPLEMENT. "IAM role binding" fires SECURITY as a credential/identity binding. "EKS node groups" → gap.

---

## B14-edge-023-003 — "Dockerfile" scaffold fires IMPLEMENT

A: "Build the multi-stage Dockerfile for the gRPC (Remote Procedure Call) service and define the layer build dependencies."
  "build" → IMPLEMENT | "multi-stage Dockerfile" → IMPLEMENT (container code) | "define" → ARCHITECTURE | "layer build dependencies" → ARCHITECTURE ("layer", "dependency")
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Teaching: "build" fires IMPLEMENT. "define" fires ARCHITECTURE. "layer" and "dependency" reinforce ARCHITECTURE.

---

## B14-edge-023-004 — "configure" in IaC context fires IMPLEMENT

A: "Configure the Kubernetes RBAC (Role-Based Access Control) roles and bindings in the Helm chart values."
  "configure" → IMPLEMENT (concrete IaC configuration) | "RBAC roles and bindings" → SECURITY ("credential", access control) | "Helm chart values" → IMPLEMENT
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Teaching: "configure" in IaC/code artifact context fires IMPLEMENT. RBAC = access control = credential binding → SECURITY.
