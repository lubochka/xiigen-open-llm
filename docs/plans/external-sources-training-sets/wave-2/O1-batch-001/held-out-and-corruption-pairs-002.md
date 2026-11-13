# Held-Out and Corruption Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 002-H001 — tenant-scoped JWT validation

A: "Implement tenant-scoped JWT validation middleware that enforces per-tenant key sets."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  IMPLEMENT: "implement"
  SECURITY: "jwt", "validate" (security context), "key"
  AUTHORIZATION: "tenant"

---

## Held-out pair 002-H002 — tenant-scoped JWT with role binding

A: "Add tenant-aware JWT role binding to the existing claims extraction pipeline."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

---

## Corruption 002-C001 — v6-01 invents JWT_AUTH predicate

v6 corrupt: "Implement tenant-scoped JWT validation."
  → Predicates: ["JWT_AUTH", "TENANT_ISOLATION"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
Error type: v6-01 (predicate invention)
Invented predicates: JWT_AUTH, TENANT_ISOLATION — neither exists in the O1 predicate lexicon
Correct predicates: only the 9 canonical predicates are valid (SECURITY, AUTHORIZATION, API_CONTRACT, IMPLEMENT, TEST, DOCUMENT, ARCHITECTURE, QA, UI)

---

## Corruption 002-C002 — v6-01 invents TenantJwtFlow factory

v6 corrupt: For the above prompt, also invents factory name "TenantJwtFlowAdapter"
  → Factory: TenantJwtFlowAdapter | TrustState: trusted_learned
Correct: Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned
Error type: v6-01 (factory name invention)
