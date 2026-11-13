# Layer 0-C — Knowledge Edge Pairs (025)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E025-001 — CheckoutFlowTest: gap vocabulary

A: Which words in a checkout test request are gap and do not fire any predicate?
B: Gap vocabulary in checkout context: "automate" (gap — not a test-writing verb), "Cypress" (framework name — gap), "run the checkout tests" ("run" is gap), "execute the flow" ("execute" is gap), "trigger payment" ("trigger" is gap), "schedule checkout tests" ("schedule" is gap). Key rule: "checkout flow" alone does not fire any predicate — it is a domain descriptor, not a predicate trigger.

---

## E025-002 — CheckoutFlowTest: boundary — "validate" fires TEST

A: Does "validate the checkout payment form" fire TEST?
B: Yes. "validate" is a standalone test assertion verb → fires TEST. "payment form" → "form" fires UI. Result: [TEST, UI]. Contrast: "validate" in pure non-test context (e.g., "validate the schema definition") is ambiguous — in B39 E2E context, validate always fires TEST when associated with a UI component or flow.

---

## E025-003 — CheckoutFlowTest: boundary — "checkout" alone is gap

A: Does "Test the checkout." fire any predicate?
B: Depends on interpretation: "Test" as a noun (the noun "Test") does not fire TEST — TEST fires on test-writing verbs and test nouns. "Test the checkout" could read as "run the test" (gap) or as a request to write a test. In B39 context: if "test" is used as a verb in the imperative ("Test the checkout") without a test noun, it is ambiguous. Safe classification: "Test" as an imperative verb without a test noun = gap. No predicates fire. Result: [] → knowledge_edge.

---

## E025-004 — CheckoutFlowTest: boundary — "payment button" fires UI

A: "Run tests for the payment button." Does UI fire even though TEST does not?
B: Yes. UI fires on "payment button" → "button" is in the UI lexicon → UI fires → routes to 1.5. TEST does not fire because "run" is a gap verb. Result: [UI]. Single sub-phase: 1.5. This is a common pattern: gap verb + UI vocabulary → UI fires, TEST does not.

---

## E025-005 — CheckoutFlowTest: synthesis — "order summary modal" fires UI

A: What predicate does "assert the order summary modal" fire?
B: Two predicates: TEST ("assert" → standalone test assertion verb → TEST fires) + UI ("order summary modal" → "modal" fires UI). Result: [TEST, UI]. This is the canonical assert + UI component pattern that produces [TEST, UI] routing to [5.3, 1.5].
