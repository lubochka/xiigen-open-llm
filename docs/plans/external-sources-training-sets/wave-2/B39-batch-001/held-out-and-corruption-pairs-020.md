# Held-Out and Corruption Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H020-001 — Held-out: TEST+UI+QA — accessibility audit with QA gate

A: "Write accessibility audit tests for the modal, form inputs, and navigation, and QA sign-off on WCAG 2.1 AA."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write accessibility audit tests" → TEST; "modal/form inputs/navigation" → UI; "QA sign-off" → QA.

---

## H020-002 — Held-out: gap — "review accessibility report"

A: "Review the accessibility report from the last axe-core scan."
  Expected: []
  Held-out: "review" → gap; "accessibility report" → gap; "axe-core" → gap (tool name). Zero predicates.

---

## H020-003 — Held-out: TEST+UI — keyboard navigation on dropdown

A: "Validate that the dropdown component supports full keyboard navigation: arrow keys, Enter, Escape."
  Expected: ["TEST", "UI"]
  Held-out: "validate" → TEST; "dropdown component" → UI; "keyboard navigation" → UI (accessibility). Two UI triggers → one UI predicate.

---

## C020-001 — Corruption v6-02: "review" → QA (wrong)

A: "Review the accessibility report from the last axe-core scan."
  Correct: []
  Corrupt: ["QA"] ← v6-02 ("review" incorrectly treated as QA — model sees "review" as quality review)
  Teaching: "review" alone does not fire QA. The literal abbreviation "QA" must be present. "accessibility review" and "quality review" are gap.

---

## C020-002 — Corruption v6-01: ACCESSIBILITY predicate invented

A: "Write accessibility tests for the modal, form inputs, and navigation."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "ACCESSIBILITY"] ← v6-01 (ACCESSIBILITY invented as separate predicate)
  Teaching: Accessibility is a modifier for TEST and part of the UI lexicon. It does not create a separate ACCESSIBILITY predicate. TEST+UI are the correct predicates.
