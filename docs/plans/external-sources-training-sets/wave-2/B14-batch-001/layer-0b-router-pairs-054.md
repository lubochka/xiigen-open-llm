# Layer 0-B — Router Pairs (054)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-054-001 — OPAGatekeeper: routing for ARCHITECTURE → 3.5

"Architect the OPA (Open Policy Agent) constraint framework for the cluster."
  ARCHITECTURE → 3.5 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-054-002 — OPAGatekeeper: routing for IMPLEMENT + SECURITY

"Write the Rego policy module and enforce security constraints on all namespaces."
  IMPLEMENT → 4.1 | SECURITY → 6.3
  Sequential: 6.3 → 4.1 | TransitionType: feature_addition | AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-054-003 — OPAGatekeeper: routing for AUTHORIZATION + IMPLEMENT

"Grant OPA controller ClusterRole and implement the ConstraintTemplate manifest."
  AUTHORIZATION → auth sub-phase | IMPLEMENT → 4.1
  Sequential: AUTHORIZATION → 4.1 | TransitionType: feature_addition

---

## B14-router-054-004 — OPAGatekeeper: routing disambiguation — "policy" context

"Implement the OPA policy module." → IMPLEMENT → 4.1 (policy module = IaC code artifact)
"Enforce OPA security policies." → SECURITY → 6.3 (enforce + security context)
"Restrict namespace access via OPA policy." → AUTHORIZATION (restrict in access context)
"Design the OPA policy framework." → ARCHITECTURE → 3.5 (design + framework in infra context)
Same noun "policy" routes differently based on verb and context — always determined by trigger verb.
