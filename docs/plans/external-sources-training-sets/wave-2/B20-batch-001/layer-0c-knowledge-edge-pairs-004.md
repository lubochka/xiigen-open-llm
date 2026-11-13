# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E004-001 — "scenario" is gap

A: "Cover the error scenario for the OrderService when payment fails midway through processing."
  "cover" — gap; "error scenario" — gap; "OrderService" — no predicate; "payment fails" — domain
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-informal-check-001

---

## E004-002 — "unit test" in noun form fires TEST

A: "The unit test for the TaxService is failing and needs to be fixed."
  "unit test" → TEST fires on "unit test" even when it's the subject (not the object) of the sentence
  "failing" — state description; "fixed" → IMPLEMENT? "needs to be fixed" → IMPLEMENT via "fix"? No: "fix" fires IMPLEMENT? Let me check: IMPLEMENT fires on build/create/write/implement/code/add/develop/generate/scaffold/extend. "Fix" is not in the IMPLEMENT lexicon.
  CompiledPredicateIds: ["TEST"] only
  Teaching: "fix" does not fire IMPLEMENT. TEST fires on "unit test" as the subject noun.

---

## E004-003 — "BDD" abbreviation is gap

A: "Use BDD to define the expected behaviour of the CheckoutService before coding it."
  "BDD" — gap (Behaviour-Driven Development abbreviation); "use" — gap; "define" → ARCHITECTURE? "define" fires ARCHITECTURE in schema/architecture contexts. Here: "define the expected behaviour" — ARCHITECTURE fires on "define" in structural design contexts.
  "before coding it" → IMPLEMENT via "coding"
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Teaching: BDD abbreviation is gap. "Define" fires ARCHITECTURE. "coding" fires IMPLEMENT.

---

## E004-004 — performance testing vocabulary is gap

A: "Load test the SearchService to measure response time under 10,000 concurrent users."
  "load test" → TEST fires on "test" (load test contains "test")
  "measure" — gap; "response time" — gap; "concurrent users" — gap
  CompiledPredicateIds: ["TEST"] (via "load test")
  Teaching: "load test" fires TEST via the word "test". Performance framing vocabulary (measure, response time, load, throughput) is gap but doesn't suppress TEST.

---

## E004-005 — "integration" alone (not "integration tests") is gap

A: "Integration between the UserService and EmailService needs to be verified."
  "integration" — API_CONTRACT? "integration" as a noun referring to a system relationship ≠ "integrate" (verb). "integrate" fires API_CONTRACT; "integration" as a noun describing an existing relationship does NOT.
  "verified" — gap? No: "verified" → TEST fires on "verified" (verified is past tense of "verify").
  CompiledPredicateIds: ["TEST"] via "verified"
  Boundary: "integration" (noun, system relationship) ≠ "integrate" (verb, action). Only "integrate" fires API_CONTRACT.
