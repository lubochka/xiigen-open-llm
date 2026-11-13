# Layer 3 — Teaching Unit Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T013-001 — EKS cluster topology: "cluster topology" fires ARCHITECTURE

A: "Does 'Design the EKS cluster topology' fire ARCHITECTURE?"
B: Yes. "design" → ARCHITECTURE. "cluster topology" → ARCHITECTURE via "topology". "EKS" → gap (platform name). Gap does not suppress ARCHITECTURE. CompiledPredicateIds: ["ARCHITECTURE"]. Route to 3.5.

---

## T013-002 — Namespace boundary fires ARCHITECTURE

A: "Define the namespace boundary isolation." Does ARCHITECTURE fire?
B: Yes. "define" → ARCHITECTURE. "namespace boundary isolation" → ARCHITECTURE via "boundary". "namespace" alone is gap (Kubernetes vocabulary) but "boundary" fires ARCHITECTURE. CompiledPredicateIds: ["ARCHITECTURE"].

---

## T013-003 — "upgrade the cluster" is gap

A: "Upgrade the EKS cluster to Kubernetes 1.29." Any predicates?
B: Zero predicates. "upgrade" is operational maintenance gap. "EKS cluster" and "Kubernetes 1.29" are platform vocabulary gap. Result: [] → knowledge_edge. To fire IMPLEMENT: "implement the cluster version migration procedure."

---

## T013-004 — "node group structure" fires ARCHITECTURE via "structure"

A: "Architect the node group structure for dedicated workload pools." Predicates?
B: ["ARCHITECTURE"]. "architect" → ARCHITECTURE. "node group structure" → ARCHITECTURE via "structure" (structural noun). "dedicated workload pools" → gap but doesn't suppress. Route to 3.5.

---

## T013-005 — ARCHITECTURE+IMPLEMENT for control plane IaC

A: "Design the EKS control plane architecture and implement the Terraform node group configuration." Result?
B: ["ARCHITECTURE", "IMPLEMENT"]. "design" + "control plane architecture" → ARCHITECTURE. "implement" + "Terraform node group configuration" → IMPLEMENT. TransitionType: feature_addition (IMPLEMENT present). Two flows: 3.5 + 4.1.
