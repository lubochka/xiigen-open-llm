# Layer 3 — Teaching Unit Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU017-001 — "snapshot assertions" fires TEST; "assert" fires TEST

A: "Create snapshot assertions for every modal variant." Does TEST fire?
B: Yes. "create snapshot assertions" → TEST fires on "assertions" (test noun) + "create" (test-writing verb). "modal variant" → UI.
Result: ["TEST", "UI"]. Teaching: "assertions" is a TEST noun. "snapshot assertions" is a compound TEST noun.

---

## TU017-002 — "build" fires IMPLEMENT; "create assertions" fires TEST in the same sentence

A: "Build the screenshot test harness and create snapshot assertions for every modal." What predicates fire?
B: IMPLEMENT fires on "build". TEST fires on "create snapshot assertions". UI fires on "modal".
Result: ["TEST", "UI", "IMPLEMENT"]. TransitionType: feature_addition (IMPLEMENT present). Teaching: Two different predicates from two different clauses in one sentence — both fire independently.

---

## TU017-003 — "update snapshots" is gap for TEST; "layout" fires UI alone

A: "Update all snapshots after the spacing token redesign." What predicates fire?
B: TEST does not fire — "update" is gap. IMPLEMENT does not fire — "update" is gap. Zero predicates: [] — unless "spacing token" fires something. "spacing token" → gap. "redesign" → gap.
Result: []. Teaching: "update" is the most common IMPLEMENT false-positive in B39. It consistently does not fire IMPLEMENT or TEST.

---

## TU017-004 — "pagination" is in UI lexicon

A: "Assert that the pagination component snapshot matches." What predicates fire?
B: TEST fires on "assert". UI fires on "pagination component" ("pagination" and "component" both fire UI independently).
Result: ["TEST", "UI"]. Teaching: "pagination" is confirmed in the UI lexicon. It is one of the navigation/list UI components.

---

## TU017-005 — "tooltip" is in UI lexicon

A: "Create snapshot tests for each tooltip variant." What predicates fire?
B: TEST fires on "create snapshot tests". UI fires on "tooltip variant" ("tooltip" is in the UI component vocabulary).
Result: ["TEST", "UI"]. Teaching: "tooltip" fires UI. UI vocabulary includes: tooltip, popover, snackbar, toast, badge, chip — all UI overlay/feedback components.
