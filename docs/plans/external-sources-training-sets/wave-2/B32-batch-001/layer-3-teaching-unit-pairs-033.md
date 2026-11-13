# Layer 3 — Teaching Unit Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-033-001 — Teaching: negation never fires a predicate

**Teaching unit:** Negated vocabulary does not fire predicates

"No tests needed", "skip the tests", "don't bother with tests" → TEST does NOT fire.

This applies to all predicates. Negated vocabulary is the opposite of a predicate signal.

**Anti-pattern (v6-02 variant):** Firing TEST because "tests" appears in the request, even when negated.

---

## B32-L3-033-002 — Teaching: gap vocabulary + negation still fires SCL

**Teaching unit:** Gap vocabulary drives SCL independently of negation

"Good enough — no tests needed." → gap vocabulary present → SCL emits.

SCL is driven by gap vocabulary being present, not by any predicate firing. Negation of a predicate does not cancel SCL when gap vocabulary is present.

---

## B32-L3-033-003 — Teaching: "no tests needed" alone (no gap vocabulary) → no SCL

**Teaching unit:** SCL only emits when gap vocabulary is present

"No tests needed — just ship it." → no gap vocabulary → SCL does NOT emit.

SCL is a gap vocabulary signal, not a negation signal. Without gap vocabulary, no gap-related action is taken.

---

## B32-L3-033-004 — Teaching: positive action verb fires even alongside negations

**Teaching unit:** Positive predicates fire regardless of co-present negations

"Lean sprint — implement the feature, don't bother with tests."
- "implement" → IMPLEMENT fires (positive imperative)
- "don't bother with tests" → TEST does not fire (negation)
- Result: IMPLEMENT fires; TEST does not

Negations only suppress their own predicate; they do not suppress positive predicates nearby.

---
