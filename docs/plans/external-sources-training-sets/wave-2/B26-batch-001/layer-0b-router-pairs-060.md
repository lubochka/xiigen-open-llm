# Layer 0-B — Router Pairs (060) — Complete Factory Routing Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R060-001 — Multi-topic stacking → 6.3 deterministic_generic

R060-001 test: "Audit the authentication service for CSRF, SSRF, XXE, SQL injection, and privilege escalation."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Five SECURITY triggers + dual predicate
  TrustState: deterministic_generic

---

## R060-002 — Audit + IMPLEMENT combo → 4.1 + 6.3

R060-002 test: "Audit the JWT signing algorithm and implement RS256 rotation."
  SECURITY → 6.3 | IMPLEMENT → 4.1 | TransitionType: feature_addition (IMPLEMENT present)

---

## R060-003 — "security review" as noun (no action object) → GapHandler

R060-003 test: "Discuss the results of the security review in the next sprint planning meeting."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "security review" as a past-event noun in discussion context is gap.

---

## R060-004 — Six-predicate routing completeness

R060-004 test: "Redesign the authentication architecture, implement bcrypt, audit RBAC for privilege escalation, document findings, and QA the security suite."
  ARCHITECTURE → 2.3 | IMPLEMENT → 4.1 | SECURITY → 6.3 | AUTHORIZATION → 6.3 | DOCUMENT → 6.2 | QA → 7.3
  AllSubPhaseIds: [2.3, 4.1, 6.3, 6.2, 7.3] | TransitionType: feature_addition

---

## R060-005 — Zero predicates boundary check

R060-005 test: "Make the entire platform more secure and ensure compliance with all regulations."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "more secure" + "ensure compliance" are both gap vocabulary. Zero predicates.
