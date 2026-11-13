# Layer 0-B — Router Pairs (038)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-038-001 — SpotInstances: routing for ARCHITECTURE → 3.5

"Architect the spot instance fleet topology for compute workloads."
  ARCHITECTURE → 3.5 (infra vocabulary: "fleet", "topology", "compute")
  TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-038-002 — SpotInstances: routing for IMPLEMENT + SECURITY

"Write the spot node group Terraform module and enforce IMDSv2 (Instance Metadata Service v2) security."
  IMPLEMENT → 4.1 | SECURITY → 6.3
  Sequential: 6.3 → 4.1 (security validated before implementation) | Wait — standard ordering is IMPLEMENT then SECURITY validation? No: Sequential: ARCHITECTURE → SECURITY → IMPLEMENT; so 6.3 before 4.1 only if ARCHITECTURE absent.
  Correction: When ARCHITECTURE is absent, ordering defaults to SECURITY (6.3) → IMPLEMENT (4.1).
  TransitionType: feature_addition | AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-038-003 — SpotInstances: routing for ARCHITECTURE + IMPLEMENT + SECURITY + DOCUMENT

"Design the spot topology, implement Terraform modules, enforce IMDSv2, and document the ADR."
  ARCHITECTURE → 3.5 | IMPLEMENT → 4.1 | SECURITY → 6.3 | DOCUMENT → 6.2
  Sequential: 3.5 → 6.3 → 4.1 → 6.2 | TransitionType: feature_addition
  AllSubPhaseIds: [3.5, 4.1, 6.3, 6.2]

---

## B14-router-038-004 — SpotInstances: gap routing

"Use spot instances for the batch job workload."
  Zero predicates → GapHandler | CapabilityGapWarnings: gap-devops-cloud-001
  "use" is operational gap; "spot instances" is gap platform vocabulary
