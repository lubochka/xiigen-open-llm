# Layer 0-D — Flow Factory Pairs (060)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-060-001 — Complete factory reference: FlowFactory for SECURITY alone

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (SECURITY / 6.3) | TrustState: deterministic_generic
  Triggered by: harden, enforce (security noun), rotate (key/cert), zero-trust compound phrase, mTLS, TLS termination, encryption at rest/in transit
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-060-002 — Complete factory reference: FlowFactory for AUTHORIZATION alone

FlowFactory.Create("AUTHORIZATION"):
  → AuthorizationFlow (AUTHORIZATION / authorization sub-phase) | TrustState: deterministic_generic
  Triggered by: grant, revoke, permit (access), restrict (access), authorize, ClusterRole, RoleBinding, service account binding, trust policy, permission boundary
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-060-003 — Complete factory reference: TransitionType master rule

feature_addition: IMPLEMENT present (overrides all) OR ARCHITECTURE alone with design verb OR SECURITY alone OR AUTHORIZATION alone OR API_CONTRACT alone
documentation: DOCUMENT present AND IMPLEMENT absent (regardless of other predicates)
unknown: zero predicates → GapHandler
Never: SECURITY or AUTHORIZATION drive documentation TransitionType. Only DOCUMENT drives documentation.

---

## B14-factory-060-004 — Complete factory reference: FlowFactory invariants

Invariant 1: No cap on FlowCount — one flow per fired predicate.
Invariant 2: All flows have TrustState deterministic_generic when at least one predicate fires.
Invariant 3: TrustState knowledge_edge_synthetic only for GapHandler (zero predicate) cases.
Invariant 4: Sequential ordering is always 3.5 → 2.1 → 6.3 → AUTHORIZATION → 4.1 → 5.1 → 6.2 → 7.3 (omitting absent predicates).
Invariant 5: DOCUMENT last in sequential ordering — always records decisions made in all prior phases.
