# Layer 3 — Teaching Unit Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU015-001 — "visual regression tests" is a TEST compound

A: "Write visual regression tests for the component library buttons and modals." What predicates fire?
B: TEST fires on "write visual regression tests" ("visual regression tests" is a recognized multi-word TEST compound). UI fires on "buttons" and "modals".
Result: ["TEST", "UI"]. Teaching: Multi-word TEST compounds can span several words. "visual regression tests" is one TEST trigger.

---

## TU015-002 — "snapshot tests" fires TEST; "snapshot" alone with gap verb does not

A: Compare: "Create snapshot tests for the product card" vs "Update the product card snapshot."
B: "Create snapshot tests" → ["TEST", "UI"]. "Update the product card snapshot" → ["UI"] (update is gap; "snapshot" alone with gap verb → no TEST; "product card" → UI).
Teaching: The test-writing verb is required. "update snapshot" = gap. "create snapshot tests" = TEST fire.

---

## TU015-003 — "quality check" does not fire QA

A: "Run a quality check on the visual regression baseline approvals." Does QA fire?
B: No. "quality check" → gap. QA fires ONLY on the abbreviation "QA". "run" → gap. Zero predicates.
Teaching: The most common false-positive in B39 is "quality check" or "quality review" — neither fires QA. The literal two letters "QA" must appear.

---

## TU015-004 — "QA the baselines" fires QA

A: "Assert that all visual regression test baselines pass QA review." Does QA fire?
B: Yes. "QA review" contains the abbreviation "QA" → QA fires. "assert" → TEST. No UI component vocabulary in this sentence.
Result: ["TEST", "QA"]. Teaching: "QA review" — the abbreviation "QA" is present → QA fires.

---

## TU015-005 — routing: visual regression tests go to 5.3 not 5.1

A: "Write visual regression tests for the button component." Does this route to 5.1 or 5.3?
B: 5.3. "visual regression tests" → E2E/UI testing context → 5.3. "button component" → UI → 1.5.
Teaching: Visual regression testing is a 5.3 domain (End-to-End and UI Testing). 5.1 is for unit/integration tests. Visual + UI + snapshot → always 5.3.
