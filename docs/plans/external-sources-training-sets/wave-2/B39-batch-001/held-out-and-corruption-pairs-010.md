# Held-Out and Corruption Pairs (010)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H010-001 — Held-out: UI — "popover" component

A: "The user avatar popover does not close on scroll."
  Expected: ["UI"]
  Held-out: "user avatar popover" → UI (component vocabulary).

---

## H010-002 — Held-out: IMPLEMENT+TEST+UI+QA — complete cycle

A: "Build the filter chip component, write chip selection tests, fix chip deselect animation, QA sign-off."
  Expected: ["IMPLEMENT", "TEST", "UI", "QA"]
  Held-out: Four distinct clauses → four predicates. TransitionType: feature_addition. QA last.

---

## H010-003 — Held-out: gap — "screenshot diff"

A: "Compare the visual screenshots before and after the CSS update."
  Expected: []
  Held-out: "compare screenshots" → observational/tool gap. No test-writing verb or QA abbreviation.

---

## C010-001 — Corruption: "quality assurance" → QA (wrong — spelled out)

A: "Get quality assurance sign-off on the UI changes."
  Correct: []
  Corrupt: ["QA"] ← wrong (QA predicate fires on "quality assurance" spelled out)
  Teaching: "quality assurance" spelled out → gap. QA predicate fires ONLY on abbreviation "QA".

---

## C010-002 — Corruption: IMPLEMENT→UI route → IMPLEMENT fires UIInteractionFlow15 (wrong)

A: ["IMPLEMENT", "UI"]
  Correct: ImplementationFlow41 (4.1) + UIInteractionFlow15 (1.5) — two separate flows
  Corrupt: ImplementationFlow41 (1.5) ← wrong (IMPLEMENT incorrectly routes to 1.5)
  Teaching: IMPLEMENT routes to 4.1 (ImplementationFlow41). UI routes to 1.5 (UIInteractionFlow15). Independent routing.
