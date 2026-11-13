# Held-Out and Corruption Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H013-001 — Held-out: TEST+UI — Cypress e2e for checkout button

A: "Write Cypress e2e tests for the shopping cart add-to-cart button flow."
  Expected: ["TEST", "UI"]
  Held-out: "write e2e tests" → TEST; "cart button" → UI. "Cypress" → gap (framework name).

---

## H013-002 — Held-out: gap — "run Cypress against staging"

A: "Run the Cypress suite against the staging checkout page."
  Expected: []
  Held-out: "run" is operational gap; "Cypress" is gap; "staging" is gap. Zero predicates → knowledge_edge.

---

## H013-003 — Held-out: TEST — "assert" fires TEST in Cypress context

A: "Assert that the Cypress test for the header navigation passes on all breakpoints."
  Expected: ["TEST", "UI"]
  Held-out: "assert" → TEST; "header navigation" → UI. "Cypress test" → framework name is gap but "assert" fires TEST independently.

---

## C013-001 — Corruption v6-02: "e2e" alone incorrectly mapped to TEST

A: "The e2e workflow covers the checkout, payment, and confirmation steps."
  Correct: []
  Corrupt: ["TEST"] ← v6-02 (incorrectly treating "e2e" as a TEST trigger without a test-writing verb)
  Teaching: "e2e" in a descriptive sentence without a test-writing verb is gap. TEST fires on the verb+noun compound, not on "e2e" alone.

---

## C013-002 — Corruption v6-01: IMPLEMENT invented for "set up"

A: "Set up the Cypress project folder structure."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-01 ("set up" incorrectly mapped to IMPLEMENT)
  Teaching: "set up" is gap. IMPLEMENT lexicon does not include "set up". The corrupt model treated setup as building/creating.
