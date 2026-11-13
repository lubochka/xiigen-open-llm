# Held-Out and Corruption Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H022-001 — Held-out: TEST+UI — form validation E2E test cases

A: "Write end-to-end test cases covering every validation rule on the contact form."
  Expected: ["TEST", "UI"]
  Held-out: "write end-to-end test cases" → TEST; "contact form" → UI (form). "validation rule" → gap.

---

## H022-002 — Held-out: DOCUMENT+UI — document form validation rules

A: "Document the form validation rules for the developer handbook."
  Expected: ["DOCUMENT", "UI"]
  Held-out: "document" → DOCUMENT; "form" → UI (form fires). Neither TEST nor QA fires.

---

## H022-003 — Held-out: TEST+UI+QA — full form e2e + QA acceptance

A: "Create end-to-end test scenarios for every validation rule on the registration form and get QA acceptance."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "create end-to-end test scenarios" → TEST; "registration form" → UI; "QA acceptance" → QA.

---

## C022-001 — Corruption v6-02: "validate" (in "validation rule") → TEST (wrong context)

A: "Document the form validation rules."
  Correct: ["DOCUMENT", "UI"]
  Corrupt: ["DOCUMENT", "UI", "TEST"] ← v6-02 ("validation" in "validation rule" incorrectly triggers TEST)
  Teaching: "validation" fires TEST only when used as a test action verb ("validate the form") or in "validation tests" compounds. "validation rule" is a documentation term — it is gap for TEST. Context matters: "validate [something]" → TEST; "validation rule" → documentation concept → DOCUMENT context.

---

## C022-002 — Corruption v6-03: UI collapses into TEST sub-phase for form tests

A: "Write E2E test cases for the contact form."
  Correct: TEST → 5.3, UI → 1.5 (separate)
  Corrupt: TEST+UI → 5.3 only (UI routing collapsed into TEST routing)
  Teaching: UI always routes independently to 1.5. Form vocabulary fires UI. UI never merges into TEST's 5.3 routing. The v6-03 corruption of collapsing UI into TEST is a critical error.
