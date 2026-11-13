# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E010-001 — "make testable" (adjective) is gap; "refactor" fires IMPLEMENT

A: "Refactor the LegacyOrderProcessor to make it unit testable."
  "refactor" → IMPLEMENT (TransitionType: refactoring); "unit testable" — gap (adjective; no TEST predicate)
  CompiledPredicateIds: ["IMPLEMENT"] only
  TransitionType: refactoring
  Teaching: "testable"/"unit testable" as an adjective goal does not fire TEST.

---

## E010-002 — "flaky test" fires TEST via "test"

A: "Fix the flaky test in the CartService that fails intermittently."
  "fix" — gap (not in IMPLEMENT lexicon); "flaky test" → TEST via "test"; "fails intermittently" — gap
  CompiledPredicateIds: ["TEST"]
  Teaching: "flaky test" fires TEST. "fix" does not fire IMPLEMENT.

---

## E010-003 — "happy path" is gap

A: "Cover the happy path for the CheckoutService order placement."
  "cover" — gap; "happy path" — gap (testing scenario terminology); "order placement" — domain
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001
  Teaching: "happy path" is testing vocabulary but not in the TEST predicate lexicon.

---

## E010-004 — "given/when/then" is BDD gap; TEST fires via "test" if present

A: "Given the UserService has an active session, When login is called again, Then return the existing token."
  "Given/When/Then" — BDD syntax; gap vocabulary; no "test" word present
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: Pure BDD syntax without "test" fires zero predicates. TEST requires the literal word.

---

## E010-005 — "error case" is gap; "test the error case" fires TEST

A: "Handle the error case in the RefundService when the original transaction is not found."
  "handle" — gap; "error case" — gap
  vs "Test the error case in the RefundService when the original transaction is not found."
  "test" → TEST
  Teaching: The verb determines the predicate. "handle" → gap. "test" → TEST.
  First sentence: CompiledPredicateIds: [] | Second: ["TEST"]
