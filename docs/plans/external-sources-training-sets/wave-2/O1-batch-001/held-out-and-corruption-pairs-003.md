# Held-Out and Corruption Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 003-H001 — OAuth2 client credentials flow

A: "Add OAuth2 (Open Authorization 2.0) client credentials flow for machine-to-machine tenant API access."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  IMPLEMENT: "add"
  SECURITY: "oauth"
  AUTHORIZATION: "tenant"
  API_CONTRACT: "api"

---

## Held-out pair 003-H002 — OIDC discovery endpoint

A: "Implement the OIDC (OpenID Connect) discovery endpoint for the tenant authorization server."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

---

## Corruption 003-C001 — v6-02 suppresses SECURITY on "login reset"

v6 corrupt: "Add a login reset flow for forgotten account credentials."
  → Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
  ("credential" fires SECURITY; "login" and "reset" do not fire any predicate;
   but "credential" is present → SECURITY must fire)
Error type: v6-02 (gap suppression)
v6-02 error: model sees "login reset" as the dominant frame and suppresses SECURITY
  even though "credentials" is present and fires SECURITY

---

## Corruption 003-C002 — v6-02 fires SECURITY on "login" alone (inverse gap suppression)

v6 corrupt: "Fix the login page redirect loop."
  → Predicates: ["SECURITY"] | TransitionType: feature_addition
Correct: Predicates: [] | TransitionType: none | KnowledgeEdge: true
Error type: v6-02 (gap suppression — firing where should not)
Explanation: "login" is not in the SECURITY lexicon; no predicate should fire
