# Layer 3 — Teaching Unit Pairs (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU002-001 — TEST fires on test-writing verbs (not execution)

A: "Write E2E tests for the login form." → ["TEST"] ✓
A: "Run E2E tests for the login form." → [] ✗ (gap — "run" is operational)
Teaching: TEST requires test-writing verbs. "run", "execute", "trigger" are operational gap.

---

## TU002-002 — UI fires on component vocabulary

A: "Fix the button hover state." → ["UI"] ✓
A: "Fix the API response time." → [] ✗ (gap — no UI vocabulary)
Teaching: UI predicate fires on UI element names and interaction states. Backend/API vocabulary does not fire UI.

---

## TU002-003 — QA fires on abbreviation only

A: "QA sign-off before merge." → ["QA"] ✓
A: "Quality assurance sign-off before merge." → [] ✗ (gap — spelled out)
Teaching: QA abbreviation gate is strict. Full form "quality assurance" is gap vocabulary.

---

## TU002-004 — TEST+UI: two predicates from two distinct triggers

A: "Write tests for the modal and fix the modal focus trap."
→ ["TEST", "UI"] — "write tests" → TEST; "modal focus trap" → UI.
Teaching: Two triggers from one request → two predicates. They are not merged into one.

---

## TU002-005 — "automate" is gap

A: "Automate the checkout form tests."
→ [] — "automate" is not a test-writing verb. Gap.
Teaching: TEST does not fire on "automate". The model must learn "automate" ≠ "write tests".
