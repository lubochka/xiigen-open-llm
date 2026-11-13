# Layer 3 — Teaching Unit Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU022-001 — "validation rule" is gap; "form" fires UI

A: "Write test cases covering every validation rule on the contact form." What fires?
B: TEST fires on "write test cases". UI fires on "contact form" (form). "validation rule" → gap (documentation vocabulary). "every" → gap.
Result: ["TEST", "UI"]. Teaching: "validation rule" does not fire TEST or any predicate. The firing comes from "write test cases" (TEST) and "contact form" (UI).

---

## TU022-002 — "inline error" fires UI as visual state vocabulary

A: "Assert that the contact form shows the inline validation error on invalid email input." What fires?
B: TEST fires on "assert". UI fires on "contact form" (form), "email input" (input), and "inline validation error" (visual state feedback vocabulary → UI).
Result: ["TEST", "UI"]. Teaching: Visual state feedback terms ("inline error", "error message", "validation feedback", "error banner") are in the UI visual vocabulary.

---

## TU022-003 — "document" fires DOCUMENT (not TEST, UI, or QA); but "form" still fires UI

A: "Document the form validation rules for the developer handbook." What fires?
B: DOCUMENT fires on "document" (documentation verb). UI fires on "form" (form → UI). Neither TEST nor QA fires.
Result: ["DOCUMENT", "UI"]. Teaching: DOCUMENT is a valid but separate predicate. Even in a documentation request, UI vocabulary fires UI independently. The two predicates are independent.

---

## TU022-004 — "aria-live region" fires UI

A: "Add spec coverage for the password strength meter: indicator bar color and aria-live region updates." What fires?
B: TEST fires on "add spec coverage". UI fires on "password strength meter" (component), "indicator bar" (visual), "aria-live region" (accessibility ARIA vocabulary).
Result: ["TEST", "UI"]. Teaching: "aria-live region" fires UI as ARIA accessibility vocabulary. Any aria-* attribute name fires UI.

---

## TU022-005 — "QA acceptance" fires QA

A: "Create test scenarios for the registration form and get QA acceptance." Does QA fire?
B: Yes. "get QA acceptance" → QA fires on "QA". "create test scenarios" → TEST. "registration form" → UI.
Result: ["TEST", "UI", "QA"]. Teaching: "QA acceptance" fires QA because the abbreviation "QA" is literally present. "acceptance" alone is gap. "QA" in any context (QA acceptance, QA sign-off, QA gate, QA review, QA the results) fires the QA predicate.
