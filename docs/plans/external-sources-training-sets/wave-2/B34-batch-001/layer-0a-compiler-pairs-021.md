# Layer 0-A — Compiler Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-021-001 — "walk through integrating" → API_CONTRACT fires

A: "Walk through integrating the payment API with our checkout service."
Analysis: "walk through" = guidance frame; "integrating the payment API" = embedded API action → API_CONTRACT fires. "integrate" always fires API_CONTRACT.
B: Predicates: ["API_CONTRACT"] | TransitionType: documentation
Evidence: "integrate" + "API" → API_CONTRACT; "walk through" = gap warning alongside API_CONTRACT. Embedded verb rule: API_CONTRACT fires.

---

## B34-compiler-021-002 — "guide through integrating" → API_CONTRACT fires

A: "Guide the team through integrating the Stripe payment gateway into the checkout flow."
Analysis: "guide through" = guidance frame; "integrating the Stripe payment gateway" = API action → API_CONTRACT fires.
B: Predicates: ["API_CONTRACT"] | TransitionType: documentation
Evidence: "integrate" always fires API_CONTRACT; "guide through" = gap warning.

---

## B34-compiler-021-003 — "walk the team through the API" → gap (no integrate verb)

A: "Walk the team through how the third-party shipping API works."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "walk through how the API works" = explain/describe vocabulary → gap. No "integrate" verb present. API_CONTRACT requires "integrate/connect" as the action verb, not description of the API.

---

## B34-compiler-021-004 — "show how to integrate" → API_CONTRACT fires

A: "Show the developers how to integrate the OAuth2 (Open Authorization 2.0) provider with our user service."
Analysis: "show" = guidance frame; "integrate the OAuth2 provider" = embedded API action → API_CONTRACT fires.
B: Predicates: ["API_CONTRACT"] | TransitionType: documentation
Evidence: Embedded "integrate" fires API_CONTRACT; "show" = gap warning. Embedded verb rule: "integrate" always fires in any frame.

---

## B34-compiler-021-005 — v6 corruption: "walk through integrating" collapsed to gap

v6 corrupt: "Walk through integrating the SSO (Single Sign-On) with our platform" → Predicates: []
Correct: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03. "integrate" always fires API_CONTRACT, even inside walk-through guidance frame. Gap warning alongside API_CONTRACT.
Fix: Predicates: ["API_CONTRACT"] | GapWarning.
