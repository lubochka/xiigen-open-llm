# Layer 0-B — Router Pairs (052)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-052-001 — PodSecurityPolicy: routing for ARCHITECTURE + SECURITY

"Architect the pod security standard model and enforce the restricted profile."
  ARCHITECTURE → 3.5 | SECURITY → 6.3 (from "enforce" + "restricted" security vocabulary)
  Sequential: 3.5 → 6.3 | TransitionType: feature_addition | AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-052-002 — PodSecurityPolicy: routing for IMPLEMENT + SECURITY + AUTHORIZATION

"Implement the PSA manifest, enforce restricted profile, and grant usage to service accounts."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | AUTHORIZATION → auth sub-phase
  Sequential: 6.3 → AUTHORIZATION → 4.1 | TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 6.3, auth]

---

## B14-router-052-003 — PodSecurityPolicy: routing for ARCHITECTURE + SECURITY + IMPLEMENT

"Design the pod security model, enforce the restricted profile, and implement the admission configuration."
  ARCHITECTURE → 3.5 | SECURITY → 6.3 | IMPLEMENT → 4.1
  Sequential: 3.5 → 6.3 → 4.1 | TransitionType: feature_addition
  AllSubPhaseIds: [3.5, 6.3, 4.1]

---

## B14-router-052-004 — PodSecurityPolicy: gap routing

"Enable privileged mode for the kube-system namespace."
  Zero predicates → GapHandler | "enable" is operational gap; "privileged mode" is gap runtime config
  CapabilityGapWarnings: gap-devops-cloud-001
