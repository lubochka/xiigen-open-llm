# Held-Out and Corruption Pairs (007)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 007-H001 — secure the API key rotation endpoint

A: "Secure the API key rotation endpoint with rate limiting and HMAC (Hash-based Message Authentication Code) signature verification."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "API_CONTRACT"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  IMPLEMENT: "secure" (as action — structural hardening), implicitly "add"
  SECURITY: "secure", "hmac", "hash"
  API_CONTRACT: "api", "endpoint"

---

## Held-out pair 007-H002 — add TLS to the tenant webhook endpoint

A: "Add TLS (Transport Layer Security) certificate pinning to the tenant webhook delivery endpoint."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

---

## Corruption 007-C001 — v6-03: SECURITY+AUTHORIZATION collapse to wrong sub-phase (two 3.4 decisions)

v6 corrupt: "Implement tenant role enforcement for JWT-validated API calls."
  → RoutingDecisions: [4.1(IMPLEMENT), 3.3(API_CONTRACT), 3.4(SECURITY), 3.4(AUTHORIZATION)]
  — 4 decisions with duplicate 3.4
Correct: RoutingDecisions: [4.1(IMPLEMENT), 3.3(API_CONTRACT), 3.4(SECURITY+AUTHORIZATION)]
  — 3 decisions, SECURITY+AUTHORIZATION deduplicated into one 3.4 decision
Error type: v6-03 (wrong routing — SECURITY and AUTHORIZATION not deduplicated)

---

## Corruption 007-C002 — v6-03: SECURITY+AUTHORIZATION routed to wrong sub-phase

v6 corrupt: "Add tenant role-based JWT access control."
  → AUTHORIZATION → 4.5 (wrong sub-phase selection)
Correct: AUTHORIZATION → 3.4
Error type: v6-03 (wrong routing — AUTHORIZATION mapped to wrong sub-phase)
Rule: AUTHORIZATION always routes to 3.4 in O1; never to 4.5 or any other sub-phase
