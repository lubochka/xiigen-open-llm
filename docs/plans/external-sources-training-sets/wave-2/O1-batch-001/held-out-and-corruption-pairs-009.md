# Held-Out and Corruption Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 009-H001 — write API contract for tenant management

A: "Write the API contract for the tenant management service including all CRUD (Create, Read, Update, Delete) endpoints."
Expected B: Predicates: ["DOCUMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: documentation
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  DOCUMENT: "write" (specification context), implicitly documentation intent
  API_CONTRACT: "api", "contract", "endpoints"
  AUTHORIZATION: "tenant"
  TransitionType: documentation (DOCUMENT fires; no explicit IMPLEMENT)

---

## Held-out pair 009-H002 — specify the permission API schema

A: "Specify the request and response schema for the permission management API."
Expected B: Predicates: ["DOCUMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: documentation
(not in training set — held out for evaluation)

---

## Corruption 009-C001 — v6-02 suppresses API_CONTRACT when "tenant" is present

v6 corrupt: "Write the API contract for the tenant management service."
  → Predicates: ["DOCUMENT", "AUTHORIZATION"] | TransitionType: documentation
  (API_CONTRACT suppressed — model thinks AUTHORIZATION from "tenant" overrides API_CONTRACT)
Correct: Predicates: ["DOCUMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: documentation
Error type: v6-02 (gap suppression — API_CONTRACT suppressed by co-present AUTHORIZATION)
Rule: predicates do not suppress each other; when both "tenant" and "api" appear, BOTH fire

---

## Corruption 009-C002 — v6-02 treats "contract" as AUTHORIZATION instead of API_CONTRACT

v6 corrupt: "Define the service contract for the tenant authorization module."
  → Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
  ("contract" not recognized as API_CONTRACT trigger; "authorization" fires AUTHORIZATION)
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: feature_addition
Error type: v6-02 (gap suppression — API_CONTRACT not fired on "contract")
Rule: "contract" IS in the API_CONTRACT lexicon; it fires regardless of domain context
