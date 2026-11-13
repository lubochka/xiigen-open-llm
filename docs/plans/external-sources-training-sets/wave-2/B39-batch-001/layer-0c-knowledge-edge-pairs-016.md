# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E016-001 — "scenario" fires TEST in BDD context

A: "Write BDD scenarios in Gherkin for the checkout payment step."
B: ["TEST"] — "write BDD scenarios" → TEST (scenario is in TEST lexicon). "checkout payment" → no UI component. "Gherkin" → gap.
Edge: "scenario" / "scenarios" is in the TEST predicate lexicon. It fires TEST regardless of BDD framework names.

---

## E016-002 — "Given-When-Then" fires TEST via "scenarios" co-occurring with test verb

A: "Add Given-When-Then test cases for the registration form."
B: ["TEST", "UI"] — "add test cases" → TEST; "registration form" → UI. "Given-When-Then" is BDD vocabulary — it is context for TEST, but the predicate fires on "test cases" not on "Given-When-Then" itself.
Edge: "Given-When-Then" does not independently fire TEST. The firing comes from "test cases" or "scenarios".

---

## E016-003 — gap: "describe" in BDD sense is not TEST

A: "Describe the acceptance criteria for the checkout feature."
B: [] — "describe" → gap; "acceptance criteria" → gap (no predicate fires on this alone); "checkout feature" → gap. Zero predicates.
Edge: In BDD context, "describe" might seem like a test verb, but it is gap vocabulary in B39.

---

## E016-004 — boundary: "acceptance test" fires TEST (compound noun)

A: "Write acceptance tests for the shopping cart checkout flow."
B: ["TEST"] — "write acceptance tests" → TEST ("acceptance test" is a TEST compound). "shopping cart checkout flow" → no UI component fires here.
Edge: "acceptance test" is recognized as a TEST compound. "shopping cart" is not in UI lexicon.

---

## E016-005 — boundary: "Cucumber" and "Gherkin" are both gap

A: "Implement Cucumber step definitions for the Gherkin checkout scenarios."
B: ["TEST", "IMPLEMENT"] — "implement" → IMPLEMENT; "checkout scenarios" → "scenarios" → TEST; "Cucumber" → gap; "Gherkin" → gap.
Edge: Both Cucumber (tool name) and Gherkin (language name) are gap. Predicate fires on "implement" (IMPLEMENT) and "scenarios" (TEST).
CompiledPredicateIds: ["TEST", "IMPLEMENT"]
