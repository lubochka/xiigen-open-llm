# Held-Out and Corruption Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H003-001 — Held-out: "develop test-driven" — IMPLEMENT+TEST via two triggers

Request: "Develop the NotificationService test-driven with 100% branch coverage."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "develop" → IMPLEMENT; "test-driven" → TEST via "test"; "100% coverage" threshold is gap
  TransitionType: feature_addition

---

## H003-002 — Corruption v6-01: "testable" adjective triggers TEST (invention)

Corrupt output: "refactor to make it testable" → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
Correct output: CompiledPredicateIds: ["IMPLEMENT"]
  Corruption type: v6-01 (predicate invention — incorrectly firing TEST on adjective "testable")
  Fix: "testable" is an adjective goal, not a TEST predicate trigger.

---

## H003-003 — Corruption v6-01: "fix" fires IMPLEMENT (invention)

Corrupt output: "fix the failing test" → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
Correct output: CompiledPredicateIds: ["TEST"]
  Corruption type: v6-01 (predicate invention — "fix" incorrectly fires IMPLEMENT)
  Fix: "fix" is gap for IMPLEMENT. TEST fires on "test".

---

## H003-004 — Held-out: "validate schema" fires TEST; no API_CONTRACT from "schema" alone

Request: "Validate the request schema for the OrdersController endpoint."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "validate" → TEST (always); "schema" alone is gap for API_CONTRACT (needs REST/endpoint context verb)
  Note: "endpoint" is domain vocabulary here; doesn't fire API_CONTRACT without "integrate"/"contract"/"spec" verbs

---

## H003-005 — Corruption v6-03: multi-branch collapse — TEST+IMPLEMENT collapsed to IMPLEMENT only

Corrupt output: "write unit tests for the BillingService" → CompiledPredicateIds: ["IMPLEMENT"]
Correct output: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Corruption type: v6-03 (multi-branch result collapse — incorrectly dropping TEST when IMPLEMENT is present)
  Fix: Both predicates fire independently. Neither suppresses the other.
