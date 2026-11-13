# Held-Out and Corruption Pairs
# Batch: B39-batch-001
# Scenario: B39 — QA UI automation, sub-phases 5.3/7.3/1.5
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "validate" + UI terms → TEST + UI

A: "Validate the responsive layout of the product listing page across breakpoints."
Expected: TEST fires on "validate". UI fires on "layout" + "page". IMPLEMENT? "validate" alone
does not fire IMPLEMENT. CompiledPredicateIds: ["TEST", "UI"]. Routing: TEST → 5.3, UI → 1.5.
"responsive" also fires UI. CapabilityGapWarnings: gap-testing-automation-001 (breakpoints).

---

## HO-002 — QA + UI + TEST triple: regression QA on UI components

A: "QA and verify the modal and input component behavior for regression."
Expected: QA fires on "QA". TEST fires on "verify". UI fires on "modal" + "input" + "component".
CompiledPredicateIds: ["QA", "TEST", "UI"]. Three routing decisions: QA → 7.3, TEST → 5.3, UI → 1.5.

---

## HO-003 — "regression" alone is gap; "regression testing" fires TEST

A: "Run regression on the checkout flow."
Expected: "regression" alone is gap. "run" is gap. No "test/testing" in the sentence.
Zero predicates → knowledge_edge. Gap ID: gap-testing-automation-001.
Contrast: "Run regression testing on the checkout flow" → TEST fires on "testing".

---

## COR-001 — v6-01: UI suppressed for "button" in testing context

CORRUPT: "Write tests that click the place-order button and verify the result." → CompiledPredicateIds: ["TEST"].
Why corrupt: "button" fires UI predicate. "write" fires IMPLEMENT. The corrupt model suppressed
UI because the sentence is about testing, not about designing a button. But the predicate
fires on vocabulary, not on intent. Correct: CompiledPredicateIds: ["TEST", "UI", "IMPLEMENT"].

---

## COR-002 — v6-01: IMPLEMENT invented for "automate"

CORRUPT: "Automate the UI test suite for the checkout page." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: "automate" is gap vocabulary — no predicate. The corrupt model invented IMPLEMENT
because automating sounds like building. "test suite" → TEST fires. "page" → UI fires.
Correct: CompiledPredicateIds: ["TEST", "UI"] — not ["IMPLEMENT"].

---

## COR-003 — v6-03: UI routes to 5.3 instead of 1.5 when in testing context

CORRUPT: "Write tests for the checkout form." → UI → 5.3 (collapsed with TEST routing).
Why corrupt: UI predicate always routes to 1.5, regardless of surrounding test context.
The corrupt model merged UI into TEST's 5.3 routing. Correct: TEST → 5.3, UI → 1.5 —
two independent routing decisions to two separate sub-phases. They are never merged.
