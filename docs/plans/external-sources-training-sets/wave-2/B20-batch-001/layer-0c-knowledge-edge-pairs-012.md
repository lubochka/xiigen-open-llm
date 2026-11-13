# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E012-001 — "parametrize" is gap; fires IMPLEMENT only if "build"/"write" present

A: "Parametrize the shipping cost tests for different carrier combinations."
  "parametrize" — gap (test framework keyword); "shipping cost tests" → TEST via "tests"
  CompiledPredicateIds: ["TEST"]
  Teaching: "parametrize" alone is gap. TEST fires on "tests". No IMPLEMENT predicate without an IMPLEMENT verb.

---

## E012-002 — "mock out" is gap; "write mock" fires IMPLEMENT+TEST

A: "Mock out the PaymentGateway in the OrderService unit tests."
  "mock out" — gap (setup verb, not IMPLEMENT); "unit tests" → TEST fires on "unit tests"
  CompiledPredicateIds: ["TEST"]
  Teaching: "mock out" is gap. TEST fires on "unit tests". Compare: "write a mock" → IMPLEMENT+TEST.

---

## E012-003 — "assert throws" is assertion — fires TEST

A: "Assert that RefundService throws InvalidOperationException when balance is zero."
  "assert" → TEST; "throws" — domain/language keyword; no additional predicate
  CompiledPredicateIds: ["TEST"]
  Teaching: "assert" fires TEST. "throws" is domain vocabulary, not a separate predicate trigger.

---

## E012-004 — "test plan" fires TEST; "create a test plan" fires IMPLEMENT+TEST

A: "Create a test plan for the Q3 release of the OrderModule."
  "create" → IMPLEMENT; "test plan" → TEST fires on "test"; "Q3 release" — gap (project vocabulary)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Teaching: "create" fires IMPLEMENT. "test plan" fires TEST via "test". Gap vocabulary ("Q3 release") doesn't suppress either.

---

## E012-005 — "verify the fix" fires TEST; "fix" alone does not fire IMPLEMENT

A: "Verify that the fix for the CartService null-reference bug is correct."
  "verify" → TEST; "fix" — gap (B20 rule: "fix" does not fire IMPLEMENT); "null-reference" — domain
  CompiledPredicateIds: ["TEST"]
  Teaching: "verify" fires TEST. "fix" does NOT fire IMPLEMENT. Canonical: fix → gap in the IMPLEMENT lexicon.
