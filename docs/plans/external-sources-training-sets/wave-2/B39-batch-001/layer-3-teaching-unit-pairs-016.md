# Layer 3 — Teaching Unit Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU016-001 — "scenario" fires TEST in BDD (Behaviour-Driven Development) context

A: "Write BDD scenarios for the checkout payment step." Does TEST fire?
B: Yes. "write BDD scenarios" → TEST fires on "scenarios" (test noun) + "write" (test-writing verb). "BDD" is context vocabulary; the predicate fires on "scenarios". "checkout payment" → no UI component vocabulary.
Result: ["TEST"]. Teaching: "scenario"/"scenarios" is in the TEST lexicon. BDD and Gherkin tool names are gap.

---

## TU016-002 — "describe" (in BDD sense) does not fire TEST

A: "Describe the acceptance criteria for the checkout feature." Does TEST fire?
B: No. "describe" is gap vocabulary. "acceptance criteria" → gap. Zero predicates.
Teaching: In BDD, "describe" sounds like a test verb, but it is gap in B39. Test-writing verbs are: write, create, add, assert, validate, verify. Not: describe, define, explain, outline.

---

## TU016-003 — "acceptance test" compound fires TEST

A: "Write acceptance tests for the shopping cart." Does TEST fire?
B: Yes. "write acceptance tests" → TEST fires on the compound "acceptance tests". "shopping cart" → not in UI lexicon → no UI.
Result: ["TEST"]. Teaching: "acceptance tests" and "acceptance test scenarios" are TEST compounds.

---

## TU016-004 — Cucumber + Gherkin: both gap; predicates fire on generic vocabulary

A: "Implement Cucumber step definitions for the Gherkin checkout scenarios." What predicates fire?
B: IMPLEMENT fires on "implement". TEST fires on "scenarios". Cucumber → gap. Gherkin → gap.
Result: ["TEST", "IMPLEMENT"]. Routing: TEST → 5.3, IMPLEMENT → 4.1. Teaching: Tool/language names never fire predicates. Predicates fire on the generic verbs and nouns.

---

## TU016-005 — "Given-When-Then" does not independently fire TEST

A: "The Given-When-Then format is used for checkout BDD tests." Does TEST fire?
B: "tests" in "BDD tests" → TEST fires? "The ... format is used for ... tests" — "tests" here is a descriptive noun in a passive sentence without a test-writing verb. In B39, TEST fires when a test-writing action verb is present.
Conclusion: If no test-writing verb → no TEST. CompiledPredicateIds: []. Teaching: "Given-When-Then" does not fire TEST independently. TEST requires: "write/create/add scenarios", "write/create/add tests", "assert", "validate".
