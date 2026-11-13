# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E022-001 — "end-to-end test cases" fires TEST; "form" fires UI

A: "Write end-to-end test cases covering every validation rule on the contact form."
B: ["TEST", "UI"] — "write end-to-end test cases" → TEST; "contact form" → UI (form). "validation rule" → gap (rule documentation vocabulary, not a predicate trigger by itself).
Edge: "end-to-end test cases" is a multi-word TEST compound. "validation rule" does not independently fire TEST.

---

## E022-002 — "inline validation error message" fires UI via visual state vocabulary

A: "Assert that the contact form shows the inline validation error message on invalid email input."
B: ["TEST", "UI"] — "assert" → TEST; "contact form" → UI (form); "email input" → UI (input); "inline validation error message" → UI (visual feedback state).
Edge: "inline error", "error message", "validation feedback" are UI visual state vocabulary and fire UI.

---

## E022-003 — "document" fires DOCUMENT predicate; TEST/UI/QA do not fire

A: "Document the form validation rules for the developer handbook."
B: ["DOCUMENT"] — "document" → DOCUMENT. "form validation rules" → "form" fires UI? Yes: "form" → UI fires.
Correction: CompiledPredicateIds: ["DOCUMENT", "UI"] — "document" → DOCUMENT; "form" → UI.
Edge: Even in a documentation request, UI vocabulary in the sentence fires UI predicate independently.

---

## E022-004 — "aria-live region" fires UI; "spec coverage" fires TEST

A: "Add spec coverage for the password strength meter: weak/medium/strong states, indicator bar, aria-live region."
B: ["TEST", "UI"] — "add spec coverage" → TEST; "password strength meter" → UI (component); "indicator bar" → UI (visual); "aria-live region" → UI (accessibility).
Edge: "aria-live region" is ARIA vocabulary and fires UI. "indicator bar" is visual UI vocabulary and fires UI. Three UI triggers → one UI predicate (deduplication).

---

## E022-005 — "registration form" fires UI; "QA acceptance" fires QA

A: "Create end-to-end test scenarios for every validation rule on the registration form and get QA acceptance."
B: ["TEST", "UI", "QA"] — "create end-to-end test scenarios" → TEST; "registration form" → UI (form); "QA acceptance" → QA.
Edge: "QA acceptance" fires QA. "acceptance" alone is gap but "QA acceptance" triggers QA via the QA abbreviation.
