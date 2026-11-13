# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E009-001 — "test suite" fires TEST; "run the test suite" fires TEST (not gap)

A: "Run the test suite for the ShippingModule after the database schema change."
  "run" — gap (run as standalone action); "test suite" → TEST fires on "test"; "database schema change" — domain
  CompiledPredicateIds: ["TEST"] via "test suite"

---

## E009-002 — "in-memory database" is gap

A: "Set up an in-memory database for the repository unit tests."
  "set up" — gap; "in-memory database" — gap (infrastructure term); "unit tests" → TEST
  CompiledPredicateIds: ["TEST"] via "unit tests"
  Teaching: "set up" is gap (established in B28). TEST fires via "unit tests" regardless.

---

## E009-003 — "mutation testing" fires TEST via "testing"

A: "Apply mutation testing to the DiscountService to evaluate test suite effectiveness."
  "apply" — IMPLEMENT? "apply" alone → IMPLEMENT fires on "apply" (apply a change/patch = implementation).
  "mutation testing" → TEST fires on "testing"; "DiscountService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "apply" fires IMPLEMENT (apply is an IMPLEMENT-adjacent action verb). TEST fires via "testing".

---

## E009-004 — "code coverage" contains "coverage" (gap) not "test" — borderline

A: "Achieve 100% code coverage on the BillingService."
  "achieve" — gap; "code coverage" — "coverage" alone is gap; "100%" — threshold; gap
  Note: Does "code coverage" contain "test" via association? No — "coverage" is not "test". 
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "code coverage" does NOT fire TEST. Coverage is a metric, not a test action.

---

## E009-005 — "negative test case" fires TEST via "test case"

A: "Add a negative test case for the LoginService when credentials are invalid."
  "add" → IMPLEMENT; "negative test case" → TEST via "test case"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "negative test case" fires TEST. The adjective "negative" doesn't suppress the predicate.
