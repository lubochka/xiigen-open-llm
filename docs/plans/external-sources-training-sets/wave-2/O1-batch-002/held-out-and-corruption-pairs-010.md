# Held-Out and Corruption Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-010-001 — Held-out: "refactor the OAuth token service"

A: "refactor the OAuth token service to use the new key management infrastructure"

B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: refactoring
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]
   Reasoning: "refactor" verb → TransitionType: refactoring (overrides feature_addition). "oauth" fires SECURITY; "token" fires SECURITY; "key" fires SECURITY; "implement" implicit in "refactor". Two routing decisions.

---

## O1b-HO-010-002 — Held-out: "refactor tenant scope storage"

A: "refactor the tenant scope storage layer to use a distributed cache"

B: Predicates: ["AUTHORIZATION", "IMPLEMENT"] | TransitionType: refactoring
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]
   Reasoning: "refactor" → TransitionType: refactoring; "tenant" fires AUTHORIZATION; "scope" fires AUTHORIZATION.

---

## O1b-HO-010-003 — Held-out: "refactor auth middleware"

A: "refactor the authentication middleware to support pluggable token validators"

B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: refactoring
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

---

## O1b-COR-010-001 — Corruption v6-03: wrong TransitionType for refactoring

A: "refactor the OAuth token validation service"

CORRUPTED B (v6-03): Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
   ERROR: "refactor" verb should override feature_addition and set TransitionType: refactoring. The model missed the verb-based TransitionType override.

CORRECT B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: refactoring

---

## O1b-COR-010-002 — Corruption v6-03: "refactor" misidentified as feature_addition

A: "refactor the tenant scope enforcement layer to improve performance"

CORRUPTED B (v6-03): TransitionType: feature_addition
   ERROR: "refactor" verb detected in prompt → TransitionType must be refactoring. feature_addition applies only when "refactor" is absent.

CORRECT B: Predicates: ["AUTHORIZATION"] | TransitionType: refactoring
