# Layer 0-C — Knowledge Edge Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E017-001 — "snapshot test" fires TEST; "baseline" alone does not

A: "Add snapshot tests for the button, input, and dropdown components."
B: ["TEST", "UI"] — "add snapshot tests" → TEST; "button", "input", "dropdown" → UI.
Edge: "snapshot tests" is a TEST compound. "baseline" alone (without a test verb) does not fire TEST.

---

## E017-002 — "build" fires IMPLEMENT; "assertions" fires TEST independently

A: "Build the screenshot test harness and create snapshot assertions for every modal variant."
B: ["TEST", "UI", "IMPLEMENT"] — "build" → IMPLEMENT; "create snapshot assertions" → TEST; "modal variant" → UI (modal).
Edge: "build" and "create" in the same sentence fire IMPLEMENT and TEST respectively. Both predicates fire independently.

---

## E017-003 — "update" is gap even in snapshot context

A: "Update all snapshots after the spacing token redesign."
B: [] — "update" → gap; "snapshots" with gap verb → no TEST; no UI component vocabulary. Zero predicates.
Edge: "update" is consistently gap in B39. No predicate fires on "update [noun]" patterns.
Correction: "spacing token redesign" → "token" → gap; but does "spacing" fire UI? Not in the UI lexicon. Zero predicates confirmed.
CompiledPredicateIds: []

---

## E017-004 — boundary: "pagination" is in UI lexicon

A: "Assert that the pagination component snapshot matches the approved baseline."
B: ["TEST", "UI"] — "assert" → TEST; "pagination component" → UI (pagination is in UI lexicon). "snapshot" → context for TEST, not a standalone trigger.
Edge: "pagination" is confirmed in the UI lexicon.

---

## E017-005 — boundary: "tooltip" is in UI lexicon

A: "Create visual snapshot tests for each tooltip variant."
B: ["TEST", "UI"] — "create snapshot tests" → TEST; "tooltip variant" → UI (tooltip is in UI lexicon).
Edge: "tooltip" fires UI predicate. This confirms tooltip is in the UI component vocabulary list.
