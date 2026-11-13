# Layer 3 — Teaching Unit Pairs (031)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU031-001 — SearchFlowTest: "scan" is gap

A: What does SearchFlowTest teach about the verb "scan"?
B: "scan" is explicitly gap — does not fire TEST, IMPLEMENT, or QA. "Scan the search results for duplicate entries" → zero predicates if no UI vocabulary is also present. "scan" is a monitoring/analysis verb in the gap category alongside: run, check, execute, automate, monitor, profile, configure, describe.

---

## TU031-002 — SearchFlowTest: autocomplete dropdown fires UI

A: "Assert the search autocomplete dropdown shows top 5 results." UI fires on which token?
B: "autocomplete dropdown" → "dropdown" is in the UI lexicon → fires UI. "autocomplete" alone is not a UI lexicon word. The "dropdown" token carries the UI trigger. Combined with "assert" (TEST): [TEST, UI].

---

## TU031-003 — SearchFlowTest: search input fires UI

A: Does "search input" fire UI?
B: Yes. "input" is in the UI lexicon. "search input", "text input", "password input" all fire UI via the "input" token.

---

## TU031-004 — SearchFlowTest: no-results screen fires UI

A: Does "no-results screen" fire UI?
B: Yes. "screen" is in the UI lexicon → fires UI. Compound noun "no-results screen" fires UI via the "screen" token.

---

## TU031-005 — SearchFlowTest: "run search tests" → gap + UI

A: "Run search tests and verify the results grid renders." Predicate lesson?
B: "run" is gap → TEST does not fire. "verify" fires TEST. "results grid" — "grid" is not in the UI lexicon, but if "results screen" or "results page" were present, UI would fire. In this exact request: TEST fires (verify), UI may not fire unless explicit UI vocabulary is present. "results grid" alone = gap for UI. Result: [TEST] only. TransitionType: documentation.
