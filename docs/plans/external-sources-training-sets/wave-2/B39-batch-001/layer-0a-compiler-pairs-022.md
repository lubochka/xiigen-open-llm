# Layer 0-A — Compiler Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-022-001 — TEST alone: form validation e2e — pure TEST

A: "Write end-to-end test cases covering every validation rule on the contact form: required fields, format checks, and character limits."
B: ["TEST"] — "write end-to-end test cases" → TEST; "contact form" → UI (form). "form" fires UI.
Correction: CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-022-002 — TEST+UI: assert inline error on email input

A: "Assert that submitting the contact form with an invalid email input shows the inline validation error message."
B: ["TEST", "UI"] — "assert" → TEST; "contact form" → UI (form); "email input" → UI (input); "inline validation error message" → UI (visual state feedback).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-022-003 — gap: "document form validation rules"

A: "Document the form validation rules for the developer handbook."
B: [] — "document" → DOCUMENT predicate (not TEST, UI, or QA). Neither TEST nor UI nor QA fires.
CompiledPredicateIds: ["DOCUMENT"]
Note: DOCUMENT is a valid predicate but is outside the TEST/UI/QA triad focus of this batch.

---

## C0A-022-004 — TEST+UI: spec for password strength meter

A: "Add spec coverage for the password strength meter: weak, medium, strong states update the indicator bar color and aria-live region."
B: ["TEST", "UI"] — "add spec coverage" → TEST; "password strength meter" → UI (component); "indicator bar" → UI (visual); "aria-live region" → UI (accessibility).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-022-005 — TEST+UI+QA: full form e2e suite + QA gate

A: "Create end-to-end test scenarios for every validation rule on the registration form and get QA acceptance before the feature ships."
B: ["TEST", "UI", "QA"] — "create end-to-end test scenarios" → TEST; "registration form" → UI (form); "QA acceptance" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
