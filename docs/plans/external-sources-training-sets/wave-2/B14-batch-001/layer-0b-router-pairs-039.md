# Layer 0-B — Router Pairs (039)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-039-001 — ReservedCapacity: routing for ARCHITECTURE → 3.5

"Architect the reserved capacity model for compute tier allocation."
  ARCHITECTURE → 3.5 (infra vocabulary: "capacity", "compute tier", "model" in infra context)
  TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-039-002 — ReservedCapacity: routing for AUTHORIZATION alone

"Grant the billing IAM role for reserved instance management."
  AUTHORIZATION fires → authorization sub-phase
  TransitionType: feature_addition | TrustState: deterministic_generic
  Note: AUTHORIZATION alone still produces feature_addition (creating a new role binding = net-new artifact)

---

## B14-router-039-003 — ReservedCapacity: routing for ARCHITECTURE + AUTHORIZATION + DOCUMENT

"Design the reserved capacity model, authorize the capacity-admin role, and document the ADR."
  ARCHITECTURE → 3.5 | AUTHORIZATION → authorization sub-phase | DOCUMENT → 6.2
  Sequential: 3.5 → AUTHORIZATION → 6.2 (DOCUMENT last, design before role definition)
  TransitionType: documentation (IMPLEMENT absent; DOCUMENT present with ARCHITECTURE)
  Wait: AUTHORIZATION present — does AUTHORIZATION drive feature_addition? Answer: No. AUTHORIZATION alone does not override documentation. Only IMPLEMENT overrides.
  TransitionType: documentation (ARCHITECTURE + DOCUMENT without IMPLEMENT)

---

## B14-router-039-004 — ReservedCapacity: gap routing

"Buy savings plans for the production EC2 (Elastic Compute Cloud) fleet."
  Zero predicates → GapHandler | "buy", "savings plans", "EC2" are all gap
  CapabilityGapWarnings: gap-devops-cloud-001
