# Layer 0-B — Router Pairs (051)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-051-001 — NetworkPolicy: routing for ARCHITECTURE → 3.5

"Architect the NetworkPolicy topology for zero-trust namespace isolation."
  ARCHITECTURE → 3.5 | SECURITY fires from "zero-trust" → 6.3
  Both fire from the same request: ARCHITECTURE → 3.5, SECURITY → 6.3
  Sequential: 3.5 → 6.3 | TransitionType: feature_addition

---

## B14-router-051-002 — NetworkPolicy: routing for IMPLEMENT + SECURITY

"Implement the Kubernetes NetworkPolicy manifest and enforce deny-all baseline."
  IMPLEMENT → 4.1 | SECURITY → 6.3
  Sequential: 6.3 → 4.1 | TransitionType: feature_addition | AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-051-003 — NetworkPolicy: routing for AUTHORIZATION alone

"Permit ingress from the monitoring namespace to the app namespace."
  AUTHORIZATION → auth sub-phase | TransitionType: feature_addition | TrustState: deterministic_generic
  Note: "permit" in access policy context = AUTHORIZATION.

---

## B14-router-051-004 — NetworkPolicy: gap routing

"Apply the NetworkPolicy YAML to the cluster."
  Zero predicates → GapHandler | "apply" in kubectl context = operational gap; "YAML" is gap
  CapabilityGapWarnings: gap-devops-cloud-001

"Check network connectivity between pods."
  Zero predicates → GapHandler | "check" is operational gap
