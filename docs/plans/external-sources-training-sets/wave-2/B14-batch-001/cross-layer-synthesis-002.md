# Cross-Layer Synthesis (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S002 — End-to-end trace: "Architect the IaC pipeline and implement the Terraform modules"

### Layer 0-A
Input: "Architect the IaC (Infrastructure as Code) pipeline — implement the Terraform modules for VPC, compute, and database layers."
  "architect" → ARCHITECTURE | "IaC pipeline" → ARCHITECTURE ("IaC" + "pipeline") | "implement" → IMPLEMENT | "Terraform modules" → IMPLEMENT | "VPC, compute, database layers" → ARCHITECTURE ("layers")
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

### Layer 0-B
  ARCHITECTURE → SubPhase 3.5 | IMPLEMENT → SubPhase 4.1
  DecisionSet: [ArchitectureDecision(3.5), ImplementDecision(4.1)]

### Layer 0-D
  FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT"]) → [InfrastructureArchitectureFlow35, ImplementationFlow41]

### Layer 1
  TransitionType: feature_addition (IMPLEMENT verb present) | Sequential: ARCHITECTURE → IMPLEMENT

### Layer 2 (InfrastructureArchitectureFlow35)
  VPC topology: /16 CIDR with /24 subnets per AZ | Compute: autoscaling group, 3 AZs | DB: primary + replica
  Node 7: PASS

### Layer 3
  Teaching: "architect" fires ARCHITECTURE. "implement Terraform modules" fires IMPLEMENT. "IaC" fires ARCHITECTURE. IMPLEMENT drives feature_addition TransitionType.

### Cross-layer invariant
  IaC workflows always produce ARCHITECTURE+IMPLEMENT. IMPLEMENT drives feature_addition. Both flows created; neither suppresses the other.

---

## S002-B — Contrast: "plan the IaC pipeline"

### Layer 0-A
  "plan" → gap | "IaC pipeline" → ARCHITECTURE? → "plan" without design verb → gap
  CompiledPredicateIds: [] → gap
  Teaching: "plan the IaC pipeline" is gap. ARCHITECTURE requires design/architect/structure, not "plan".

